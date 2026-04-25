# CréditosSeguros - Evaluación Parcial

Plataforma web interna para gestionar solicitudes de crédito de clientes. Desarrollado con ASP.NET Core MVC 8, Identity, EF Core (SQLite) y Redis.

## 🚀 Despliegue en Render (Enlace)

- **URL de la Aplicación:** `[https://tu-app-en-render.onrender.com]` *(Reemplazar con la URL final)*

---

## 🛠️ Requisitos Previos (Ejecución Local)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Una cuenta en [Redis Labs](https://app.redislabs.com/) (gratuita) o un servidor Redis local.

## 💻 Pasos Locales

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/Lezamito/Examen-Parcial---Lezama.git
   cd Examen-Parcial---Lezama
   ```

2. **Configurar las variables locales:**
   Asegúrate de tener configurada tu cadena de conexión a Redis en el archivo `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=creditos.db",
     "RedisConnection": "tu_endpoint_redis,password=tu_password"
   }
   ```

3. **Restaurar paquetes y compilar:**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Migraciones y Base de Datos:**
   No necesitas ejecutar comandos de migración manualmente. Al iniciar el proyecto por primera vez, `Program.cs` y `DbSeeder.cs` se encargan de:
   - Crear la base de datos SQLite (`creditos.db`).
   - Aplicar las migraciones.
   - Insertar los datos semilla (2 clientes, solicitudes y usuario Analista).

5. **Ejecutar el proyecto:**
   ```bash
   dotnet run
   ```

---

## 🌍 Variables de Entorno (Producción en Render)

Al crear el **Web Service** en Render (usando Docker), debes configurar las siguientes variables de entorno:

| Variable | Valor sugerido | Descripción |
|----------|----------------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Indica a .NET que corra en modo producción |
| `ASPNETCORE_URLS` | `http://0.0.0.0:${PORT}` | Enlaza la app al puerto dinámico asignado por Render |
| `ConnectionStrings__DefaultConnection` | `Data Source=creditos.db` | Cadena de conexión para SQLite |
| `Redis__ConnectionString` | `tu_endpoint,password=tu_password` | Cadena de conexión para Redis Labs |

## 👥 Usuarios de Prueba (Generados automáticamente)

**Rol Analista:**
- **Email:** analista@creditos.com
- **Contraseña:** Analista@123

**Rol Cliente:**
- **Email:** juan.perez@cliente.com
- **Contraseña:** Cliente@123
- **Email:** maria.garcia@cliente.com
- **Contraseña:** Cliente@123
