using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using BairroVivo.API.Application.Interfaces;
using BairroVivo.API.Application.UseCases;
using BairroVivo.API.Domain.Interfaces;
using BairroVivo.API.Infrastructure.Data.Repositories;
using BairroVivo.API.Infrastructure.Health;
using BairroVivo.API.Presentation.Filters;
using BairroVivo.Application.Interfaces;
using BairroVivo.Application.UseCases;
using BairroVivo.Infrastructure.Data;
using BairroVivo.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Enrichers.Span;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsEnvironment("Testing"))
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithSpan()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [TraceId:{TraceId}] [SpanId:{SpanId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<OcorrenciaAccessFilter>();
    options.Filters.AddService<IdempotencyFilter>();
})
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var erros = context.ModelState
                .Where(campo => campo.Value?.Errors.Count > 0)
                .ToDictionary(
                    campo => campo.Key,
                    campo => campo.Value!.Errors.Select(erro => erro.ErrorMessage).ToArray());

            return new BadRequestObjectResult(new
            {
                mensagem = "Dados inválidos.",
                erros
            });
        };
    });
builder.Services.AddScoped<OcorrenciaAccessFilter>();
builder.Services.AddScoped<IdempotencyFilter>();

// Rate Limiting (Throttling) conforme diretrizes da Global Solution 2026/2
var permitLimit = builder.Configuration.GetValue<int?>("RateLimit:PermitLimit") ?? 60;
var windowSeconds = builder.Configuration.GetValue<int?>("RateLimit:WindowSeconds") ?? 60;
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            mensagem = "Limite de requisições excedido (Rate Limiting). Tente novamente em instantes."
        }, token);
    };
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "global";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromSeconds(windowSeconds),
            QueueLimit = 0
        });
    });
});

// Swagger com esquema Bearer para testar tokens emitidos pelo Java Auth Service
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bairro Vivo API (.NET 8)",
        Version = "v1",
        Description = "API de Zeladoria Urbana Participativa e Votação Comunitária (CitizenTech) integrada ao Oracle e ao ecossistema Bairro Vivo."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT emitido pelo Auth Service Java."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Banco de dados Oracle
var connectionString = builder.Configuration.GetConnectionString("OracleConnection")
    ?? builder.Configuration.GetConnectionString("Oracle")
    ?? string.Empty;

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseOracle(connectionString, oracleOptions =>
        oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion21)));

// JWT compartilhado com o Auth Service em Java (HS256)
var chaveJwt = builder.Environment.IsEnvironment("Testing")
    ? "somente-testes-xunit-chave-que-nao-deve-ser-usada-fora"
    : builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(chaveJwt) || Encoding.UTF8.GetByteCount(chaveJwt) < 32)
    throw new InvalidOperationException("Configure Jwt:Secret com pelo menos 32 bytes antes de iniciar a API.");

var emissorJwt = builder.Configuration["Jwt:Issuer"] ?? "bairro-vivo-java";
var audienciaJwt = builder.Configuration["Jwt:Audience"] ?? "bairro-vivo-apis";
var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = emissorJwt,
            ValidateAudience = true,
            ValidAudience = audienciaJwt,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = chaveSimetrica,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// Repositories
builder.Services.AddScoped<IOcorrenciaRepository, OcorrenciaRepository>();
builder.Services.AddScoped<IOcorrenciaAccessService, OcorrenciaAccessService>();
builder.Services.AddScoped<IAtualizacaoRepository, AtualizacaoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IMunicipioRepository, MunicipioRepository>();
builder.Services.AddScoped<IRelatorioBairroRepository, RelatorioBairroRepository>();
builder.Services.AddScoped<IIntegracaoOracleRepository, IntegracaoOracleRepository>();

// UseCases
builder.Services.AddScoped<IOcorrenciaUseCase, OcorrenciaUseCase>();
builder.Services.AddScoped<IAtualizacaoUseCase, AtualizacaoUseCase>();
builder.Services.AddScoped<ICategoriaUseCase, CategoriaUseCase>();
builder.Services.AddScoped<IMunicipioUseCase, MunicipioUseCase>();
builder.Services.AddScoped<IRelatorioBairroUseCase, RelatorioBairroUseCase>();
builder.Services.AddScoped<IIntegracaoOracleUseCase, IntegracaoOracleUseCase>();

// Servicos externos e HealthChecks
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API em execucao."), tags: new[] { "live" })
    .AddOracle(connectionString, name: "oracle", tags: new[] { "db", "ready" })
    .AddCheck<ViaCepHealthCheck>("viacep", tags: new[] { "external", "ready" });

var app = builder.Build();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var erro = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var (status, mensagem) = erro switch
        {
            ArgumentException ex => (StatusCodes.Status400BadRequest, ex.Message),
            KeyNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            UnauthorizedAccessException ex => (StatusCodes.Status403Forbidden, ex.Message),
            InvalidOperationException ex => (StatusCodes.Status409Conflict, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno da API.")
        };

        if (status == StatusCodes.Status500InternalServerError && erro != null)
            Log.Error(erro, "Falha nao tratada em {Metodo} {Caminho}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { mensagem });
    });
});

app.UseSerilogRequestLogging();

// Swagger disponivel para inspecao e homologacao da API
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bairro Vivo API v1");
    c.RoutePrefix = string.Empty;
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
MapearHealthChecks(app);

app.Run();

static void MapearHealthChecks(WebApplication app)
{
    app.MapHealthChecks("/health/live", CriarOpcoes(r => r.Tags.Contains("live"))).AllowAnonymous();
    app.MapHealthChecks("/health/db", CriarOpcoes(r => r.Tags.Contains("db"))).AllowAnonymous();
    app.MapHealthChecks("/health/external", CriarOpcoes(r => r.Tags.Contains("external"))).AllowAnonymous();
    app.MapHealthChecks("/health/ready", CriarOpcoes(r => r.Tags.Contains("ready"))).AllowAnonymous();
}

static HealthCheckOptions CriarOpcoes(Func<HealthCheckRegistration, bool> filtro) => new()
{
    Predicate = filtro,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
    }
};

public partial class Program { }
