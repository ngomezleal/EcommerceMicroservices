# Respuestas Teóricas: Arquitectura de EcommerceMicroservices

Este documento fundamenta las decisiones arquitectónicas presentes en la solución `EcommerceMicroservices` y describe una evolución controlada hacia una arquitectura orientada a eventos. El análisis se ajusta a la implementación actual de `ProductService` y `OrderService`; las capacidades indicadas como evolución constituyen una propuesta, no funcionalidades ya incorporadas.

## 1. Arquitectura y microservicios

### 1.1. Base de datos por servicio

La solución aplica el patrón **Database per Service** mediante dos contextos de persistencia independientes: `product_db`, propiedad de `ProductService`, y `order_db`, propiedad de `OrderService`. Aunque ambos se alojan actualmente en la misma instancia de PostgreSQL para simplificar el entorno local, cada servicio se conecta a su propia base de datos, administra su propio `DbContext`, sus migraciones y sus tablas. Por tanto, `Products` pertenece exclusivamente al contexto de productos, mientras que `Orders` y `OrderItems` pertenecen al contexto de órdenes.

Esta separación establece la propiedad explícita de los datos por *bounded context*. `OrderService` no consulta tablas de productos ni depende de claves foráneas, esquemas o modelos de Entity Framework administrados por `ProductService`; valida los productos a través de su contrato HTTP. Del mismo modo, cambios en el esquema de órdenes no exigen modificar la persistencia del catálogo. El contrato entre servicios se limita a la interfaz expuesta, lo cual evita el acoplamiento implícito que produciría una base de datos compartida.

Los beneficios principales son los siguientes:

- **Menor acoplamiento de datos:** cada equipo puede evolucionar su modelo, índices, migraciones y estrategia de persistencia sin coordinar cambios internos con el otro servicio.
- **Escalabilidad independiente:** el catálogo y la gestión de órdenes presentan patrones de carga distintos. Cada servicio puede optimizar o escalar su almacenamiento y sus réplicas según sus necesidades.
- **Aislamiento de fallos y despliegues:** un problema de migración, rendimiento o mantenimiento en `order_db` no requiere detener ni redeplegar `ProductService`; la indisponibilidad se limita al contexto afectado.
- **Mejor alineación del dominio:** los límites de datos refuerzan que producto y orden son responsabilidades de negocio diferentes, en lugar de componentes que comparten directamente un modelo relacional global.

Este patrón no elimina la necesidad de coordinación entre servicios: la consistencia entre contextos pasa a gestionarse mediante contratos de integración, comunicación resiliente y, en una evolución posterior, eventos. Esa es una decisión consciente para priorizar autonomía sobre transacciones distribuidas.

### 1.2. Clean Architecture y CQRS con MediatR

Cada microservicio se organiza en las capas `Domain`, `Application`, `Infrastructure` y `Api`. El dominio contiene entidades y contratos de repositorio sin dependencias de infraestructura. La capa de aplicación contiene comandos, consultas, DTOs, validadores e interfaces de integración. Infraestructura implementa `DbContext`, repositorios y clientes externos; API expone controladores, middleware y configuración de inyección de dependencias. Esta dirección de dependencias protege las reglas de negocio frente a detalles tecnológicos como PostgreSQL, Entity Framework Core o HTTP.

Sobre esta estructura se utiliza **CQRS** con MediatR. Los comandos representan operaciones que modifican estado y las consultas representan operaciones de lectura; ambos son `record` inmutables y se resuelven mediante *handlers* específicos. Por ejemplo, `CreateOrderCommandHandler` concentra la orquestación de crear una orden, mientras que los *handlers* de consulta recuperan las proyecciones requeridas. Los comportamientos de MediatR permiten aplicar validación transversal con FluentValidation sin repetirla en controladores.

Frente a una arquitectura tradicional de tres capas —presentación, negocio y datos—, esta combinación aporta una separación más precisa de responsabilidades y dependencias. En una implementación de tres capas es frecuente que la lógica de aplicación se concentre en servicios genéricos, que los controladores conozcan detalles de persistencia o que las operaciones de lectura y escritura compartan modelos y flujos innecesariamente. Clean Architecture evita que las decisiones de infraestructura condicionen el dominio; CQRS evita que el crecimiento de casos de uso convierta una única capa de negocio en un componente ambiguo.

El resultado es mayor testeabilidad y mantenibilidad: los *handlers* pueden verificarse con contratos simulados, los repositorios pueden cambiarse sin alterar casos de uso y las reglas del dominio permanecen encapsuladas. La estructura también favorece que preocupaciones transversales —validación, trazabilidad o autorización futura— se incorporen como *behaviors* sin contaminar cada endpoint.

## 2. Resiliencia y comunicación entre servicios

### 2.1. Comunicación HTTP actual y Polly

Al crear una orden, `OrderService` consulta sincrónicamente a `ProductService` mediante `IProductApiClient` para obtener cada producto solicitado y validar su existencia y disponibilidad de stock. El cliente tipado se registra a través de `HttpClientFactory` y dispone de un *resilience handler* de Polly. Esta configuración reconoce que una llamada de red puede fallar temporalmente por congestión, reinicios, indisponibilidad breve o errores HTTP recuperables.

La política de **Retry** está configurada con un máximo de tres reintentos, un retraso base de dos segundos y *backoff* exponencial, sin *jitter*. Ante un fallo transitorio, el cliente no devuelve el error de inmediato: vuelve a intentar la misma operación, incrementando la espera entre intentos. Esta estrategia permite absorber interrupciones breves sin que el usuario deba repetir manualmente la solicitud.

La política de **Circuit Breaker** usa una ventana de muestreo de 30 segundos, un mínimo de tres ejecuciones y una razón de fallos de 1. Cuando se alcanzan tres fallos dentro de dicha ventana y todos ellos fallan, el circuito se abre durante 30 segundos. Mientras está abierto, las nuevas llamadas fallan rápidamente en lugar de seguir enviando tráfico a un servicio posiblemente degradado. Al concluir el período de apertura, el mecanismo puede comprobar de nuevo si la dependencia se ha recuperado.

Polly no sustituye la validación funcional ni garantiza que el servicio remoto esté disponible. Su función es administrar de forma consistente la incertidumbre de las comunicaciones HTTP y proteger tanto a `OrderService` como a `ProductService` frente a reintentos descontrolados o cascadas de espera.

### 2.2. Retry Pattern frente a Circuit Breaker Pattern

El **Retry Pattern** está indicado para fallos que probablemente desaparezcan en un intervalo corto y cuyo reenvío sea seguro. Su objetivo es aumentar la probabilidad de completar una operación ante una perturbación puntual. El *backoff* exponencial reduce la presión sobre la dependencia y evita concentrar múltiples reintentos en el mismo instante. Debe aplicarse exclusivamente a operaciones idempotentes o protegidas frente a duplicados; una repetición automática de una escritura no idempotente puede crear efectos de negocio duplicados.

El **Circuit Breaker Pattern** responde a un fallo persistente o a una dependencia degradada. Su objetivo no es repetir la operación, sino detener temporalmente las llamadas que previsiblemente seguirán fallando, liberar recursos y permitir la recuperación del servicio dependiente. Además de la apertura del circuito, una implementación operativa debe observar este estado mediante métricas, registros y alertas.

En consecuencia, ambos patrones son complementarios. Retry resuelve una anomalía breve por solicitud; Circuit Breaker protege al sistema cuando la anomalía deja de ser breve. En el flujo actual, un reinicio corto de `ProductService` puede justificar los reintentos, mientras que una caída prolongada o una tasa sostenida de fallos debe abrir el circuito y evitar que la creación de órdenes agote conexiones e hilos esperando respuestas que no llegarán.

## 3. Evolución a una arquitectura orientada a eventos

### 3.1. Evolución propuesta

La comunicación actual establece acoplamiento temporal: `OrderService` necesita que `ProductService` responda durante la creación de una orden. Una evolución hacia **Event-Driven Architecture (EDA)** puede desacoplar ambos procesos utilizando un *message broker*. Para esta solución, se recomienda iniciar con **RabbitMQ o Azure Service Bus**, por su buen encaje con mensajería de integración basada en colas, enrutamiento y confirmaciones, y por su menor complejidad operativa en un escenario inicial. Apache Kafka sería una alternativa apropiada cuando se requiera retención prolongada de eventos, reproducción de flujos y consumo de alto volumen.

La transición debe ser incremental y mantener los límites de Clean Architecture. Los contratos de publicación y consumo se definirían como eventos de integración versionados, independientes de entidades y DTOs internos. Por ejemplo, una orden podría publicar un evento `OrderCreated` o `OrderSubmitted`; ProductService podría consumirlo para realizar una reserva o validación asíncrona de inventario y publicar eventos como `ProductStockReserved` o `ProductStockRejected`. OrderService consumiría la respuesta y actualizaría el estado de la orden a confirmado, rechazado o pendiente según corresponda. Los nombres exactos, el contenido y la semántica de los eventos deben formalizarse como contratos de integración antes de su implementación.

Para una entrega confiable, la publicación no debe depender de una doble escritura no coordinada entre la base de datos y el broker. El **patrón Outbox** persiste el cambio de negocio y el evento pendiente en la misma transacción local de la base de datos propietaria; un proceso publicador los entrega posteriormente al broker y registra su resultado. Los consumidores, por su parte, deben ser **idempotentes**: deben almacenar o reconocer un identificador de evento para que una entrega repetida no produzca una nueva reserva, transición de estado o efecto contable. La idempotencia y los reintentos asíncronos son necesarios porque los brokers suelen ofrecer entrega al menos una vez.

La migración puede comenzar publicando eventos sin retirar el flujo HTTP, observando su entrega y procesándolos en modo no crítico. Después, el flujo de órdenes puede aceptar la solicitud con un estado pendiente y completar la decisión mediante los eventos de inventario. Esta estrategia reduce el riesgo de cambiar simultáneamente la lógica de negocio, la persistencia y el mecanismo de integración.

### 3.2. Beneficios y consideraciones operativas

EDA reduce el acoplamiento temporal: el productor registra y publica un hecho sin bloquearse hasta que cada consumidor esté disponible. También permite que nuevos consumidores —por ejemplo, notificaciones, analítica o facturación— reaccionen a una orden sin modificar el productor. El procesamiento asíncrono facilita absorber picos mediante colas y escalar consumidores de forma independiente.

El costo de esa autonomía es la **consistencia eventual**. Durante un intervalo, una orden puede existir en estado pendiente mientras se confirma o rechaza la disponibilidad de inventario. La experiencia de usuario, las transiciones de estado y la comunicación de errores deben diseñarse para reflejarlo de forma explícita, en vez de simular una transacción distribuida.

La operación de una arquitectura orientada a eventos requiere además trazabilidad de extremo a extremo —por ejemplo, mediante identificadores de correlación—, observabilidad de colas y consumidores, estrategias de reintento y *dead-letter queues* para mensajes que no puedan procesarse. Estas medidas, junto con contratos versionados y compatibilidad hacia atrás, permiten evolucionar los servicios sin romper a los consumidores existentes.

## 4. Formato estándar de errores: RFC 7807

La adopción de **RFC 7807** mediante `ProblemDetails` establece un contrato de errores uniforme para todas las APIs del ecosistema. En la implementación actual, el middleware global de ambos servicios traduce excepciones a respuestas `application/problem+json` e incluye los campos `status`, `title`, `detail` e `instance`. Para fallos de FluentValidation, incorpora además la extensión `errors`, agrupada por propiedad.

Este contrato evita que cada controlador o microservicio defina formas de error incompatibles. Clientes web, aplicaciones móviles, gateways y otros servicios pueden interpretar de manera consistente el código HTTP y la información de diagnóstico sin depender de mensajes particulares. También centraliza el tratamiento de excepciones, reduciendo respuestas ad hoc y evitando que detalles internos de errores no controlados se expongan al consumidor.

La convención vigente se alinea con los siguientes resultados:

| Código HTTP | Situación | Respuesta estándar |
| --- | --- | --- |
| `400 Bad Request` | Fallos de FluentValidation o datos inválidos. | ProblemDetails con detalle y, cuando aplica, extensión `errors`. |
| `404 Not Found` | Recurso inexistente. | ProblemDetails que identifica el recurso o condición no encontrada. |
| `500 Internal Server Error` | Excepción no controlada. | ProblemDetails genérico; el detalle técnico queda registrado en los logs. |

En un entorno de microservicios, RFC 7807 favorece la interoperabilidad, simplifica la implementación de clientes y mejora la observabilidad al normalizar el contexto que se registra y propaga. Mantener este comportamiento en el middleware global asegura que nuevas rutas respeten el mismo contrato sin duplicar lógica de manejo de errores.
