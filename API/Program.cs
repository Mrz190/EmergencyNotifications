using API.Data;
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
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Error()
    .WriteTo.Console()
    .WriteTo.File("logs.log")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExtension(builder.Configuration);

builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.ConnectMongo(builder.Configuration);
builder.Services.ConnectSqlServer(builder.Configuration);

builder.Services.AddControllers();

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


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("Redis").GetValue<string>("ConnectionString");
});

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<IRedisService, RedisService>(); 

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddLogging();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<JwtMiddleware>();

app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:3000"));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v2");
    });
}

app.UseRouting();

app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

        if (!await roleManager.RoleExistsAsync("Member"))
        {
            await roleManager.CreateAsync(new AppRole { Name = "Member" });
        }
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new AppRole { Name = "Admin" });
        }

        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" Application started. ");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        Console.BackgroundColor = ConsoleColor.Red;
        Console.ForegroundColor = ConsoleColor.White;
        logger.LogError(ex, "An error occurred while seeding the database.");
        Console.ResetColor();
    }
}

app.Run();