using API.Entity;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Middleware;
using API.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Logging configuration
#if RELEASE
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() 
    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.File("logs.log", restrictedToMinimumLevel: LogEventLevel.Warning)
    .CreateLogger();
#endif
#if DEBUG
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
    .WriteTo.File("logs.log", restrictedToMinimumLevel: LogEventLevel.Warning)
    .CreateLogger();
#endif

builder.Host.UseSerilog();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod()
               .WithExposedHeaders("*");
    });
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExtension(builder.Configuration);

// IdentityServer, Mongo, PostgreSQL
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.ConnectMongo(builder.Configuration);
builder.Services.ConnectPostgreSQL(builder.Configuration);
//builder.Services.ConnectSqlServer(builder.Configuration);

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("Redis").GetValue<string>("ConnectionString");
});

// Mail
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.WebHost.UseUrls("http://+:5041");

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAllOrigins");

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<JwtMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v2");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        Console.WriteLine("RoleManager initialized.");

        var roles = new[] { "Member", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                Console.WriteLine($"Creating role: {role}");
                var result = await roleManager.CreateAsync(new AppRole { Name = role });
                if (result.Succeeded)
                {
                    Console.WriteLine($"Role {role} created successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to create role {role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"Role {role} already exists.");
            }
        }

        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Application started.");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        logger?.LogError(ex, "An error occurred while seeding the database.");
        Console.WriteLine($"Seeding failed: {ex.Message}");
    }
}

app.Run();