# Render Deployment Configuration

## Variables de Entorno Requeridas

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ConnectionStrings__DefaultConnection=DataSource=app.db;Cache=Shared
Redis__ConnectionString=<YOUR_REDIS_CONNECTION_STRING>
```

## Configuración en Render

1. **Tipo de Servicio**: Web Service
2. **Runtime**: Docker
3. **Build Command**: `docker build -t parcial .`
4. **Start Command**: Se maneja automáticamente por el Dockerfile

## Variables de Entorno en Render Dashboard

- `ASPNETCORE_ENVIRONMENT`: `Production`
- `ASPNETCORE_URLS`: `http://0.0.0.0:${PORT}`
- `ConnectionStrings__DefaultConnection`: `DataSource=app.db;Cache=Shared`
- `Redis__ConnectionString`: Tu string de conexión a Redis (opcional, usa memoria si no está disponible)

## Verificación Post-Despliegue

Una vez desplegado, verificar:

✅ **Login**: Acceso con credenciales predeterminadas
- Estudiante: `estudiante@usmp.pe` / `123`
- Coordinador: `coordinador@usmp.pe` / `123`

✅ **Catálogo**: Navegación y filtros de cursos

✅ **Inscripción**: Matriculación de estudiantes

✅ **Panel Coordinador**: Gestión administrativa (solo visible para coordinadores)

## Notas

- SQLite se incluye en el contenedor para simplicidad
- Redis es opcional - la app funciona con cache en memoria si no está disponible
- Todas las migraciones se ejecutan automáticamente al iniciar
- Los datos de prueba se siembran automáticamente