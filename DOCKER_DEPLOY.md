# 🐳 DOCKER DEPLOYMENT - RENDER.COM

## 📋 Configuración en Render

### 1. Crear Web Service
- **Repository**: `gregorio-hell/parcial`
- **Branch**: `docker`
- **Environment**: `Docker`
- **Region**: `Oregon (US West)`

### 2. Variables de Entorno
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
ConnectionStrings__DefaultConnection=DataSource=app.db;Cache=Shared
Redis__ConnectionString=memory://cache
```

### 3. Build Settings
- **Build Command**: `docker build -t parcial-app .`
- **Start Command**: `docker run -p $PORT:$PORT parcial-app`
- **Auto-Deploy**: `Yes`

## 🔍 Verificación
- **Health Check**: `/Health`
- **Login Coordinador**: `coordinador@usmp.pe` / `Coordinador123!`
- **Login Estudiante**: `estudiante@usmp.pe` / `Estudiante123!`

## 🎯 URLs de Prueba
- Home: `/`
- Catálogo: `/Cursos/Catalogo`
- Panel Coordinador: `/Coordinador` (solo coordinadores)
- Health: `/Health`