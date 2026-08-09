# API Reference

The backend exposes REST endpoints under `/api/{Controller}` and returns normalized response objects built with `Ardalis.Result`.

## Response Contract

Successful responses include a status code and a `Value` payload:

```json
{
  "Status": 200,
  "Value": { "Id": 1, "Nombre": "Osciloscopio" },
  "Errors": [],
  "ValidationErrors": []
}
```

Validation and domain failures preserve the same structure:

```json
{
  "Status": 400,
  "Value": null,
  "Errors": ["Carnet ya existe"],
  "ValidationErrors": []
}
```

Common statuses:

| Status | Meaning |
| --- | --- |
| `200 OK` | Request completed successfully. |
| `201 Created` | Resource created successfully. |
| `400 Bad Request` | Validation or business-rule failure. |
| `401 Unauthorized` | Missing or invalid credentials. |
| `403 Forbidden` | Authenticated user does not have permission. |
| `404 Not Found` | Resource does not exist or is not visible. |

## Authentication

JWT is used for authenticated requests. Protected endpoints require:

```http
Authorization: Bearer <token>
```

Administrative operations require an administrator account.

## Endpoints

### Usuario

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/Usuario` | List active users. |
| `GET` | `/api/Usuario/{carnet}` | Get a user by carnet. |
| `POST` | `/api/Usuario` | Create a user and hash the password. |
| `PUT` | `/api/Usuario/{carnet}` | Update user data. |
| `DELETE` | `/api/Usuario/{carnet}` | Soft-delete a user. |
| `POST` | `/api/Usuario/login` | Authenticate and return session data. |

### GrupoEquipo

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/GrupoEquipo` | List active equipment groups. |
| `GET` | `/api/GrupoEquipo/{id}` | Get an equipment group by id. |
| `POST` | `/api/GrupoEquipo` | Create an equipment group. |
| `PUT` | `/api/GrupoEquipo/{id}` | Update an equipment group. |
| `DELETE` | `/api/GrupoEquipo/{id}` | Soft-delete an equipment group. |
| `GET` | `/api/GrupoEquipo/{id}/comentarios` | List comments for an equipment group. |
| `POST` | `/api/GrupoEquipo/{id}/comentarios` | Add an authenticated comment. |

### Equipo

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/Equipo` | List active physical equipment units. |
| `GET` | `/api/Equipo/{id}` | Get an equipment unit by id. |
| `POST` | `/api/Equipo` | Create a physical equipment unit. |
| `PUT` | `/api/Equipo/{id}` | Update a physical equipment unit. |
| `DELETE` | `/api/Equipo/{id}` | Soft-delete a physical equipment unit. |

### Prestamo

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/Prestamo` | List loans for administration. |
| `GET` | `/api/Prestamo/{id}` | Get a loan by id. |
| `GET` | `/api/Prestamo/historial/{carnet}` | Get loan history for a user. |
| `POST` | `/api/Prestamo` | Create a loan request. |
| `PUT` | `/api/Prestamo/{id}/estado` | Change loan state. |
| `DELETE` | `/api/Prestamo/{id}` | Soft-delete a loan. |

### Disponibilidad

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/Carrito/disponibilidad` | Calculate available units by group and date range. |

### Mantenimiento

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/Mantenimiento` | List maintenance records. |
| `GET` | `/api/Mantenimiento/{id}` | Get a maintenance record. |
| `POST` | `/api/Mantenimiento` | Create a maintenance record. |
| `PUT` | `/api/Mantenimiento/{id}` | Update a maintenance record. |
| `DELETE` | `/api/Mantenimiento/{id}` | Soft-delete a maintenance record. |

### Catalogs

| Controller | Base route | Operations |
| --- | --- | --- |
| `Categoria` | `/api/Categoria` | CRUD |
| `Carrera` | `/api/Carrera` | CRUD |
| `Accesorio` | `/api/Accesorio` | CRUD |
| `Componente` | `/api/Componente` | CRUD |
| `EmpresaMantenimiento` | `/api/EmpresaMantenimiento` | CRUD |
| `Mueble` | `/api/Mueble` | CRUD |
| `Gavetero` | `/api/Gavetero` | CRUD |

## Business Rules

| Area | Rule |
| --- | --- |
| Users | `Carnet` and `Email` are required and unique. `Telefono` is unique when provided. |
| Passwords | Minimum 8 characters, at least one uppercase letter, one number and one special character. |
| Equipment | `CodigoImt` is assigned when the unit is created and must not be changed later. |
| Loans | User, equipment group, loan date and return date are required. |
| Availability | Only loans in `aprobado` or `activo` state block capacity. |
| Approval | Availability is revalidated before a pending loan can be approved. |

Loan state model:

```text
pendiente -> aprobado -> activo -> finalizado
    |             |
    |             +-> cancelado
    +-> rechazado
```

## Health Check

```http
GET /api/health
```

Returns `200 OK` with `Healthy` when the API and database are available.
