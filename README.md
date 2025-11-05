### 🖥️ 1. Corriendo proyectos individualmente (sin Docker)

Cuando ejecutas `dotnet run` en cada proyecto, estos son los puertos por defecto (perfil `http`):

| API | Puerto HTTP | Puerto HTTPS |
| :--- | :--- | :--- |
| **Usuarios.API** | `http://localhost:5260` | `https://localhost:7106` |
| **Grupos.API** | `http://localhost:5217` | `https://localhost:7189` |
| **Mensajes.API** | `http://localhost:5233` | `https://localhost:7274` |

Para iniciar cada API individualmente (asegúrate de estar en la raíz del repositorio):

```bash
# Terminal 1: Iniciar Usuarios.API
dotnet run --project Usuarios.API/Usuarios.API.csproj --launch-profile http

# Terminal 2: Iniciar Grupos.API
dotnet run --project Grupos.API/Grupos.API.csproj --launch-profile http

# Terminal 3: Iniciar Mensajes.API
dotnet run --project Mensajes.API/Mensajes.API.csproj --launch-profile http
```

-----

### 🐳 2. Corriendo con Docker Compose

Cuando ejecutas `docker-compose up`, los servicios se exponen de la siguiente manera:

| Servicio | Puerto Externo | Puerto Interno | URL desde tu PC | URL entre contenedores |
| :--- | :--- | :--- | :--- | :--- |
| `usuarios-api` | `5260` | `8080` | `http://localhost:5260` | `http://usuarios-api:8080` |
| `grupos-api` | `5217` | `8080` | `http://localhost:5217` | `http://grupos-api:8080` |
| `mensajes-api` | `5233` | `8080` | `http://localhost:5233` | `http://mensajes-api:8080` |
| `db-chatapp` | `3306` | `3306` | `localhost:3306` | `db-chatapp:3306` |
