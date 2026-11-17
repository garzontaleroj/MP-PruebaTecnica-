# Taskflow — prueba técnica Fullstack

Aplicación interna para crear colaboradores, asignarles tareas, filtrar tareas por estado y avanzar su ciclo de trabajo. La solución usa ASP.NET Core Web API, Angular y SQL Server.

## Requisitos

- .NET SDK 10
- Node.js 20.19+ o 22.12+
- SQL Server 2017+ (local, Express o una instancia accesible)
- SQL Server Command Line Utilities (`sqlcmd`) para ejecutar el script

## Puesta en marcha

1. Crea la base de datos y sus tablas:

   ```powershell
   sqlcmd -S tcp:localhost,1433 -E -b -i .\database\01_create_database.sql
   ```

   Si tu instancia local usa autenticación de SQL Server, reemplaza `-E` por `-U <usuario> -C`. Sin `-P`, `sqlcmd` solicita la contraseña si `SQLCMDPASSWORD` no está definida. `-C` confía en el certificado del servidor y se usa aquí solo para desarrollo local.

2. Si usas autenticación de SQL Server en `localhost:1433`, inicia la API desde PowerShell:

   ```powershell
   .\Start-TaskflowApi.ps1
   ```

   El script solicita solo la contraseña de `sa` y no la escribe en el repositorio. Para usar una cuenta con permisos limitados (recomendado fuera de esta prueba), ejecuta `.\Start-TaskflowApi.ps1 -SqlUser tuUsuario`. Si el log muestra `Login failed for user ''`, detén la API con `Ctrl+C` y vuelve a iniciarla con el script.

   Si tu instancia usa autenticación integrada de Windows, en lugar del script ejecuta:

   ```powershell
   dotnet run --project .\backend\TaskManagement.Api
   ```

   Espera a que aparezca `Now listening on: http://localhost:5000` y comprueba que <http://localhost:5000/api/tasks> devuelve JSON. Mantén la terminal abierta mientras usas Angular. Si la página ya estaba abierta antes de iniciar la API, recárgala.

3. En otra terminal, instala dependencias e inicia Angular:

   ```powershell
   cd .\frontend\task-management-ui
   npm install
   npm start
   ```

4. Abre <http://localhost:4200>. Crea un colaborador desde el panel «Colaboradores» y luego asígnale tareas.

La URL de la API se define en `frontend\task-management-ui\src\environments\environment.ts`; su origen se permite mediante `AllowedOrigins` en la configuración de la API. El script SQL es idempotente para la creación inicial de tablas e índices existentes; no elimina datos.

## Funcionalidades

- Crear y listar usuarios, validando nombre, formato de correo y correo duplicado.
- Crear y listar tareas asignadas a colaboradores.
- Filtrar tareas por estado o usuario; se ordenan de más recientes a más antiguas.
- Cambiar estado entre `Pending`, `InProgress` y `Done`. Una tarea pendiente no puede pasar directamente a completada.
- Guardar prioridad, fecha estimada, etiquetas y metadatos libres en `Tasks.AdditionalData` (`nvarchar(max)`).
- Validar JSON tanto en la API (serialización del DTO) como en SQL Server (`ISJSON`).
- Mostrar errores y confirmaciones básicos en la interfaz.

## API

| Método | Ruta | Uso |
|---|---|---|
| `POST` | `/api/users` | Crear usuario |
| `GET` | `/api/users` | Listar usuarios |
| `POST` | `/api/tasks` | Crear tarea |
| `GET` | `/api/tasks?status=Pending&userId={guid}` | Listar / filtrar tareas |
| `GET` | `/api/tasks/{id}` | Consultar tarea |
| `PUT` | `/api/tasks/{id}/status` | Cambiar estado |

Las respuestas de error de negocio usan JSON con `code`, `status` y `message`; los errores inesperados se registran y responden como `500 Problem Details`. Hay ejemplos de llamadas en `backend\TaskManagement.Api\TaskManagement.Api.http`.

## Decisiones técnicas

- La API separa controladores, servicios, repositorios y persistencia EF Core. Los controladores solo traducen HTTP; reglas de negocio y validaciones viven en los servicios.
- Se usan DTOs para no exponer directamente las entidades de persistencia. Los correos son únicos y las tareas tienen clave foránea a usuarios.
- EF Core y el script SQL describen el mismo esquema. La API no ejecuta migraciones al arrancar: el esquema se crea explícitamente con el script entregado.
- El índice `(UserId, Status, CreatedAt)` apoya las consultas de asignación y filtrado; `CreatedAt` se ordena de forma descendente.
- El JSON de metadata se almacena como `nvarchar(max)` y una restricción `CHECK` evita valores malformados. El script muestra `ISJSON`, `JSON_VALUE`, `JSON_QUERY` y `OPENJSON`, además de un ejemplo opcional con `JSON_MODIFY`.
- No se incluye autenticación porque no forma parte del alcance de la prueba.

## Pruebas y compilación

```powershell
dotnet build .\backend\TaskManagement.Api\TaskManagement.Api.csproj
cd .\frontend\task-management-ui
npm run build
```

La integración de persistencia requiere una instancia de SQL Server con el script ejecutado.

## Pendientes / mejoras

- Agregar pruebas unitarias de servicios y pruebas de integración HTTP con SQL Server de pruebas.
- Incorporar paginación y búsqueda cuando crezca el volumen de tareas.
- Agregar autenticación/autorización y permisos por colaborador.
- Añadir actualización de título, descripción y asignación, además del cambio de estado.
- Automatizar despliegue y configuración segura de secretos por ambiente.
