using Auth0.Auth0;
using AuthService;
using AuthService.Grpc;
using AuthService.Services.Abstract;
using Database;
using Database.DbContext;
using Message_Queue;
using Microservice.Communications.Extensions;
using Microservice.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Redis;
using System.Text;
using Services;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Auth0 Configuration

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddRabbitMqServices(builder.Configuration);
builder.Services.AddGrpcAuthServiceExtensions(builder.Configuration);
builder.Services.AddGrpc();
builder.Services.AddAuth0ServiceExtension(builder.Configuration);
builder.Services.AddCustomFluentValidation();
builder.Services.AddServices();
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionString")));
builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();

builder.Services.AddAuthentication()
    .AddJwtBearer("Auth0", options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
        options.Audience = builder.Configuration["Auth0:Audience"];
    })
    .AddJwtBearer("Internal", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
        };
    });


builder.Services.AddHttpClient();
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService API", Version = "v1" });

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
        //Internal gRPC for microservices(HTTP / 2, no TLS)

        options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });

        //REST + Swagger port(HTTP / 1.1 + HTTP / 2, no TLS)

        options.ListenAnyIP(7230, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
        // No UseHttps() -> plain HTTP
    });
    });
}
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<AuthGrpcService>();
app.Run();
