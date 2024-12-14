# Documentación del Proyecto: Implementación de JWT

## Descripción del Proyecto
Este proyecto implementa la autenticación y autorización mediante **JSON Web Token (JWT)** para un sistema basado en .NET Core. Se busca garantizar la seguridad de las solicitudes HTTP, utilizando un token que valida la identidad del usuario en rutas protegidas.

## Índice
- [Requisitos Previos](#requisitos-previos)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Instalación de Librerías](#instalación-de-librerías)
- [Capa por Capa](#capa-por-capa)
  - [Capa de Backend](#capa-de-backend)
  - [Middleware](#middleware)
  - [Frontend](#frontend)
- [Flujo del Sistema](#flujo-del-sistema)
- [Pruebas y Validación](#pruebas-y-validación)
- [Conclusión](#conclusión)

---

## Requisitos Previos

1. **Herramientas Necesarias:**
   - **Visual Studio** o **Visual Studio Code** para el desarrollo backend.
   - **Node.js y npm** para pruebas frontend si se utiliza React o Angular.
   - Postman o cualquier herramienta para probar APIs.
2. **Librerías Específicas:**
   - **Microsoft.AspNetCore.Authentication.JwtBearer** para .NET.
   - (Opcional) Axios para realizar solicitudes HTTP desde el cliente.
3. **Conocimientos Básicos:**
   - Principios de arquitectura de software por capas.
   - Conceptos de autenticación y autorización.

---

## Estructura del Proyecto
El proyecto está estructurado en tres capas principales:

1. **Capa de Backend:**
   - Se encarga de gestionar la lógica del servidor, generar y validar tokens JWT.
2. **Middleware:**
   - Intercepta las solicitudes para aplicar validaciones y autorización antes de llegar al controlador.
3. **Frontend:**
   - Se encarga de interactuar con los usuarios y enviar el token en las solicitudes protegidas.

---

## Instalación de Librerías

### Backend (.NET Core)

1. **Configurar el Proyecto Backend:**
   ```bash
   dotnet new webapi -n JwtAuthExample
   cd JwtAuthExample
   ```

2. **Instalar Dependencias JWT:**
   ```bash
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   ```

3. **Instalar Dependencias Adicionales:**
   ```bash
   dotnet add package Microsoft.IdentityModel.Tokens
   ```

### Frontend (Opcional, por ejemplo, React)

1. **Crear el Proyecto React:**
   ```bash
   npx create-react-app jwt-auth-client
   cd jwt-auth-client
   ```

2. **Instalar Axios para manejar peticiones HTTP:**
   ```bash
   npm install axios
   ```

---

## Capa por Capa

### Capa de Backend

1. **Generación del Token JWT:**
   En el controlador de autenticación, se genera el token cuando el usuario inicia sesión correctamente:
   ```csharp
   using System.IdentityModel.Tokens.Jwt;
   using System.Security.Claims;
   using Microsoft.IdentityModel.Tokens;
   
   public class AuthController : ControllerBase
   {
       [HttpPost("login")]
       public IActionResult Login(UserLoginDto user)
       {
           if (user.Username == "admin" && user.Password == "password")
           {
               var claims = new[]
               {
                   new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
               };

               var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave_secreta_super_segura"));
               var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

               var token = new JwtSecurityToken(
                   issuer: "tu_dominio.com",
                   audience: "tu_dominio.com",
                   claims: claims,
                   expires: DateTime.Now.AddMinutes(30),
                   signingCredentials: creds
               );

               return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
           }
           return Unauthorized();
       }
   }
   ```

2. **Configuración del Middleware para Validación de Tokens:**
   En el archivo `Program.cs` o `Startup.cs`:
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
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave_secreta_super_segura"))
           };
       });

   app.UseAuthentication();
   app.UseAuthorization();
   ```

### Middleware

El Middleware valida las solicitudes entrantes antes de que lleguen a los controladores protegidos. Si el token no es válido o está ausente, la solicitud es rechazada automáticamente con un código **401 Unauthorized**.

### Frontend

1. **Guardar y Usar el Token:**
   Cuando el usuario inicia sesión, el cliente guarda el token en un lugar seguro (por ejemplo, LocalStorage o Cookies).
   ```javascript
   import axios from 'axios';

   const login = async () => {
       const response = await axios.post('http://localhost:5000/login', {
           username: 'admin',
           password: 'password',
       });
       localStorage.setItem('token', response.data.token);
   };
   ```

2. **Enviar el Token en las Solicitudes:**
   ```javascript
   const getProtectedData = async () => {
       const token = localStorage.getItem('token');
       const response = await axios.get('http://localhost:5000/protected', {
           headers: {
               Authorization: `Bearer ${token}`,
           },
       });
       console.log(response.data);
   };
   ```

---

## Flujo del Sistema

1. El cliente envía las credenciales al backend.
2. El backend valida las credenciales y genera un JWT.
3. El cliente almacena el JWT y lo envía en las solicitudes subsecuentes.
4. El middleware valida el JWT antes de permitir el acceso a las rutas protegidas.
5. Si el token es válido, la solicitud se procesa; si no, se rechaza.

---

## Pruebas y Validación

1. **Usar Postman:**
   - Hacer una petición POST a `/login` con credenciales válidas y recibir el token.
   - Usar el token recibido para acceder a rutas protegidas con un encabezado `Authorization: Bearer <token>`.

2. **Errores Comunes:**
   - Token expirado.
   - Token inválido debido a una clave secreta incorrecta.

---

## Conclusión
La autenticación y autorización con JWT es una solución eficaz y escalable para sistemas distribuidos. Esta arquitectura asegura que solo los usuarios autenticados puedan acceder a los recursos protegidos, manteniendo el sistema seguro y bien estructurado.
