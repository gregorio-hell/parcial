#!/bin/bash
set -e

echo "=== Iniciando aplicación ASP.NET Core ==="
echo "Environment: $ASPNETCORE_ENVIRONMENT"
echo "URLs: $ASPNETCORE_URLS"
echo "Working Directory: $(pwd)"
echo "Files in directory:"
ls -la

echo "=== Verificando directorio de datos ==="
mkdir -p /app/data
ls -la /app/data || true

echo "=== Variables de entorno relacionadas con BD ==="
env | grep -i connection || echo "No hay variables de conexión configuradas"

echo "=== Iniciando aplicación .NET ==="
exec dotnet parcial.dll