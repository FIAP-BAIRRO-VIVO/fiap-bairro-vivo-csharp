using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BairroVivo.Domain.Entities;
using BairroVivo.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace BairroVivo.Tests.Integration;

public class BairroVivoApiFactory : WebApplicationFactory<Program>
{
    public const string ChaveTeste = "somente-testes-xunit-chave-que-nao-deve-ser-usada-fora";
    private readonly string _nomeBanco = $"BairroVivoTestes_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationContext>>();
            services.AddDbContext<ApplicationContext>(options =>
                options.UseInMemoryDatabase(_nomeBanco));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            PopularDadosTeste(db);
        });
    }

    public static string CriarToken(long userId, string perfil, string? email = null)
    {
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ChaveTeste)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("sub", userId.ToString()),
            new("userId", userId.ToString()),
            new("email", email ?? $"usuario{userId}@bairrovivo.com"),
            new("perfil", perfil),
            new("roles", perfil)
        };

        var token = new JwtSecurityToken(
            issuer: "bairro-vivo-java",
            audience: "bairro-vivo-apis",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credenciais);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void PopularDadosTeste(ApplicationContext db)
    {
        if (db.Usuarios.Any()) return;

        db.Municipios.Add(new MunicipioEntity
        {
            IdMunicipio = 1,
            CodigoIbge = 3550308,
            Nome = "São Paulo",
            Uf = "SP"
        });

        db.Categorias.AddRange(
            new CategoriaEntity
            {
                IdCategoria = 1,
                Nome = "Buraco na Via",
                Descricao = "Asfalto danificado ou cratera na via pública",
                Icone = "road",
                FlAtiva = "S"
            },
            new CategoriaEntity
            {
                IdCategoria = 2,
                Nome = "Iluminação Pública",
                Descricao = "Poste apagado ou fiação exposta",
                Icone = "lightbulb",
                FlAtiva = "S"
            }
        );

        db.Usuarios.AddRange(
            new UsuarioEntity
            {
                IdUsuario = 1,
                Nome = "Maria Cidadã",
                Email = "usuario1@bairrovivo.com",
                SenhaHash = "hash1",
                Papel = "CIDADAO",
                FlAtivo = "S",
                DtCriacao = DateTime.UtcNow,
                DtAtualizacao = DateTime.UtcNow
            },
            new UsuarioEntity
            {
                IdUsuario = 2,
                Nome = "João Morador",
                Email = "usuario2@bairrovivo.com",
                SenhaHash = "hash2",
                Papel = "CIDADAO",
                FlAtivo = "S",
                DtCriacao = DateTime.UtcNow,
                DtAtualizacao = DateTime.UtcNow
            },
            new UsuarioEntity
            {
                IdUsuario = 3,
                Nome = "Gestor Municipal Admin",
                Email = "admin@bairrovivo.com",
                SenhaHash = "hash3",
                Papel = "ADMIN",
                FlAtivo = "S",
                DtCriacao = DateTime.UtcNow,
                DtAtualizacao = DateTime.UtcNow
            }
        );

        db.Ocorrencias.AddRange(
            new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                IdUsuario = 1,
                IdCategoria = 1,
                IdMunicipio = 1,
                Titulo = "Buraco fundo na Avenida Paulista",
                Descricao = "Cratera na faixa da direita próxima ao MASP causando risco a ciclistas.",
                Status = "ABERTA",
                NumCiclo = 1,
                FlExcluida = "N",
                Latitude = -23.5614m,
                Longitude = -46.6558m,
                EnderecoReferencia = "Avenida Paulista, 1578 - Bela Vista",
                FlAnonimo = "N",
                DtCriacao = DateTime.UtcNow.AddDays(-2),
                DtAtualizacao = DateTime.UtcNow.AddDays(-2)
            },
            new OcorrenciaEntity
            {
                IdOcorrencia = 2,
                IdUsuario = 2,
                IdCategoria = 2,
                IdMunicipio = 1,
                Titulo = "Poste sem iluminação na Rua dos Pinheiros",
                Descricao = "Três postes seguidos apagados durante a noite.",
                Status = "RESOLVIDA",
                NumCiclo = 1,
                FlExcluida = "N",
                Latitude = -23.5665m,
                Longitude = -46.6908m,
                EnderecoReferencia = "Rua dos Pinheiros, 400 - Pinheiros",
                FlAnonimo = "S",
                DtCriacao = DateTime.UtcNow.AddDays(-1),
                DtAtualizacao = DateTime.UtcNow.AddDays(-1)
            }
        );

        db.SaveChanges();
    }
}
