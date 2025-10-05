# Render Deployment - Sin PostgreSQL (SQLite + Redis Opcional)

## 📋 Configuración del Web Service en Render

### Tipo de Servicio
- **Service Type**: Web Service
- **Runtime**: Docker
- **Branch**: deploy/render
- **Auto-Deploy**: Enabled

## 🔧 Variables de Entorno CRÍTICAS

### Mínimas Requeridas
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:${PORT}
```

### Opcionales (Cache Redis)
```bash
# Solo si tienes Redis disponible
REDIS_URL=redis://user:password@host:port
ConnectionStrings__Redis=your-redis-connection-string
```

### Base de Datos
```bash
# Opcional - Por defecto usa almacenamiento local automático
ConnectionStrings__DefaultConnection=DataSource=/opt/render/project/data/app.db;Cache=Shared
```

## Endpoints de Diagnóstico

- **Basic Health**: `https://tu-app.onrender.com/Health`
- **Full Health**: `https://tu-app.onrender.com/Health/Full`
- **Home**: `https://tu-app.onrender.com/`

## Pasos para Resolver Errores

1. **Configurar Variables**: Copia las variables exactamente como están arriba
2. **Verificar Logs**: Ve a Render Dashboard → tu servicio → Logs
3. **Probar Health**: Visita `/Health` para ver el estado
4. **Redespliega**: Manual redeploy si es necesario

## Credenciales de Prueba

## Debugging en Render

Si encuentras errores después del despliegue:

1. **Verificar logs**: Ve a tu servicio en Render → Logs tab
2. **Health check**: Visita `/Health` en tu aplicación desplegada
3. **Variables de entorno**: Verifica que estén configuradas correctamente

### Health Check Endpoint
`https://tu-app.onrender.com/Health`

Este endpoint te mostrará:
- Status de la aplicación
- Información de la base de datos  
- Variables de entorno
- Detalles del sistema

### Variables de Entorno Requeridas

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
- `ConnectionStrings__DefaultConnection`: `DataSource=/app/data/app.db;Cache=Shared`
- `Redis__ConnectionString`: Tu string de conexión a Redis (opcional, usa memoria si no está disponible)

## Verificación Post-Despliegue

Una vez desplegado, verificar:

✅ **Login**: Acceso con credenciales predeterminadas
- Estudiante: `estudiante@usmp.pe` / `Estudiante123!`
- Coordinador: `coordinador@usmp.pe` / `Coordinador123!`

✅ **Catálogo**: Navegación y filtros de cursos

✅ **Inscripción**: Matriculación de estudiantes

✅ **Panel Coordinador**: Gestión administrativa (solo visible para coordinadores)

## Notas

- SQLite se incluye en el contenedor para simplicidad
- Redis es opcional - la app funciona con cache en memoria si no está disponible
- Todas las migraciones se ejecutan automáticamente al iniciar
- Los datos de prueba se siembran automáticamente