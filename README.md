# Proyecto API .net 6 con Arquitectura Limpia 🏗️

Este proyecto es una plantilla genérica para desarrollar APIs en .NET con una arquitectura de capas bien definida. Está diseñado para ser reutilizable y simplificar la creación de nuevas APIs, minimizando la necesidad de configuraciones iniciales.

## Carpetas Principales 📂
El proyecto utiliza una arquitectura de tres capas:

### 1. **Biblioteca.Application 🚀**
   - **Responsabilidad:** Contiene la lógica de negocio y las reglas que controlan el flujo de datos entre la capa de dominio y la infraestructura.
   - **¿Por qué?** Separa las reglas de negocio para que sean independientes del almacenamiento de datos y de la interfaz de usuario.
   - Carpeta UseCases: Cada caso de uso debe estar aquí, representando acciones específicas que la API puede ejecutar (por ejemplo, crear un usuario, autenticar un usuario, etc.).
      Ejemplo de un caso de uso: Crear usuario, obtener usuarios, etc.

### 2. **Biblioteca.Domain 🔑**
   - **Responsabilidad:** Define las entidades principales, interfaces, y contratos del dominio. 
   - **¿Por qué?** Facilita el cumplimiento del principio de "Dominio Rico" y asegura que las reglas de negocio estén correctamente representadas en las entidades.
   - Carpeta Entities: Contiene las clases que representan las entidades de tu base de datos y cualquier lógica relacionada con esas entidades.

### 3. **Biblioteca.Infrastructure 🏗️**
   - **Responsabilidad:** Implementa la interacción con la base de datos y otros sistemas externos.
   - **¿Por qué?** Mantiene el acceso a los datos desacoplado del resto de la aplicación, lo que facilita cambios o mejoras en la infraestructura sin afectar la lógica de negocio.
   - Carpeta Persistence: Contiene la clase AppDbContext.cs, que es la clase encargada de interactuar con la base de datos utilizando Entity Framework Core.
   - Carpeta Services: Aquí se implementan los servicios generales, como aquellos que interactúan con APIs externas o realizan tareas comunes.


### Flujo del Proyecto 🔄
   - El usuario realiza una petición (por ejemplo, para registrarse).
   - El controlador recibe la petición y la pasa al caso de uso correspondiente en la capa de Application.
   - La capa de Application procesa la lógica del negocio utilizando las entidades del Domain.
   - La capa de Infrastructure maneja la persistencia de datos (a través de AppDbContext) o cualquier servicio adicional necesario (como validaciones de contraseñas o autenticación).
   - La respuesta es devuelta al controlador, que la envía al cliente.

### Flujo de Autenticación JWT 🔐
   - **Login de Usuario:**
        - El usuario envía sus credenciales.
        - Si las credenciales son correctas, el servidor genera un JWT que contiene información sobre el usuario.
        - El JWT se envía al cliente.
    
   - **Autenticación en cada petición:**
        - El usuario envía sus credenciales.
        - El cliente incluye el JWT en el encabezado de la solicitud (Authorization: Bearer <token>).
        - El servidor verifica el token en cada solicitud protegida. Si es válido, permite el acceso a la ruta; de lo contrario, rechaza la solicitud.

## Instalación de Dependencias 📦
Estas son las librerías utilizadas y su propósito:

### 1. **BCrypt.Net-Next**
   - **Uso:** Encriptación de contraseñas para asegurar datos sensibles.
   - **Instalación:**
     ```bash
     dotnet add package BCrypt.Net-Next
     ```

### 2. **Microsoft.AspNetCore.Authentication.JwtBearer**
   - **Uso:** Implementación de autenticación basada en JWT (JSON Web Tokens).
   - **Instalación:**
     ```bash
     dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
     ```

### 3. **Microsoft.EntityFrameworkCore**
   - **Uso:** ORM para manejar la interacción con bases de datos de manera eficiente y tipada.
   - **Instalación:**
     ```bash
     dotnet add package Microsoft.EntityFrameworkCore
     ```

### 4. **Microsoft.EntityFrameworkCore.SqlServer**
   - **Uso:** Proveedor específico para bases de datos SQL Server.
   - **Instalación:**
     ```bash
     dotnet add package Microsoft.EntityFrameworkCore.SqlServer
     ```

### 5. **Newtonsoft.Json**
   - **Uso:** Manejo de serialización y deserialización de objetos JSON.
   - **Instalación:**
     ```bash
     dotnet add package Newtonsoft.Json
     ```
---
## Explicación de las Librerías 🧑‍💻:
   - **BCrypt.Net-Next:** Utilizado para la encriptación de contraseñas. Ayuda a mantener las contraseñas de los usuarios seguras mediante un algoritmo de hash.
   - **Microsoft.AspNetCore.Authentication.JwtBearer:** Permite autenticar solicitudes utilizando JWT (JSON Web Tokens). Asegura que solo los usuarios autenticados puedan acceder a ciertas rutas.
   - **Microsoft.EntityFrameworkCore y Microsoft.EntityFrameworkCore.SqlServer:** Son las bibliotecas para interactuar con bases de datos SQL Server a través de Entity Framework Core.
   - **Newtonsoft.Json:** Utilizado para la serialización y deserialización de objetos JSON, muy útil en la interacción con APIs.

## ¿Por Qué Usar Esta Arquitectura? 🤔
   - **Escalabilidad:** Separando las responsabilidades en capas, el proyecto es fácilmente escalable. Puedes agregar nuevas funcionalidades sin afectar el código existente.
   - **Mantenimiento:**  La arquitectura limpia permite un mantenimiento más fácil y rápido. Los cambios en una capa no afectan directamente a otras capas.
   - **Pruebas:** Es más fácil realizar pruebas unitarias debido a que cada capa tiene responsabilidades bien definidas. Puedes probar cada capa por separado sin necesidad de depender de otras capas.
   - **Flexibilidad:** Permite reutilizar las clases y servicios en diferentes proyectos, haciendo que el desarrollo de APIs sea más rápido.


## Instalación y Configuración

### 1. Clonar el repositorio
```bash
git clone <URL_DEL_REPOSITORIO>
cd <NOMBRE_DEL_PROYECTO>
```

### 2. Restaurar las dependencias
```bash
dotnet restore
```

### 3. Configurar la Base de Datos (SQL Server)
- Modifica el archivo `appsettings.json` en el proyecto `Biblioteca.Infrastructure`:
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=<SERVIDOR>;Database=<NOMBRE_BD>;User Id=<USUARIO>;Password=<CONTRASEÑA>;"
    }
  }
  ```

### 4. Crear la Base de Datos
Ejecuta las migraciones para crear las tablas:
```bash
dotnet ef migrations add InitialCreate -p Biblioteca.Infrastructure -s Biblioteca.API
```
```bash
dotnet ef database update -p Biblioteca.Infrastructure -s Biblioteca.API
```

---

## Estructura del Código

### **Controladores (API)**
Los controladores están en `Biblioteca.API` y exponen los endpoints. Ejemplo de un controlador:
```csharp
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly GeneralServices _GeneralServices;
        public HomeController(GeneralServices generalServices)
        {
            _GeneralServices = generalServices;
        }

        [Authorize]
        [HttpGet("ObtenerUsuarios")]
        public async Task<IActionResult> GetUsuario()
        {
            var response = await _GeneralServices.ObtenerData("uspListarUsuarioCsv", "");

            if (response == null)
                return NotFound();

            return Ok(response);
        }
```

### **Servicios (Application)**
La lógica de negocio está en `Biblioteca.Application`:
```csharp
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<bool> RegisterUsuarioAsync(UsuarioDto usuarioDto)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Password);
        var usuario = new Usuario { Nombre = usuarioDto.Nombre, Password = hashedPassword };
        return await _usuarioRepository.AddAsync(usuario);
    }
}
```

### **Repositorio (Infrastructure)**
Acceso a la base de datos en `Biblioteca.Infrastructure`:
```csharp
    public class GeneralServices
    {
        private readonly AppDbContext _context;

        public GeneralServices(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> ObtenerData(string nameProcedure, string dataParameter)
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                try
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = nameProcedure;
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    var param = command.CreateParameter();
                    param.ParameterName = "@data";
                    param.Value = dataParameter;
                    command.Parameters.Add(param);

                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        return reader.GetString(0);
                    }
                    return string.Empty;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ObtenerData: {ex}");
                throw new Exception($"Error al obtener datos para {nameProcedure}", ex);
            }
        }

        public JArray ConvertToJSON(string response)
        {
            var parts = response.Split('°');
            var headers = parts[0].Split('|');
            var types = parts[1].Split('|');
            var data = parts[2].Split('¬');

            var jsonArray = new JArray();

            foreach (var row in data)
            {
                var values = row.Split('|');

                var jsonObject = new JObject();

                for (int i = 0; i < headers.Length && i < values.Length; i++)
                {
                    string value = values[i];
                    string type = types[i];

                    JToken token;
                    switch (type.ToLower())
                    {
                        case "int32":
                            token = int.TryParse(value, out int intValue) ? new JValue(intValue) : new JValue(value);
                            break;
                        case "datetime":
                            token = DateTime.TryParse(value, out DateTime dateValue) ? new JValue(dateValue) : new JValue(value); // format: yy-mm-dd:hh-mm-ss
                            break;
                        case "time":
                            token = TimeSpan.TryParse(value, out TimeSpan timeValue) ? new JValue(timeValue) : new JValue(value); // format: hh-mm-ss:ms
                            break;
                        case "string":
                            token = new JValue(value);
                            break;
                        case "int64":
                            token = long.TryParse(value, out long longValue) ? new JValue(longValue) : new JValue(value);
                            break;
                        case "double":
                            token = double.TryParse(value, out double doubleValue) ? new JValue(doubleValue) : new JValue(value); // Para hacer calculos cientificos 
                            break;
                        case "decimal":
                            token = decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue)
                                ? new JValue(decimalValue)
                                : new JValue(value); // Para hacer cálculos monetarios
                            break;
                        case "boolean":
                            token = bool.TryParse(value, out bool boolValue) ? new JValue(boolValue) : new JValue(value);
                            break;
                        case "float":
                            token = float.TryParse(value, out float floatValue) ? new JValue(floatValue) : new JValue(value);
                            break;
                        case "guid":
                            token = Guid.TryParse(value, out Guid guidValue) ? new JValue(guidValue) : new JValue(value); // ejemplo: "IDUser": "d9b1d7db-5dd7-4f6d-b3ae-8e6c237d6700"
                            break;
                        default:
                            token = new JValue(value);
                            break;
                    }

                    jsonObject[headers[i]] = token;
                }

                jsonArray.Add(jsonObject);
            }

            return jsonArray;
        }


    }
```

---

## Autenticación con JWT

### Configuración en `Startup.cs` o `Program.cs`
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "tu_dominio.com",
            ValidAudience = "tu_dominio.com",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave_secreta"))
        };
    });
```

---

## Ejecución del Proyecto
### 1. Ejecutar la API
```bash
dotnet run --project Biblioteca.API
```

### 2. Acceder a Swagger
Una vez ejecutada la API, abre el navegador y accede a:
```
http://localhost:<PUERTO>/swagger
```

---

## Próximos Pasos
1. Agregar más módulos reutilizables (autorización, auditorías, etc.).
2. Crear pruebas unitarias para asegurar la estabilidad del sistema.
3. Documentar configuraciones avanzadas (caching, logging, etc.).

---

## Contribuciones
¡Se aceptan contribuciones para mejorar esta plantilla! Por favor, crea un *pull request* o abre un *issue* para sugerencias.

---

## Licencia
Este proyecto está licenciado bajo la Licencia MIT. Consulta el archivo `LICENSE` para más detalles.
