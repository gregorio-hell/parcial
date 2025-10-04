# Portal Universitario - Sistema de Gestión de Cursos

Sistema web desarrollado en ASP.NET Core 9.0 para la gestión de cursos universitarios con funcionalidades de matrícula, sesiones Redis y panel administrativo.

## 📋 Funcionalidades Implementadas

### Pregunta 1 - Bootstrap + Dominio + Identity
- ✅ Modelos de dominio (Curso, Matricula, EstadoMatricula)
- ✅ ASP.NET Core Identity con roles
- ✅ Interfaz Bootstrap con Font Awesome
- ✅ Base de datos SQLite con Entity Framework

### Pregunta 2 - Catálogo de Cursos
- ✅ Catálogo con filtros avanzados
- ✅ Validaciones server-side
- ✅ Vista detalle de cursos
- ✅ Búsqueda por nombre/código, créditos, horarios

### Pregunta 3 - Sistema de Matrículas
- ✅ Inscripción con validaciones de negocio
- ✅ Control de cupos máximos
- ✅ Prevención de matrículas duplicadas
- ✅ Estados: Pendiente, Confirmada, Cancelada
- ✅ Gestión completa desde panel de usuario

### Pregunta 4 - Sesiones y Redis
- ✅ SesionService para último curso visitado
- ✅ Cache distribuido de 60 segundos para cursos activos
- ✅ ViewComponent "Volver al curso" en navegación
- ✅ Invalidación automática de cache

### Pregunta 5 - Panel de Coordinador
- ✅ Rol Coordinador con autorización
- ✅ CRUD completo de cursos
- ✅ Gestión de matrículas (confirmar/cancelar)
- ✅ Activar/desactivar cursos
- ✅ Página de acceso denegado

## 🔐 Credenciales del Sistema

### Coordinador (Panel Administrativo)
- **Email**: `coordinador@usmp.pe`
- **Contraseña**: `Coordinador123!`
- **Acceso**: Panel completo de administración

### Registro de Usuarios
- **Requisitos mínimos**: 3 caracteres
- **Sin restricciones**: mayúsculas, números o símbolos especiales

## 🚀 Instalación y Ejecución

### Prerrequisitos
- .NET 9.0 SDK
- SQLite (incluido)

### Pasos de instalación
```bash
# Clonar repositorio
git clone https://github.com/gregorio-hell/parcial.git
cd parcial

# Restaurar dependencias
dotnet restore

# Aplicar migraciones
dotnet ef database update

# Ejecutar aplicación
dotnet run
```

### Acceso
- **Aplicación**: `http://localhost:5054`
- **Panel Coordinador**: `http://localhost:5054/Coordinador` (requiere login)

## 🏗️ Arquitectura

### Tecnologías
- **Framework**: ASP.NET Core 9.0 MVC
- **Base de datos**: SQLite con Entity Framework Core
- **Autenticación**: ASP.NET Core Identity
- **Cache**: Redis (con fallback a memoria)
- **Frontend**: Bootstrap 5 + Font Awesome
- **Patrones**: Repository, Service Layer, ViewModels

### Estructura del Proyecto
```
├── Controllers/          # Controladores MVC
├── Models/              # Modelos de dominio
├── ViewModels/          # ViewModels para vistas
├── Services/            # Lógica de negocio
├── Data/                # Contexto EF y migraciones
├── Views/               # Vistas Razor
├── ViewComponents/      # Componentes reutilizables
└── wwwroot/            # Archivos estáticos
```

## 📊 Base de Datos

### Tablas principales
- **AspNetUsers**: Usuarios del sistema
- **AspNetRoles**: Roles (Coordinador, Estudiante)
- **Cursos**: Información de cursos
- **Matriculas**: Inscripciones de estudiantes

### Relaciones
- Usuario 1:N Matriculas
- Curso 1:N Matriculas
- Validaciones de integridad y restricciones únicas

## 🔄 Flujo de Trabajo Git

### Ramas implementadas
- `feature/bootstrap-dominio` - Pregunta 1
- `feature/catalogo-cursos` - Pregunta 2  
- `feature/matriculas` - Pregunta 3
- `feature/sesion-redis` - Pregunta 4
- `feature/panel-coordinador` - Pregunta 5

### Pull Requests
Cada funcionalidad tiene su PR correspondiente hacia `main`:
- [PR #1: Bootstrap + Dominio](./pulls)
- [PR #2: Catálogo de Cursos](./pulls)
- [PR #3: Sistema de Matrículas](./pulls)
- [PR #4: Sesiones Redis](./pulls)
- [PR #5: Panel Coordinador](./pulls)

## 🎯 Casos de Uso

### Para Estudiantes
1. Registrarse en el sistema
2. Explorar catálogo de cursos con filtros
3. Ver detalles de cursos
4. Inscribirse en cursos disponibles
5. Gestionar sus matrículas

### Para Coordinadores
1. Acceder al panel administrativo
2. Crear/editar/desactivar cursos
3. Gestionar matrículas por curso
4. Confirmar o cancelar inscripciones
5. Ver estadísticas de ocupación

## 🔧 Configuración Redis

### Desarrollo (memoria)
```csharp
services.AddDistributedMemoryCache();
```

### Producción (Redis real)
```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis-connection-string";
});
```

## 📝 Validaciones Implementadas

### Server-side
- Modelos con DataAnnotations
- Validaciones de negocio en servicios
- Control de ModelState en controladores
- Validaciones personalizadas de horarios

### Reglas de Negocio
- Un usuario no puede matricularse dos veces en el mismo curso
- No se puede exceder el cupo máximo
- Horario de inicio debe ser menor al de fin
- Solo coordinadores pueden acceder al panel administrativo

## 🚀 Despliegue

### Rama: deploy/render
**Requerido**: Desplegar en Render como Web Service.

**Variables mínimas**:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://0.0.0.0:${PORT}`
- `ConnectionStrings__DefaultConnection=DataSource=app.db;Cache=Shared`
- `Redis__ConnectionString=<OPTIONAL_REDIS_URL>`

**Verificar online**: login, catálogo, inscripción y panel coordinador.

### Configuración Render.com
1. **Tipo**: Web Service
2. **Runtime**: Docker
3. **Repository**: Conectar a GitHub
4. **Branch**: `deploy/render`
5. **Build**: Automático via Dockerfile
6. **Port**: Detectado automáticamente desde `${PORT}`

### Docker Container
```dockerfile
# El proyecto incluye Dockerfile optimizado para .NET 9
# Construcción multi-stage para optimizar tamaño
# SQLite incluido para simplicidad de base de datos
```

### Post-Despliegue
Verificar funcionalidades:
- ✅ Login con credenciales predeterminadas
- ✅ Catálogo de cursos con filtros
- ✅ Sistema de inscripción
- ✅ Panel coordinador (solo coordinadores)

## 👥 Contribución

Proyecto desarrollado siguiendo:
- ✅ Git Flow con ramas feature
- ✅ Pull Requests hacia main
- ✅ Commits descriptivos
- ✅ Código limpio y documentado

## 📄 Licencia

Proyecto académico - Universidad San Martín de Porres

---
**Desarrollado por**: [Tu Nombre]  
**Fecha**: Octubre 2025  
**Curso**: Desarrollo Web