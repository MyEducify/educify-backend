using Microservice.Communications.Extensions;
using Microservice.Middleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Redis;
using UserService;
using UserService.DBContext;
using UserService.Grpc;


AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionString")));

builder.Services.AddGrpcUserServiceExtensions(builder.Configuration);
builder.Services.AddRedis(builder.Configuration);
builder.Services
    .AddAuthentication("DelegatedJwt")
    .AddScheme<AuthenticationSchemeOptions, DelegatedJwtHandler>("DelegatedJwt", null);

builder.Services.AddAuthorization();
builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserService API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

if (builder.Configuration["environment"] != "local")
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        // Internal gRPC for microservices (HTTP/2, no TLS)
        options.ListenAnyIP(5000, listenOptions =>
        {
            listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        });

        // REST + Swagger port (HTTP/1.1 + HTTP/2, no TLS)
        options.ListenAnyIP(7202, listenOptions =>
        {
            listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
            //No UseHttps() -> plain HTTP
        });
    });
}
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<UserServiceImpl>();
app.MapGrpcService<UserGrpcService>();
app.UseHttpsRedirection();

app.Run();
