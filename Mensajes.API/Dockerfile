# 1. Etapa de SDK (Build) - Basada en .NET 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
ARG PROJECT_NAME=Mensajes.API
ARG PROJECT_PATH=${PROJECT_NAME}/${PROJECT_NAME}.csproj

# Copiar archivos de solución y proyecto
COPY ChatApp.sln .
COPY ${PROJECT_NAME}/*.csproj ${PROJECT_NAME}/

# Restaurar dependencias
RUN dotnet restore ChatApp.sln

# Copiar el resto del código fuente del proyecto
COPY ${PROJECT_NAME}/ ./ ${PROJECT_NAME}/

# Publicar la aplicación
RUN dotnet publish ${PROJECT_PATH} -c Release -o /app/publish

# 2. Etapa Final (Runtime) - Basada en ASP.NET 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponer el puerto 8080 (puerto estándar de contenedores .NET)
EXPOSE 8080

# Entrypoint para correr la app
ENTRYPOINT ["dotnet", "Mensajes.API.dll"]