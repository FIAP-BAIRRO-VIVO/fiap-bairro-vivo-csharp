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

    public static string CriarToken(decimal userId, string perfil, string? email = null)
    {
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ChaveTeste)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("sub", email ?? $"usuario{userId}@bairrovivo.com"),
            new("userId", userId.ToString()),
            new("perfil", perfil),
            new("roles", perfil)
        };

        var token = new JwtSecurityToken(
            issuer: "bairro-vivo-java",
            audience: "bairro-vivo-apis",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credenciais);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void PopularDadosTeste(ApplicationContext db)
    {
        if (db.Usuarios.Any()) return;

        db.Municipios.Add(new MunicipioEntity
        {
            IdMunicipio = 1,
            CodigoIbge = "3550308",
            Nome = "São Paulo",
            Uf = "SP",
            Ativo = "S"
        });

        db.Categorias.AddRange(
            new CategoriaEntity
            {
                IdCategoria = 1,
                Nome = "Buraco na Via",
                Descricao = "Asfalto danificado ou cratera na via pública",
                Icone = "road",
                CorHex = "#E53935",
                SlaDiasPadrao = 10,
                Ativo = "S"
            },
            new CategoriaEntity
            {
                IdCategoria = 2,
                Nome = "Iluminação Pública",
                Descricao = "Poste apagado ou fiação exposta",
                Icone = "lightbulb",
                CorHex = "#FDD835",
                SlaDiasPadrao = 7,
                Ativo = "S"
            }
        );

        db.Usuarios.AddRange(
            new UsuarioEntity
            {
                IdUsuario = 1,
                Nome = "Maria Cidadã",
                Email = "usuario1@bairrovivo.com",
                SenhaHash = "hash",
                Bairro = "Bela Vista",
                IdMunicipio = 1,
                Perfil = "CIDADAO",
                Pontos = 50,
                Ativo = "S",
                CriadoEm = DateTime.Now
            },
            new UsuarioEntity
            {
                IdUsuario = 2,
                Nome = "João Morador",
                Email = "usuario2@bairrovivo.com",
                SenhaHash = "hash",
                Bairro = "Pinheiros",
                IdMunicipio = 1,
                Perfil = "CIDADAO",
                Pontos = 30,
                Ativo = "S",
                CriadoEm = DateTime.Now
            },
            new UsuarioEntity
            {
                IdUsuario = 3,
                Nome = "Gestor Municipal Admin",
                Email = "admin@bairrovivo.com",
                SenhaHash = "hash",
                Bairro = "Centro",
                IdMunicipio = 1,
                Perfil = "ADMIN",
                Pontos = 100,
                Ativo = "S",
                CriadoEm = DateTime.Now
            }
        );

        db.Ocorrencias.AddRange(
            new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                Protocolo = "BV-2026-000001",
                IdUsuario = 1,
                IdCategoria = 1,
                IdMunicipio = 1,
                Titulo = "Buraco fundo na Avenida Paulista",
                Descricao = "Cratera na faixa da direita próxima ao MASP causando risco a ciclistas.",
                Status = "ABERTA",
                Urgencia = "ALTA",
                Latitude = -23.5614m,
                Longitude = -46.6558m,
                Logradouro = "Avenida Paulista, 1578",
                Bairro = "Bela Vista",
                Cep = "01310-200",
                Confirmacoes = 2,
                ValidadaComunidade = "N",
                CategoriaSugeridaIa = "Buraco na Via",
                ConfiancaIa = 0.945m,
                CriadaEm = DateTime.Now.AddDays(-2),
                SlaLimite = DateTime.Now.AddDays(8)
            },
            new OcorrenciaEntity
            {
                IdOcorrencia = 2,
                Protocolo = "BV-2026-000002",
                IdUsuario = 2,
                IdCategoria = 2,
                IdMunicipio = 1,
                Titulo = "Poste sem iluminação na Rua dos Pinheiros",
                Descricao = "Três postes seguidos apagados durante a noite.",
                Status = "EM_ANDAMENTO",
                Urgencia = "MEDIA",
                Latitude = -23.5665m,
                Longitude = -46.6908m,
                Logradouro = "Rua dos Pinheiros, 400",
                Bairro = "Pinheiros",
                Cep = "05422-000",
                Confirmacoes = 1,
                ValidadaComunidade = "N",
                CriadaEm = DateTime.Now.AddDays(-1),
                SlaLimite = DateTime.Now.AddDays(6)
            }
        );

        db.RelatoriosBairro.Add(new RelatorioBairroEntity
        {
            IdRelatorio = 1,
            IdMunicipio = 1,
            Bairro = "Bela Vista",
            PeriodoRef = "2026-02",
            TotalOcorrencias = 14,
            TotalResolvidas = 9,
            ResumoIa = "Prioridade de manutenção viária concentrada na Avenida Paulista e entorno.",
            ModeloIa = "gemini-1.5-flash",
            GeradoEm = DateTime.Now
        });

        db.SaveChanges();
    }
}
