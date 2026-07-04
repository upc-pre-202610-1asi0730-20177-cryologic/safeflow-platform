<div align="center">
        <img src="https://upload.wikimedia.org/wikipedia/commons/f/fc/UPC_logo_transparente.png"><br>
</div>
<h3 align="center"> Universidad Peruana de Ciencias Aplicadas </h3>

<h3 align="center">Carrera de Ingeniería de Software </h3>

<h3 align="center">1ASI0730 </h3>
<h3 align="center">Aplicaciones Web </h3>
<h3 align="center"> NRC </h3>
<h3 align="center"> 20177 </h3>

<h3 align="center"> Informe de Trabajo Final</h3>


<h3 align="center"> Docente</h3>
<h3 align="center"> Jose Miguel, Flores Ingaruca </h3>


<h3 align="center"> Equipo </h3>
<h3 align="center"> Cryologic Systems </h3>

<h3 align="center"> Proyecto</h3>
<h3 align="center"> SafeFlow </h3>

<h3 align="center"> Integrantes </h3>

<div align = "center">
 
| Code       |      Member |
| :---:      |     :--- |
| U202218531 | Andy Alejandro Mio Mejia |
| U202116018 | Mel Andree Orellana Rodriguez |
| U202114701 | Angel Guillermo Berrospi Marin |
| U20221C803 | Anhelo Rodrigo Rocca Leon |
| U202316049 | Jefferson Bayron Morales Yapuchura |


</div>

<h3 align="center">Periodo 202610</h3>
<h3 align="center">Junio 2026</h3>


<div style="page-break-after: always;"></div>



# SafeFlow Platform — Backend API

> Plataforma tecnológica para el monitoreo y control en tiempo real de la cadena de frío en almacenamiento y transporte logístico. Backend REST desarrollado por **Cryologic Systems**.

---

## Tabla de contenidos

1. [Descripción del proyecto](#descripción-del-proyecto)
2. [Tecnologías utilizadas](#tecnologías-utilizadas)
3. [Arquitectura](#arquitectura)
4. [Bounded Contexts](#bounded-contexts)
5. [Modelo de datos](#modelo-de-datos)
6. [Seguridad](#seguridad)
7. [Patrones de diseño](#patrones-de-diseño)
8. [Estructura del proyecto](#estructura-del-proyecto)
9. [Configuración](#configuración)
10. [Instalación y uso local](#instalación-y-uso-local)
11. [Mapa de endpoints](#mapa-de-endpoints)
12. [Despliegue (CI/CD)](#despliegue-cicd)
13. [Ecosistema SafeFlow](#ecosistema-safeflow)
14. [Notas de implementación](#notas-de-implementación)

---

## Descripción del proyecto

**SafeFlow Platform** (`safeflow-backend`) es la API REST del sistema SafeFlow. Expone los servicios de negocio que consume el frontend web (`safeflow-webapp`) y soporta la operación de monitoreo térmico, inventario, logística, alertas, analytics y reportes.

| Aspecto | Detalle |
|--------|---------|
| **Nombre del proyecto** | `safeflow-backend` |
| **Namespace raíz** | `SafeFlow.API` |
| **Tipo** | API REST (ASP.NET Core) |
| **Framework** | .NET 10 (`net10.0`) |
| **Base de datos** | MySQL (Azure Database for MySQL en producción) |
| **ORM** | Entity Framework Core 9 |
| **Autenticación** | JWT Bearer |
| **Puerto local** | `http://localhost:5115` |
| **Documentación interactiva** | Swagger UI (solo en Development) |
| **Deploy** | Azure Web App `safeflow-api` vía GitHub Actions |

### Capacidades principales

- Autenticar usuarios (registro e inicio de sesión con JWT).
- Gestionar **inventario** de productos sensibles a temperatura (lotes, ubicaciones, rangos térmicos).
- Administrar **logística** (choferes, destinos, despachos/envíos).
- Consultar **monitoreo ambiental** (lecturas de temperatura).
- Crear y resolver **alertas** por excursiones térmicas u otros eventos.
- Obtener **dashboards** de analytics y reporting para la operación.

---

## Tecnologías utilizadas

| Tecnología | Uso |
|------------|-----|
| **ASP.NET Core Web API** | Host HTTP, controllers, middleware |
| **Entity Framework Core 9** | Persistencia y mapeo de entidades |
| **MySql.EntityFrameworkCore** | Proveedor MySQL |
| **BCrypt.Net-Next** | Hash de contraseñas |
| **System.IdentityModel.Tokens.Jwt** | Emisión y validación de tokens |
| **Swashbuckle** | OpenAPI / Swagger |
| **Humanizer** | Utilidades de texto (convenciones de rutas) |
| **GitHub Actions** | CI/CD hacia Azure App Service |

---

## Arquitectura

El proyecto sigue un enfoque **modular por bounded context** inspirado en **Domain-Driven Design (DDD)** e implementa **CQRS ligero** (comandos y consultas separados).

### Capas por módulo

```
{Módulo}/
├── Domain/                 # Núcleo de negocio
│   ├── Model/
│   │   ├── Aggregates/     # Entidades raíz (Product, Alert, User, …)
│   │   ├── Commands/       # Intenciones de escritura
│   │   ├── Queries/        # Intenciones de lectura
│   │   └── ValueObjects/   # Códigos tipados (ProductCode, AlertCode, …)
│   └── Repositories/       # Contratos de persistencia
├── Application/
│   ├── Services/           # Interfaces de aplicación
│   ├── Internal/
│   │   ├── CommandServices/
│   │   └── QueryServices/
│   ├── Errors/             # Errores tipados de aplicación
│   └── Outbound/           # Puertos salientes (hash, token) — solo IAM
├── Infrastructure/
│   └── Persistence/EFC/Repositories/
└── Interfaces/REST/        # Controllers, Resources, Assemblers
```

### Capas DDD

| Capa | Responsabilidad |
|------|-----------------|
| **Domain** | Lógica de negocio, agregados, value objects, contratos de repositorio |
| **Application** | Casos de uso (commands / queries), orquestación, errores tipados |
| **Infrastructure** | EF Core, JWT, BCrypt, implementaciones de repositorios |
| **Interfaces (REST)** | Controllers, resources (DTOs), assemblers |

### Módulo Shared

Concentra infraestructura transversal:

- `AppDbContext` y configuración EF Core
- `UnitOfWork` / `BaseRepository`
- Interceptor de auditoría (`AuditableEntityInterceptor`)
- Patrón `Result<TValue, TError>`
- Registro de servicios DI (`AddSafeFlowServices`)
- Convención de rutas en **kebab-case**
- Utilidad de rango térmico (`ThermalRange`)
- Textos localizados (`LocalizedText`)

---

## Bounded Contexts

### IAM (Identity & Access Management)

**Ruta base:** `/api/authentication`

Gestiona usuarios, hash de contraseñas (BCrypt) y tokens JWT.

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `POST` | `/api/authentication/sign-in` | Anónimo | Inicia sesión; devuelve `id`, `username` y `token` |
| `POST` | `/api/authentication/sign-up` | Anónimo | Registra un usuario nuevo |

**Errores típicos:**

| Código HTTP | Body | Causa |
|-------------|------|-------|
| `400` | `{ "error": "invalid_credentials" }` | Usuario o contraseña incorrectos |
| `409` | `{ "error": "username_taken" }` | Nombre de usuario ya existe |
| `500` | `{ "error": "database_error" }` | Fallo de persistencia |

- **Agregado:** `User` (`id`, `username`, `password_hash`)
- **Servicios de dominio:** `ITokenService`, `IHashingService`
- **Middleware:** `RequestAuthorizationMiddleware` exige `Authorization: Bearer <token>` en todos los endpoints salvo `[AllowAnonymous]`. Las peticiones `OPTIONS` (CORS preflight) se permiten sin token.

---

### Inventory

**Rutas base:** `/api/inventory`, `/api/inventory/items`

Gestiona productos y líneas de stock (ubicación, lote, rango de temperatura, vencimiento).

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/inventory` | Snapshot / resumen de inventario |
| `GET` | `/api/inventory/items` | Lista de ítems |
| `GET` | `/api/inventory/items/{id}` | Detalle por código de línea |
| `POST` | `/api/inventory/items` | Crea producto + línea de stock |
| `POST` | `/api/inventory/items/stock-line` | Agrega línea de stock a un producto existente |
| `PUT` | `/api/inventory/items/{id}` | Actualiza ítem |
| `DELETE` | `/api/inventory/items/{id}` | Elimina línea de inventario |

**Agregados:**

- `Product` — código, nombre, categoría, `TemperatureMin` / `TemperatureMax`, lote, estado, fecha de vencimiento.
- `InventoryLine` — código de línea, ubicación, fecha de ingreso, relación con `Product`.

Los productos definen el **rango térmico permitido**; el resto del sistema usa esa información para alertas y monitoreo.

---

### Logistics

**Rutas base:** `/api/logistics/...`

| Recurso | Ruta | Operaciones |
|---------|------|-------------|
| Despachos / envíos | `/api/logistics/shipments` | `GET`, `POST` |
| Choferes | `/api/logistics/choferes` | `GET`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Destinos | `/api/logistics/destinos` | `GET`, `POST`, `PUT/{id}`, `DELETE/{id}` |

**Agregados:** `LogisticsCarrier`, `LogisticsDriver`, `LogisticsDestination`, `LogisticsRoute`, `LogisticsDispatch`.

Los despachos referencian códigos de línea de inventario (`InventoryLineCode`) para vincular producto en tránsito.

---

### Alerts

**Ruta base:** `/api/alerts`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/alerts/dashboard` | Resumen para dashboard |
| `GET` | `/api/alerts` | Lista de alertas |
| `POST` | `/api/alerts` | Crea alerta |
| `PATCH` | `/api/alerts/{id}/resolve` | Marca alerta como resuelta |

**Agregado `Alert`:**

| Campo | Descripción |
|-------|-------------|
| `AlertCode` | Identificador de negocio (ej. `ALT-XXXXXX`) |
| `AlertType` | Tipo (ej. `sistema`) |
| `Status` | `activa` / `resuelta` |
| `Severity` | Severidad (ej. `high`) |
| `TitleJson` / `MessageJson` | Textos localizados (ES/EN) en JSON |
| `ProductCode` / `DispatchCode` | Referencias opcionales |
| `RecordedAt` | Momento del evento |

---

### Environmental Monitoring

**Ruta base:** `/api/monitoring`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/monitoring/dashboard` | Tarjetas / estado de monitoreo térmico |

**Agregado:** `TemperatureReading` — lecturas de temperatura asociadas a la operación.

La utilidad compartida `ThermalRange` determina si una lectura está fuera de rango (`warning` vs `safe`).

---

### Analytics

**Ruta base:** `/api/analytics`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/analytics/dashboard` | Métricas agregadas para el panel de analytics |

Servicio de solo lectura que consolida información de otros módulos.

---

### Reporting

**Ruta base:** `/api/reporting`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/reporting/dashboard` | Catálogo / ejecuciones de reportes |

**Agregados:** `ReportCatalogItem`, `ReportRun`.

---

## Modelo de datos

| Entidad | Campos principales |
|---------|-------------------|
| **User** | `Id`, `Username`, `PasswordHash` |
| **Product** | `Id`, `ProductCode`, `Name`, `TemperatureMin`, `TemperatureMax`, `Batch`, `Status`, `ExpiryDate` |
| **InventoryLine** | `Id`, `LineCode`, `Location`, `EntryDate`, `ProductId` |
| **Alert** | `Id`, `AlertCode`, `AlertType`, `Status`, `Severity`, `TitleJson`, `MessageJson`, `ProductCode`, `DispatchCode`, `RecordedAt` |
| **TemperatureReading** | Lecturas de temperatura asociadas a la operación |
| **LogisticsCarrier** | Transportistas |
| **LogisticsDriver** | Choferes |
| **LogisticsDestination** | Destinos |
| **LogisticsRoute** | Rutas |
| **LogisticsDispatch** | Despachos (referencia `InventoryLineCode`) |
| **ReportCatalogItem** | Catálogo de reportes |
| **ReportRun** | Ejecuciones de reportes |

Relaciones principales: un `Product` tiene muchas `InventoryLine`; los despachos referencian líneas de inventario; las alertas pueden asociarse a producto o despacho.

Al arrancar, `SafeFlowDbSeeder`:

1. Crea la base de datos si no existe (`CREATE DATABASE IF NOT EXISTS`).
2. Asegura el esquema con `EnsureCreatedAsync`.
3. Garantiza la tabla `users` (compatibilidad / migraciones ligeras).
4. **No inserta datos de ejemplo** por defecto.

Las entidades auditables se actualizan automáticamente mediante `AuditableEntityInterceptor`.

---

## Seguridad

### JWT

- Configuración en `appsettings.json` → sección `TokenSettings:Secret`.
- El secreto debe tener al menos 32 caracteres.
- **Importante:** cambiar el secreto por defecto antes de producción.

### Autorización por endpoint

- Por defecto, los controllers usan `[Authorize]` (atributo propio del módulo IAM).
- Solo `sign-in` y `sign-up` usan `[AllowAnonymous]`.
- Header esperado: `Authorization: Bearer <jwt>`.

### Contraseñas

- Almacenadas como hash con **BCrypt** (`HashingService`).
- Nunca se devuelve el hash en las respuestas de autenticación.

### CORS

Política `SafeFlowFrontend`:

- Permite orígenes `localhost` y `127.0.0.1` (cualquier puerto).
- Permite cualquier subdominio `*.netlify.app` (frontend desplegado).
- Orígenes adicionales configurables en `Cors:AllowedOrigins`.

---

## Patrones de diseño

| Patrón | Dónde |
|--------|--------|
| **Bounded contexts** | Carpetas `Iam`, `Inventory`, `Logistics`, etc. |
| **CQRS ligero** | `*CommandService` / `*QueryService` |
| **Repository + Unit of Work** | `IBaseRepository`, `IUnitOfWork` |
| **Result / Either** | `Result<TValue, TError>` con `Fold` / `Match` |
| **Value Objects** | `ProductCode`, `AlertCode`, `InventoryLineCode`, … |
| **Assembler / DTO** | `*ResourceAssembler` en capa REST |
| **Middleware de autorización** | Autorización JWT centralizada |
| **DI composition root** | `WebApplicationBuilderExtensions.AddSafeFlowServices` |

Ejemplo de manejo de errores en controllers:

```csharp
return result.Fold<IActionResult>(
    value => Ok(value),
    error => error switch
    {
        SomeError.NotFound => NotFound(),
        _ => StatusCode(500)
    });
```

---

## Estructura del proyecto

```
safeflow-platform-main/
├── Program.cs                          # Entry point, pipeline HTTP
├── appsettings.json                    # Connection string, JWT secret
├── safeflow-backend.csproj
├── safeflow-backend.http               # Colección HTTP de prueba
├── Properties/launchSettings.json      # Puerto 5115
├── .github/workflows/
│   └── main_safeflow-api.yml           # Deploy a Azure
├── Alerts/
├── Analytics/
├── EnvironmentalMonitoring/
├── Iam/
├── Inventory/
├── Logistics/
├── Reporting/
└── Shared/
    ├── Application/Patterns/           # Result<T,E>
    ├── Domain/
    └── Infrastructure/
        ├── Interfaces/ASP/Configuration/
        └── Persistence/EFC/
```

---

## Configuración

### `appsettings.json` (referencia)

```json
{
  "TokenSettings": {
    "Secret": "SafeFlow-JWT-Secret-Change-In-Production-Min-32-Chars"
  },
  "ConnectionStrings": {
    "DefaultConnection": "server=...;port=3306;user=...;password=...;database=safeflow_db;sslmode=Required"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

| Clave | Descripción |
|-------|-------------|
| `ConnectionStrings:DefaultConnection` | Cadena MySQL (obligatoria) |
| `TokenSettings:Secret` | Clave de firma JWT |
| `Cors:AllowedOrigins` | Lista opcional de orígenes extra |

> No subas contraseñas reales al repositorio. Usa User Secrets, variables de entorno o Azure App Settings en producción.

---

## Instalación y uso local

### Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MySQL accesible (local o Azure)
- Cadena de conexión válida en `appsettings.json` o `appsettings.Development.json`

### Comandos

```bash
cd safeflow-platform-main

dotnet restore
dotnet run --launch-profile http
```

La API queda en:

- **HTTP:** `http://localhost:5115`
- **Swagger (Development):** `http://localhost:5115/swagger`

Si el puerto 5115 está ocupado, el proceso imprime ayuda para cerrar la instancia previa (`safeflow-backend.exe`).

### Ejemplo de prueba

1. Registrar usuario:

```http
POST /api/authentication/sign-up
Content-Type: application/json

{ "username": "demo", "password": "Demo123!" }
```

2. Iniciar sesión:

```http
POST /api/authentication/sign-in
Content-Type: application/json

{ "username": "demo", "password": "Demo123!" }
```

3. Usar el `token` en el resto de llamadas:

```http
GET /api/inventory/items
Authorization: Bearer <token>
```

---

## Mapa de endpoints

Todos los endpoints (excepto autenticación anónima) requieren JWT.

| Módulo | Método | Ruta |
|--------|--------|------|
| IAM | `POST` | `/api/authentication/sign-in` |
| IAM | `POST` | `/api/authentication/sign-up` |
| Inventory | `GET` | `/api/inventory` |
| Inventory | `GET` | `/api/inventory/items` |
| Inventory | `GET` | `/api/inventory/items/{id}` |
| Inventory | `POST` | `/api/inventory/items` |
| Inventory | `POST` | `/api/inventory/items/stock-line` |
| Inventory | `PUT` | `/api/inventory/items/{id}` |
| Inventory | `DELETE` | `/api/inventory/items/{id}` |
| Logistics | `GET` / `POST` | `/api/logistics/shipments` |
| Logistics | `GET` / `POST` / `PUT` / `DELETE` | `/api/logistics/choferes` |
| Logistics | `GET` / `POST` / `PUT` / `DELETE` | `/api/logistics/destinos` |
| Alerts | `GET` | `/api/alerts/dashboard` |
| Alerts | `GET` | `/api/alerts` |
| Alerts | `POST` | `/api/alerts` |
| Alerts | `PATCH` | `/api/alerts/{id}/resolve` |
| Monitoring | `GET` | `/api/monitoring/dashboard` |
| Analytics | `GET` | `/api/analytics/dashboard` |
| Reporting | `GET` | `/api/reporting/dashboard` |

Las rutas de controllers siguen convención **kebab-case** (`KebabCaseRouteNamingConvention`).

---

## Despliegue (CI/CD)

Archivo: `.github/workflows/main_safeflow-api.yml`

| Evento | Acción |
|--------|--------|
| Push a `main` | Build + deploy automático |
| `workflow_dispatch` | Deploy manual |

**Pasos del workflow:**

1. Checkout del repositorio.
2. Setup .NET 10.
3. `dotnet build -c Release`
4. `dotnet publish`
5. Login a Azure (OIDC / secrets de GitHub).
6. Deploy a Azure Web App **`safeflow-api`** (slot Production).

---

## Ecosistema SafeFlow

| Proyecto | Rol |
|----------|-----|
| **safeflow-platform** (este repo) | API de negocio y persistencia |
| **safeflow-webapp** | Frontend Vue 3 + Vite + PrimeVue; consume esta API |
| **safeflow-website** | Landing corporativa; enlaza a la demo del webapp |

El frontend suele apuntar a la API desplegada en Azure (`safeflow-api`) o a `http://localhost:5115` en desarrollo.

---

## Notas de implementación

1. **Sin migraciones EF formales:** se usa `EnsureCreated` + scripts SQL puntuales (tabla `users`). Útil para prototipo/académico; en producción conviene migraciones versionadas.
2. **Textos bilingües en alertas:** títulos y mensajes se guardan como JSON (`LocalizedText` / serialización manual).
3. **Códigos de negocio** (no solo IDs numéricos) en rutas y respuestas (`ALT-…`, códigos de línea, etc.).
4. **CORS orientado a Netlify** para el frontend desplegado.
5. **Swagger solo en Development** para no exponer el explorador en producción por defecto.

### Posibles mejoras futuras

- Migraciones EF Core versionadas en lugar de `EnsureCreated`.
- Roles y permisos (RBAC) más allá de “usuario autenticado”.
- Endpoints de escritura para lecturas de temperatura (telemetría IoT entrante).
- Rate limiting y refresh tokens.
- Tests automatizados (unitarios de dominio + integración de API).
- Secretos fuera de `appsettings.json` (Azure Key Vault / User Secrets).

---

## Autor

* **Cryologic Systems** — Startup de soluciones tecnológicas para el control de cadena de frío.
* **Proyecto:** SafeFlow Platform (Backend)
* **Periodo:** 202610
