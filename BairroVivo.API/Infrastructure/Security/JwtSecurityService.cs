using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BairroVivo.API.Infrastructure.Security;

public interface IJwtSecurityService
{
    IEnumerable<SecurityKey> ObterChavesValidacao();
    SigningCredentials ObterCredenciaisAssinaturaDev();
}

public class JwtSecurityService : IJwtSecurityService
{
    // Chave RSA estatica de 2048 bits para assinatura e validacao de tokens RS256 no ambiente de dev/Swagger
    private static readonly RSA RsaDev = RSA.Create(2048);
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public JwtSecurityService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public IEnumerable<SecurityKey> ObterChavesValidacao()
    {
        var chaves = new List<SecurityKey>();

        // 1. Chave Publica RSA (RS256) fornecida pelo backend Java (via configuracao ou arquivo .pem)
        var chavePublicaPem = _configuration["Jwt:PublicKey"];
        var caminhoChavePublica = _configuration["Jwt:PublicKeyPath"] ?? "public_key.pem";

        if (string.IsNullOrWhiteSpace(chavePublicaPem))
        {
            var caminhos = new[]
            {
                caminhoChavePublica,
                Path.Combine(AppContext.BaseDirectory, caminhoChavePublica),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "BairroVivo.API", caminhoChavePublica)
            };
            foreach (var path in caminhos)
            {
                if (File.Exists(path))
                {
                    chavePublicaPem = File.ReadAllText(path);
                    break;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(chavePublicaPem))
        {
            try
            {
                var rsaJava = RSA.Create();
                rsaJava.ImportFromPem(chavePublicaPem.Replace("\\n", "\n").Trim());
                chaves.Add(new RsaSecurityKey(rsaJava));
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "Nao foi possivel carregar a chave publica RSA configurada em Jwt:PublicKey.");
            }
        }

        // 2. Chave Publica RSA para testes e Swagger (RS256)
        chaves.Add(new RsaSecurityKey(RsaDev));

        // 3. Chave Simetrica de fallback para retrocompatibilidade e testes de integracao XUnit (HS256)
        var secret = _environment.IsEnvironment("Testing")
            ? "somente-testes-xunit-chave-que-nao-deve-ser-usada-fora"
            : _configuration["Jwt:Secret"] ?? "bairrovivo-secret-key-compartilhada-256bits-fiap-gs-2026";

        if (!string.IsNullOrWhiteSpace(secret) && Encoding.UTF8.GetByteCount(secret) >= 32)
        {
            chaves.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)));
        }

        return chaves;
    }

    public SigningCredentials ObterCredenciaisAssinaturaDev()
    {
        var chavePrivada = _configuration["Jwt:PrivateKey"];
        if (!string.IsNullOrWhiteSpace(chavePrivada))
        {
            try
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(chavePrivada.Replace("\\n", "\n").Trim());
                return new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
            }
            catch
            {
                // Fallback para RsaDev
            }
        }

        // Emite token RS256 com chave de desenvolvimento
        return new SigningCredentials(new RsaSecurityKey(RsaDev), SecurityAlgorithms.RsaSha256);
    }
}
