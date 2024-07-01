using API.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace API.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRedisService _redis;

        public JwtMiddleware(RequestDelegate next, IRedisService redis)
        {
            _next = next;
            _redis = redis;
        }
        
        public async Task Invoke(HttpContext context) 
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                var userName = GetUserNameFromToken(token);
                if (userName != null)
                {
                    using (var scope = context.RequestServices.CreateScope())
                    {
                        var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();
                        var cachedToken = await redisService.GetTokenAsync($"jwt-{userName}");
                        if (cachedToken != token)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Unauthorized.");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }


        private string GetUserNameFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var username = jwtToken?.Claims.First(claim => claim.Type == "unique_name").Value;
            return username;
        }
    }
}
