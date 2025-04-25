using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using SchoolService.Data;
using SchoolService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:5002", "https://*:5003");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SchoolService API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// PostgreSQL 
builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Redis 
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(
    builder.Configuration.GetConnectionString("Redis")));

// JWT Auth 
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSection["Key"]!);


// DI сервисы для бизнес-логики
builder.Services.AddScoped<SchoolSer>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<ClassService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<SchoolService.Middleware.RedisJwtAuthMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
