# PatientsVIVA Backend API

Una API RESTful desarrollada en .NET Core 8 para la gestión de pacientes, implementando arquitectura en capas, Entity Framework Core, procedimientos almacenados, validaciones robustas y pruebas unitarias.

## Tabla de Contenidos

- [Características](#características)
- [Arquitectura](#arquitectura)
- [Prerrequisitos](#prerrequisitos)
- [Instalación y Configuración](#instalación-y-configuración)
- [Configuración de Base de Datos](#configuración-de-base-de-datos)
- [Ejecución del Proyecto](#ejecución-del-proyecto)
- [API Endpoints](#api-endpoints)
- [Pruebas](#pruebas)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Decisiones Técnicas](#decisiones-técnicas)

## Características

- ✅ **API RESTful completa** para gestión de pacientes (CRUD)
- ✅ **Arquitectura en capas** bien definida (API, BLL, DAL, DTO, IOC, Utility)
- ✅ **Entity Framework Core** para acceso a datos
- ✅ **Procedimientos almacenados** para consultas específicas
- ✅ **Validaciones robustas** con DataAnnotations y lógica de negocio
- ✅ **Paginación y filtros** en listados
- ✅ **AutoMapper** para mapeo de DTOs
- ✅ **Inyección de dependencias** con .NET Core DI
- ✅ **Manejo centralizado de errores** y logging
- ✅ **Documentación Swagger** automática
- ✅ **CORS configurado** para integración con frontend
- ✅ **Pruebas unitarias** con xUnit, Moq y Entity Framework InMemory

## Arquitectura

El proyecto implementa una **arquitectura en capas** que garantiza separación de responsabilidades y mantenibilidad:

```
PatientsVIVABackend/
├── PatientsVIVABackend.API/          # Capa de presentación (Controllers)
├── PatientsVIVABackend.BLL/          # Lógica de negocio (Services)
├── PatientsVIVABackend.DAL/          # Acceso a datos (Repositories)
├── PatientsVIVABackend.DTO/          # Objetos de transferencia de datos
├── PatientsVIVABackend.Model/        # Entidades del dominio
├── PatientsVIVABackend.IOC/          # Inversión de control (DI)
├── PatientsVIVABackend.Utility/      # Utilidades (AutoMapper, ResponseAPI)
└── PatientsVIVABackend.Test/         # Pruebas unitarias e integración
```

### Flujo de Datos
```
Controller → Service → Repository → Entity Framework → SQL Server
     ↓
   AutoMapper (DTO ↔ Entity)
     ↓
  Response API
```

## Prerrequisitos

- **.NET Core 8.0 SDK** o superior
- **SQL Server** (LocalDB, Express, Standard o Enterprise)
- **Visual Studio 2022** o **Visual Studio Code** (recomendado)
- **Git** para control de versiones

## Instalación y Configuración

### 1. Clonar el Repositorio

```bash
git clone https://github.com/tu-usuario/PatientsVIVABackend.git
cd PatientspVIVABackend
```

### 2. Restaurar Dependencias

```bash
dotnet restore
```

### 3. Configurar Variables de Entorno

El proyecto utiliza variables de entorno para la cadena de conexión. Configura la variable:

**Windows (PowerShell):**
```powershell
$env:VIVAPATIENTS_CONNECTION_STRING="Server=(local); DataBase=VIVAPATIENTS; Trusted_Connection=True; TrustServerCertificate=True;"
```

**Windows (Command Prompt):**
```cmd
set VIVAPATIENTS_CONNECTION_STRING=Server=(local); DataBase=VIVAPATIENTS; Trusted_Connection=True; TrustServerCertificate=True;
```


### 4. Configuración de Base de Datos

#### Paso 1: Ejecutar Script de Base de Datos

Ejecuta el script `SQL_QUERY_VIVA.sql` en SQL Server Management Studio o Azure Data Studio:

```sql
-- Ubicación: /SQL_QUERY_VIVA.sql
CREATE DATABASE VIVAPATIENTS
USE VIVAPATIENTS
CREATE TABLE Patients(
    PatientId INT IDENTITY(1,1) PRIMARY KEY,
    DocumentType VARCHAR(10) NOT NULL,
    DocumentNumber NVARCHAR(20) NOT NULL UNIQUE,
    FirstName NVARCHAR(80) NOT NULL,
    LastName NVARCHAR(80) NOT NULL,
    BirthDate DATE NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Email NVARCHAR(120) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

#### Paso 2: Ejecutar Procedimiento Almacenado

Ejecuta el script `Store_Procedure_GetPatientsCreatedAfter.sql` que se encuentra en la raiz del proyecto:

```sql
-- Ubicación: /Store_Procedure_GetPatientsCreatedAfter.sql
-- Este script crea el procedimiento almacenado GetPatientsCreatedAfter
-- que permite filtrar pacientes por fecha de creación
```

#### Verificación de Base de Datos

Verifica que la base de datos y el procedimiento se crearon correctamente:

```sql
-- Verificar tabla
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Patients'

-- Verificar procedimiento
SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'GetPatientsCreatedAfter'
```

## Ejecución del Proyecto

### Desarrollo

```bash
# Desde la carpeta raíz del proyecto
cd PatientsVIVABackend
dotnet run
```

### Producción

```bash
dotnet build --configuration Release
dotnet run --configuration Release
```

### Acceso a la API

- **Swagger UI**: `https://localhost:7208/swagger` (HTTPS)
- **API Base URL**: `https://localhost:7208/api` (HTTPS)
- **HTTP**: `http://localhost:5208` (alternativo)

## API Endpoints

### Pacientes (Patients)

| Método | Endpoint | Descripción | Parámetros |
|--------|----------|-------------|------------|
| `GET` | `/api/patients` | Listar pacientes con paginación y filtros | `page`, `pageSize`, `name`, `documentNumber` |
| `GET` | `/api/patients/{id}` | Obtener paciente por ID | `id` (int) |
| `POST` | `/api/patients` | Crear nuevo paciente | Body: `CreatePatientDTO` |
| `PUT` | `/api/patients/{id}` | Actualizar paciente (parcial/completo) | `id` (int), Body: `UpdatePatientDTO` |
| `DELETE` | `/api/patients/{id}` | Eliminar paciente | `id` (int) |
| `GET` | `/api/patients/created-after` | Pacientes creados después de fecha | `createdAfter` (DateTime) |

### Ejemplos de Uso

#### Crear Paciente
```json
POST /api/patients
{
    "documentType": "CC",
    "documentNumber": "12345678",
    "firstName": "Juan",
    "lastName": "Pérez",
    "birthDate": "1990-01-15",
    "phoneNumber": "3001234567",
    "email": "juan.perez@email.com"
}
```

#### Respuesta Exitosa
```json
{
    "success": true,
    "message": "Patient created successfully",
    "data": {
        "patientId": 1,
        "documentType": "CC",
        "documentNumber": "12345678",
        "firstName": "Juan",
        "lastName": "Pérez",
        "birthDate": "1990-01-15",
        "phoneNumber": "3001234567",
        "email": "juan.perez@email.com",
        "createdAt": "2024-01-15T10:30:00Z"
    }
}
```

#### Listar con Paginación y Filtros
```
GET /api/patients?page=1&pageSize=10&name=Juan&documentNumber=123
```

#### Actualización Parcial
```json
PUT /api/patients/1
{
    "firstName": "Juan Carlos",
    "email": "juancarlos.perez@email.com"
}
```

#### Filtrar por Fecha (Procedimiento Almacenado)
```
GET /api/patients/created-after?createdAfter=2024-01-01
```

## Pruebas

El proyecto incluye una suite completa de pruebas unitarias e integración.

### Ejecutar Todas las Pruebas

```bash
dotnet test
```

### Ejecutar Pruebas por Categoría

```bash
# Solo pruebas unitarias
dotnet test --filter "FullyQualifiedName!~Integration"

# Solo pruebas de controladores
dotnet test --filter "ClassName=PatientsControllerTests"

# Solo pruebas de servicios
dotnet test --filter "ClassName=PatientServiceTests"

# Solo pruebas de integración
dotnet test --filter "ClassName=CompleteWorkingTests"
```

### Cobertura de Pruebas

```bash
# Generar reporte de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### Tipos de Pruebas Implementadas

#### Pruebas Unitarias del Servicio (13 pruebas)
- ✅ GetPatientById (casos válidos, inválidos, no encontrado)
- ✅ CreatePatient (creación exitosa, duplicados)
- ✅ UpdatePatient (actualización exitosa, paciente no encontrado)
- ✅ DeletePatient (eliminación exitosa, paciente no encontrado)
- ✅ GetPatientsCreatedAfterAsync (procedimiento almacenado)

#### Pruebas Unitarias del Controlador (19 pruebas)
- ✅ Todos los endpoints HTTP con casos exitosos y de error
- ✅ Validaciones de entrada (IDs inválidos, formatos de email)
- ✅ Manejo de excepciones del servicio
- ✅ Códigos de estado HTTP correctos

#### Pruebas de Integración (11 pruebas)
- ✅ Flujo completo CRUD
- ✅ Validación de duplicados
- ✅ Paginación y filtros
- ✅ Procedimientos almacenados
- ✅ Validaciones de negocio

## Estructura del Proyecto

```
PatientsVIVABackend/
├── 📁 PatientsVIVABackend.API/
│   ├── Controllers/
│   │   └── PatientsController.cs           # Controlador principal
│   ├── Program.cs                          # Configuración de la aplicación
│   ├── appsettings.json                    # Configuración
│   └── PatientsVIVABackend.csproj         # Proyecto API
│
├── 📁 PatientsVIVABackend.BLL/
│   └── PatientsService/
│       ├── PatientService.cs               # Lógica de negocio
│       └── Contract/
│           └── IPatientService.cs          # Interfaz del servicio
│
├── 📁 PatientsVIVABackend.DAL/
│   └── Repositories/
│       ├── GenericRepository.cs            # Repositorio genérico
│       └── IGenericRepository.cs           # Interfaz del repositorio
│
├── 📁 PatientsVIVABackend.DTO/
│   ├── CreatePatientDTO.cs                 # DTO para creación
│   ├── UpdatePatientDTO.cs                 # DTO para actualización
│   ├── PatientDTO.cs                       # DTO para respuestas
│   └── PagedResultDTO.cs                   # DTO para paginación
│
├── 📁 PatientsVIVABackend.Model/
│   ├── Patient.cs                          # Entidad Patient
│   └── VivapatientsContext.cs              # DbContext
│
├── 📁 PatientsVIVABackend.IOC/
│   └── Dependency.cs                       # Configuración DI
│
├── 📁 PatientsVIVABackend.Utility/
│   ├── AutoMapperProfile.cs                # Perfiles de mapeo
│   └── ResponseAPI.cs                      # Clase para respuestas API
│
├── 📁 PatientsVIVABackend.Test/
│   ├── Common/
│   │   └── TestBase.cs                     # Clase base para pruebas
│   ├── Controllers/
│   │   └── PatientsControllerTests.cs      # Pruebas del controlador
│   ├── Services/
│   │   └── PatientServiceTests.cs          # Pruebas del servicio
│   ├── Integration/
│   │   └── CompleteWorkingTests.cs         # Pruebas de integración
│   └── PatientsVIVABackend.Test.csproj     # Proyecto de pruebas
│
├── 📄 SQL_QUERY_VIVA.sql                   # Script de base de datos
├── 📄 Store_Procedure_GetPatientsCreatedAfter.sql  # Procedimiento almacenado
└── 📄 README.md                            # Documentación
```

## Tecnologías Utilizadas

### Backend
- **.NET Core 8.0** - Framework principal
- **ASP.NET Core Web API** - Para crear la API RESTful
- **Entity Framework Core 8.0** - ORM para acceso a datos
- **SQL Server** - Base de datos relacional
- **AutoMapper** - Mapeo de objetos
- **Swashbuckle.AspNetCore** - Documentación Swagger

### Pruebas
- **xUnit** - Framework de pruebas unitarias
- **Moq** - Biblioteca para mocks y stubs
- **Entity Framework InMemory** - Base de datos en memoria para pruebas
- **Microsoft.AspNetCore.Mvc.Testing** - Pruebas de integración

### Herramientas
- **Visual Studio 2022** - IDE principal
- **SQL Server Management Studio** - Gestión de base de datos
- **Postman** - Pruebas de API (opcional)

## Decisiones Técnicas

### Arquitectura en Capas
**Decisión**: Implementar arquitectura en capas bien definida.
**Razón**: Facilita el mantenimiento, testing y escalabilidad. Cada capa tiene una responsabilidad específica.

### Patrón Repository
**Decisión**: Usar patrón Repository genérico.
**Razón**: Abstrae el acceso a datos, facilita pruebas unitarias y permite cambiar el proveedor de datos sin afectar la lógica de negocio.

### DTOs para Transferencia de Datos
**Decisión**: Usar DTOs separados para entrada y salida.
**Razón**: Control granular sobre qué datos se exponen, validaciones específicas y versionado de API.

### AutoMapper para Mapeo
**Decisión**: Implementar AutoMapper con perfiles personalizados.
**Razón**: Reduce código boilerplate, mapeos complejos centralizados y configuración declarativa.

### Variable de Entorno para Connection String
**Decisión**: Usar variable de entorno en lugar de appsettings.
**Razón**: Seguridad mejorada, diferentes configuraciones por ambiente sin modificar código.

### Validaciones en Múltiples Capas
**Decisión**: Validaciones en DTOs (DataAnnotations) y en servicios (lógica de negocio).
**Razón**: Validaciones de formato en entrada, validaciones de negocio en el servicio.

### Procedimientos Almacenados para Consultas Específicas
**Decisión**: Usar procedimientos almacenados para consultas complejas.
**Razón**: Mejor rendimiento, lógica compleja en base de datos, aprovechamiento de características específicas de SQL Server.

### Manejo Centralizado de Errores
**Decisión**: ResponseAPI consistente y manejo de excepciones en controladores.
**Razón**: API predecible, fácil consumo desde frontend, debugging simplificado.

### Pruebas Unitarias y de Integración
**Decisión**: Suite completa de pruebas con diferentes niveles.
**Razón**: Calidad de código, detección temprana de errores, refactoring seguro.

### CORS Habilitado
**Decisión**: Configurar CORS para permitir cualquier origen en desarrollo.
**Razón**: Facilita integración con frontend durante desarrollo. En producción debe configurarse específicamente.

## Configuración Adicional

### Logging
El proyecto usa el sistema de logging de .NET Core. Los logs se muestran en consola durante desarrollo.

### Swagger
Swagger UI está disponible automáticamente en desarrollo para probar los endpoints.


## Estados de Respuesta HTTP

| Código | Descripción | Cuándo se Usa |
|--------|-------------|---------------|
| 200 OK | Operación exitosa | GET, PUT, DELETE exitosos |
| 201 Created | Recurso creado | POST exitoso |
| 400 Bad Request | Datos inválidos | Validaciones fallidas, IDs inválidos |
| 404 Not Found | Recurso no encontrado | GET/PUT/DELETE de ID no existente |
| 409 Conflict | Conflicto de datos | Duplicados (DocumentType + DocumentNumber) |
| 500 Internal Server Error | Error del servidor | Errores no controlados |

## Soporte

Para preguntas, problemas o sugerencias:

1. **Issues**: Crear un issue en el repositorio
2. **Documentación**: Revisar este README y la documentación Swagger
3. **Logs**: Revisar los logs de la aplicación para debugging

## Licencia

Este proyecto fue desarrollado como prueba técnica para la empresa VIVA 1A.

---

**Desarrollado con ❤️ usando .NET Core 8 y las mejores prácticas de desarrollo.**