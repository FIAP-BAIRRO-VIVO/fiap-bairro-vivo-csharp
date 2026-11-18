using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using BairroVivo.API.Infrastructure.Security;

namespace BairroVivo.API.Presentation.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IJwtSecurityService _jwtSecurityService;

    public AuthController(IConfiguration configuration, IJwtSecurityService jwtSecurityService)
    {
        _configuration = configuration;
        _jwtSecurityService = jwtSecurityService;
    }

    public class TokenTesteRequestDto
    {
        public long IdUsuario { get; set; } = 1;
        public string Email { get; set; } = "maria@bairrovivo.com";
        public string Perfil { get; set; } = "CIDADAO";
    }

    [HttpPost("token")]
    [SwaggerOperation(
        Summary = "Gerar token JWT para testes no Swagger",
        Description = "Emite um token JWT compatível com o contrato do Auth Service Java (sub decimal em string, roles, perfil) para testes dos endpoints autenticados direto pelo Swagger."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Token JWT gerado com sucesso.")]
    public IActionResult GerarToken([FromBody] TokenTesteRequestDto dto)
    {
        var emissor = _configuration["Jwt:Issuer"] ?? "bairro-vivo-java";
        var audiencia = _configuration["Jwt:Audience"] ?? "bairro-vivo-apis";
        var perfil = string.IsNullOrWhiteSpace(dto.Perfil) ? "CIDADAO" : dto.Perfil.Trim().ToUpperInvariant();

        var credenciais = _jwtSecurityService.ObterCredenciaisAssinaturaDev();

        var expiraEm = DateTime.UtcNow.AddHours(4);
        var claims = new List<Claim>
        {
            new("sub", dto.IdUsuario.ToString()),
            new("roles", perfil),
            new("email", dto.Email),
            new("perfil", perfil),
            new("userId", dto.IdUsuario.ToString())
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
            idUsuario = dto.IdUsuario.ToString(),
            perfil,
            expiraEm
        });
    }
}
