# Dockerfile optimizado para Render.com (sin PostgreSQL)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies first for better caching
COPY *.csproj ./
RUN dotnet restore *.csproj

# Copy source code and build (específicamente el proyecto, no la solución)
COPY . ./
RUN dotnet publish parcial.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Install curl for health checks and create data directories
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /opt/render/project/data \
    && mkdir -p /opt/render/project/data/keys \
    && chmod 755 /opt/render/project/data \
    && chmod 755 /opt/render/project/data/keys

# Copy published app
COPY --from=build /app/publish .

# Create a non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser \
    && chown -R appuser:appgroup /app \
    && chown -R appuser:appgroup /opt/render/project/data \
    && chown -R appuser:appgroup /opt/render/project/data/keys
USER appuser

# Configure environment for Render
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:$PORT/health || exit 1

EXPOSE $PORT
ENTRYPOINT ["dotnet", "parcial.dll"]