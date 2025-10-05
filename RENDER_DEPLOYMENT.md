# 🚀 Despliegue en Render.com

## Configuración del Web Service

### 1. Variables de Entorno Requeridas

```bash
# Configuración de ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Base de Datos (PostgreSQL proporcionado por Render)
ConnectionStrings__DefaultConnection=postgresql://username:password@hostname:port/database

# Redis (Upstash o servicio Redis externo)
Redis__ConnectionString=redis://username:password@hostname:port

# Opcional: Configuraciones adicionales
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
```

### 2. Configuración en Render Dashboard

1. **Crear nuevo Web Service**
   - Repository: Connect your GitHub repo
   - Branch: `deploy/render`
   - Runtime: Docker

2. **Build & Deploy Settings**
   - Build Command: `docker build -t parcial-app .`
   - Start Command: `docker run -p $PORT:$PORT parcial-app`

3. **Environment Variables**
   - Add all variables listed above
   - Use Render's PostgreSQL add-on for database
   - Use external Redis service (Upstash recommended)

### 3. Servicios Adicionales Necesarios

#### PostgreSQL Database
```bash
# En Render Dashboard:
# 1. Create new PostgreSQL database
# 2. Copy connection string to ConnectionStrings__DefaultConnection
```

#### Redis (Upstash)
```bash
# 1. Create account at upstash.com
# 2. Create Redis database
# 3. Copy connection string to Redis__ConnectionString
```

### 4. Verificación Post-Despliegue

Una vez desplegado, verificar las siguientes funcionalidades:

✅ **Login y Autenticación**
- [ ] Registro de nuevos usuarios
- [ ] Login con credenciales existentes
- [ ] Logout correcto
- [ ] Roles (Coordinador/Estudiante)

✅ **Catálogo de Cursos**
- [ ] Listado de cursos disponibles
- [ ] Filtros por departamento, créditos, etc.
- [ ] Paginación y búsqueda
- [ ] Detalles de curso individual

✅ **Sistema de Inscripciones**
- [ ] Inscripción a cursos disponibles
- [ ] Validaciones de cupos y horarios
- [ ] Cancelación de matrículas
- [ ] Historial de inscripciones

✅ **Panel de Coordinador**
- [ ] Acceso restringido por rol
- [ ] Gestión de cursos (CRUD)
- [ ] Reportes de matriculaciones
- [ ] Estadísticas del sistema

### 5. URLs de Prueba

```bash
# Reemplazar {your-app-name} con el nombre de tu servicio en Render
Base URL: https://{your-app-name}.onrender.com

# Endpoints principales:
https://{your-app-name}.onrender.com/
https://{your-app-name}.onrender.com/Cursos
https://{your-app-name}.onrender.com/Matriculas
https://{your-app-name}.onrender.com/Coordinador
https://{your-app-name}.onrender.com/Identity/Account/Login
https://{your-app-name}.onrender.com/health
```

### 6. Credenciales por Defecto

```bash
# Usuario Coordinador (creado automáticamente)
Email: coordinador@usmp.pe
Password: Coordinador123!
```

### 7. Troubleshooting

#### Problemas Comunes:

1. **Error 500 al iniciar**
   - Verificar variables de entorno
   - Revisar conexión a base de datos
   - Comprobar logs en Render Dashboard

2. **Base de datos no inicializada**
   - Verificar que ConnectionStrings__DefaultConnection esté correcta
   - Revisar que las migraciones se ejecuten automáticamente

3. **Redis no conecta**
   - Verificar Redis__ConnectionString
   - Comprobar que el servicio Redis esté activo

4. **Problemas de CORS**
   - Verificar ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

### 8. Monitoreo

- **Health Check**: `https://{your-app-name}.onrender.com/health`
- **Logs**: Disponibles en Render Dashboard
- **Métricas**: CPU, Memory, Response Time en Dashboard

---

## 🎯 Checklist de Despliegue

- [ ] Dockerfile optimizado creado
- [ ] Variables de entorno configuradas
- [ ] Base de datos PostgreSQL creada
- [ ] Redis configurado (Upstash)
- [ ] Web Service desplegado en Render
- [ ] Health check funcionando
- [ ] Login verificado
- [ ] Catálogo funcionando
- [ ] Inscripciones operativas
- [ ] Panel coordinador accesible
- [ ] Performance verificada

## 📞 Soporte

Para problemas específicos de Render:
- [Render Documentation](https://render.com/docs)
- [Render Community](https://community.render.com)