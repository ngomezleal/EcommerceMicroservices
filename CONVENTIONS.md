# Convenciones del Proyecto - EcommerceMicroservices

Este documento establece las reglas estrictas de arquitectura, código, nomenclatura y patrones que deben seguirse en toda la solución.

---

## 1. Estructura y Arquitectura (Clean Architecture)
- **Nivel de capas:**
  - `Domain`: Entidades de dominio pura (estilo DDD), Value Objects, Interfaces de repositorios. Sin dependencias externas ni paquetes de infraestructura.
  - `Application`: DTOs, Handlers de MediatR (CQRS), Validadores (FluentValidation), Interfaces de servicios externos (HTTP Clients).
  - `Infrastructure`: `DbContext`, implementación de Repositorios, migraciones de EF Core, servicios externos concretos.
  - `Api`: Controllers, Middlewares, Health Checks, configuración de DI (`Program.cs`).

---

## 2. Convenciones de C# y .NET 8
- **Versión de C#:** C# 12 / .NET 8.
- **DTOs, Commands y Queries:** Declarar exclusivamente usando **`public record`** (inmutables por defecto).
  - *Ejemplo:* `public record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;`
- **Nullable Reference Types:** Habilitados (`#nullable enable`).
- **Asincronía:** Todos los métodos I/O deben ser asíncronos (`async`/`await`) y terminar con el sufijo `Async` (ej. `GetByIdAsync`, `AddAsync`).
- **Encapsulamiento en Entidades:** Propiedades con `get; private set;` y constructores explícitos o métodos de mutación (`Update(...)`).

---

## 3. Nomenclatura
- **Clases e Interfaces:**
  - Interfaces prefijadas con `I` (`IProductRepository`, `IOrderService`).
  - Handlers de MediatR nombrados como `<CommandOrQueryName>Handler` (ej. `CreateProductCommandHandler`).
  - Validadores nombrados como `<CommandName>Validator` (ej. `CreateProductCommandValidator`).
- **Archivos:** Un tipo (clase/interface/record) por archivo. El nombre del archivo debe coincidir exactamente con la clase.
- **Bases de Datos (EF Core & PostgreSQL):**
  - Tablas en plural (`Products`, `Orders`, `OrderItems`).
  - Nombres de columnas en `PascalCase` o `snake_case` administrado por EF Core (mantener consistente).

---

## 4. Respuestas HTTP y Manejo de Errores
- **Endpoints de la API:**
  - Usar atributos explícitos `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`.
  - Nombres de rutas en minúsculas y plural: `/api/products`, `/api/orders`.
- **Formato de Errores:**
  - Mapear excepciones a **ProblemDetails** (RFC 7807) desde el Middleware Global.
  - `400 Bad Request` para errores de FluentValidation.
  - `404 Not Found` cuando una entidad no existe.
  - `500 Internal Server Error` para excepciones no controladas.

---

## 5. Pruebas Unitarias (Testing)
- **Frameworks:** `xUnit`, `Moq`, `FluentAssertions`, `Microsoft.EntityFrameworkCore.InMemory`.
- **Patrón de nombrado de tests:** `NombreMetodo_Escenario_ResultadoEsperado`
  - *Ejemplo:* `CreateProduct_WithValidData_ReturnsCreatedResult`
- **Estructura AAA:** `// Arrange`, `// Act`, `// Assert` comentados explícitamente en cada test.