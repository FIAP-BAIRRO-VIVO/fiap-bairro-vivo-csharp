using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace BairroVivo.API.Presentation.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public class TokenTesteRequestDto
    {
        public decimal IdUsuario { get; set; } = 1;
        public string Email { get; set; } = "maria@bairrovivo.com";
        public string Perfil { get; set; } = "CIDADAO";
    }

    [HttpPost("token")]
    [SwaggerOperation(
        Summary = "Gerar token JWT para testes no Swagger",
        Description = "Emite um token JWT compatível com o contrato do Auth Service Java (claims sub, userId, perfil e roles) para facilitar testes dos endpoints autenticados direto pelo Swagger."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Token JWT gerado com sucesso.")]
    public IActionResult GerarToken([FromBody] TokenTesteRequestDto dto)
    {
        var chaveJwt = _environment.IsEnvironment("Testing")
            ? "somente-testes-xunit-chave-que-nao-deve-ser-usada-fora"
            : _configuration["Jwt:Secret"] ?? "bairrovivo-secret-key-compartilhada-256bits-fiap-gs-2026";

        var emissor = _configuration["Jwt:Issuer"] ?? "bairro-vivo-java";
        var audiencia = _configuration["Jwt:Audience"] ?? "bairro-vivo-apis";
        var perfil = string.IsNullOrWhiteSpace(dto.Perfil) ? "CIDADAO" : dto.Perfil.Trim().ToUpperInvariant();

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt)),
            SecurityAlgorithms.HmacSha256);

        var expiraEm = DateTime.UtcNow.AddHours(4);
        var claims = new List<Claim>
        {
            new("sub", string.IsNullOrWhiteSpace(dto.Email) ? $"usuario{dto.IdUsuario}@bairrovivo.com" : dto.Email),
            new("userId", dto.IdUsuario.ToString()),
            new("perfil", perfil),
            new("roles", perfil)
        };

        var token = new JwtSecurityToken(
            issuer: emissor,
            audience: audiencia,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            tipo = "Bearer",
            idUsuario = dto.IdUsuario,
            perfil,
            expiraEm
        });
    }
}
