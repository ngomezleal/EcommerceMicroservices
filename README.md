# EcommerceMicroservices

Solución de microservicios construida con .NET 8 para la gestión de productos y órdenes. El proyecto aplica una arquitectura limpia por servicio, separa las responsabilidades mediante CQRS y mantiene la persistencia de cada contexto en una base de datos independiente.

## Visión general

La solución está compuesta por dos servicios:

- **ProductService**: administra el catálogo de productos. Expone operaciones CRUD y consulta paginada de productos.
- **OrderService**: permite crear y consultar órdenes, además de actualizar su estado. Para validar la información de productos, se comunica con `ProductService` mediante un cliente HTTP.

Cada microservicio mantiene su propio contexto de datos: `product_db` para productos y `order_db` para órdenes. Ambas bases se inicializan en la instancia PostgreSQL definida por Docker Compose.

## Arquitectura y tecnologías

Cada servicio está organizado siguiendo **Clean Architecture**:

- **Domain**: entidades de dominio, value objects y contratos de repositorio, sin dependencias de infraestructura.
- **Application**: comandos, consultas, DTOs, validadores y handlers de MediatR que implementan CQRS.
- **Infrastructure**: `DbContext`, repositorios, migraciones de Entity Framework Core e integraciones externas.
- **Api**: controllers HTTP, middleware global, health checks y configuración de inyección de dependencias.

La solución utiliza DDD básico para modelar los agregados y encapsular sus cambios de estado. El stack tecnológico incluye:

- .NET 8 y C# 12.
- Entity Framework Core 8.
- PostgreSQL 16.
- MediatR para CQRS.
- FluentValidation para validar las solicitudes.
- Docker y Docker Compose para ejecutar las APIs y PostgreSQL.

### Resiliencia entre servicios

El cliente HTTP de `OrderService` para `ProductService` está configurado con Polly:

- **Retry pattern**: hasta 3 reintentos, con espera exponencial de base de 2 segundos.
- **Circuit breaker**: se abre tras 3 fallos dentro de 30 segundos y permanece abierto durante 30 segundos.

## Ejecución local con Docker

Desde la raíz de la solución, elimina los contenedores y volúmenes anteriores y levanta el entorno:

```bash
docker compose down -v
docker compose up --build -d
```

Servicios expuestos:

| Servicio | Dirección local | Descripción |
| --- | --- | --- |
| `product-api` | `http://localhost:5001` | API de productos |
| `order-api` | `http://localhost:5002` | API de órdenes |
| PostgreSQL | `localhost:5432` | Motor de datos para `product_db` y `order_db` |

Dentro de la red de Docker, `OrderService` consume `ProductService` mediante `http://product-api:8080`.

## Pruebas unitarias y de integración

Ejecuta la suite de pruebas desde la raíz de la solución:

```bash
dotnet test
```

Las pruebas usan xUnit, Moq, FluentAssertions y el proveedor InMemory de Entity Framework Core cuando corresponde.

## Endpoints principales

### Productos

Base URL: `http://localhost:5001`

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/api/products?page=1&pageSize=10` | Obtiene productos paginados. |
| `GET` | `/api/products/{id}` | Obtiene un producto por identificador. |
| `POST` | `/api/products` | Crea un producto. |
| `PUT` | `/api/products/{id}` | Actualiza un producto existente. |
| `DELETE` | `/api/products/{id}` | Elimina un producto. |

### Órdenes

Base URL: `http://localhost:5002`

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/api/orders` | Obtiene las órdenes. |
| `GET` | `/api/orders/{id}` | Obtiene una orden por identificador. |
| `POST` | `/api/orders` | Crea una orden. |
| `PUT` | `/api/orders/{id}/status` | Actualiza el estado de una orden. |

## Manejo de errores

Las APIs centralizan el manejo de excepciones en un middleware global y responden usando el estándar [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807) mediante `ProblemDetails`. Las respuestas de error incluyen información como el código de estado, título, detalle e instancia de la solicitud.

| Código | Situación |
| --- | --- |
| `400 Bad Request` | Fallos de validación de FluentValidation. |
| `404 Not Found` | El recurso solicitado no existe. |
| `500 Internal Server Error` | Excepciones no controladas. |
