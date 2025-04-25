using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace SchoolService.Middleware
{
    public class RedisJwtAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDatabase _redis;

        public RedisJwtAuthMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
        {
            _next = next;
            _redis = redis.GetDatabase();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Missing or invalid Authorization header");
                return;
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userJson = await _redis.StringGetAsync(token);
            if (userJson.IsNullOrEmpty)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid or expired JWT token");
                return;
            }
            // Сохраняем объект пользователя в HttpContext.Items
            context.Items["User"] = userJson.ToString();
            await _next(context);
        }
    }
}
