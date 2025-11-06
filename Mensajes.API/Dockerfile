# 1. Etapa de SDK (Build) - Basada en .NET 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Establece el proyecto principal para este Dockerfile
ARG PROJECT_NAME=Mensajes.API

# Copiar el archivo de la solución
COPY ChatApp.sln .

# Copiar los .csproj de TODOS los proyectos
COPY Usuarios.API/*.csproj Usuarios.API/
COPY Conversaciones.API/*.csproj Conversaciones.API/
COPY Mensajes.API/*.csproj Mensajes.API/

# Restaurar dependencias para toda la solución
RUN dotnet restore ChatApp.sln

# Copiar el resto del código fuente (solo de este proyecto)
COPY ${PROJECT_NAME}/ ${PROJECT_NAME}/

# Publicar la aplicación específica de este Dockerfile
ARG PROJECT_PATH=${PROJECT_NAME}/${PROJECT_NAME}.csproj
RUN dotnet publish ${PROJECT_PATH} -c Release -o /app/publish

# 2. Etapa Final (Runtime) - Basada en ASP.NET 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponer el puerto 8080
EXPOSE 8080

# Entrypoint para correr esta app
ENTRYPOINT ["dotnet", "Mensajes.API.dll"]