using System.IdentityModel.Tokens.Jwt;
using AsistenciaProcess.Models;
using Microsoft.EntityFrameworkCore;
using Dapper;
using MySql.Data.MySqlClient;

namespace AsistenciaProcess.Middlewares
{
    public class ValidateUser
    {
        private readonly RequestDelegate _next;
        private readonly string _connectionString;

        public ValidateUser(RequestDelegate next, string connectionString)
        {
            _next = next;
            _connectionString = connectionString;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(' ').Last();
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Token no proporcionado.");
                return;
            }
            //var accessToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            //Console.WriteLine("aqui ->" + token);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var TokenClaimOid = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value ?? "";
            using(var db = new MySqlConnection(_connectionString))
            {
                var queryValidate = $"SELECT COUNT(*) FROM Usuario WHERE EntraId='{TokenClaimOid}'";
                int count = await db.ExecuteScalarAsync<int>(queryValidate);
                int result = 0;

                if (count == 0)
                {
                    Usuario newUser = new Usuario();
                    newUser.EntraId = TokenClaimOid;
                    newUser.Rol = jwtToken.Claims.FirstOrDefault(c => c.Type == "roles")?.Value ?? "";
                    newUser.Email = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "";
                    newUser.Nombre = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";
                    newUser.Id = "T000";
                    newUser.Apellido = "";
                    Console.WriteLine($"usuario nuevo -{newUser.EntraId}");
                    var querySql = "INSERT INTO Usuario VALUES(@Id,@Nombre,@Apellido,@Rol,@Email,@EntraId)";
                    result = await db.ExecuteAsync(querySql, newUser);
                }
            }
            
            await _next(context);
        }
    }
}
