# 🚀 RENDER DEPLOYMENT GUIDE

## ⚙️ Configuración en Render.com

### 1. Crear Web Service
- Ve a [render.com](https://render.com)
- Conecta tu repositorio GitHub: `gregorio-hell/parcial`
- Selecciona **rama**: `deploy/render`
- **Service Type**: Web Service
- **Environment**: Docker

### 2. Variables de Entorno OBLIGATORIAS

⚠️ **COPIA ESTAS VARIABLES EXACTAMENTE:**

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
ConnectionStrings__DefaultConnection=DataSource=app.db;Cache=Shared
Redis__ConnectionString=redis://localhost:6379
```

### 3. Configuración del Servicio
- **Name**: `parcial-universidad`
- **Region**: Oregon (US West)
- **Branch**: `deploy/render`
- **Build Command**: `docker build -t parcial .`
- **Start Command**: `docker run -p $PORT:$PORT parcial`

## 🔍 Verificación Post-Despliegue

Una vez desplegado, verifica estas funcionalidades:

### ✅ Login
- **Coordinador**: `coordinador@usmp.pe` / `Coordinador123!`
- **Estudiante**: `estudiante@usmp.pe` / `Estudiante123!`

### ✅ Funcionalidades a probar:
1. **Home**: Panel principal con tarjetas
2. **Catálogo**: Lista de cursos con filtros
3. **Inscripción**: Matriculación de estudiantes
4. **Panel Coordinador**: Solo visible tras login como coordinador

### 🩺 Health Check
- **Basic**: `https://tu-app.onrender.com/Health`
- **Full**: `https://tu-app.onrender.com/Health/Full`

## 🛠️ Troubleshooting

### Si hay errores:
1. **Revisa logs** en Render Dashboard
2. **Verifica variables** de entorno
3. **Usa Health endpoints** para diagnóstico
4. **Redespliega** si es necesario

### Variables comunes que faltan:
- `PORT` debe ser automático en Render
- `ASPNETCORE_URLS` debe incluir `$PORT`
- `ConnectionStrings__DefaultConnection` es obligatorio

## 📱 URLs de Prueba

Una vez desplegado:
- **Home**: `https://tu-app.onrender.com/`
- **Catálogo**: `https://tu-app.onrender.com/Cursos/Catalogo`
- **Login**: `https://tu-app.onrender.com/Identity/Account/Login`
- **Health**: `https://tu-app.onrender.com/Health`