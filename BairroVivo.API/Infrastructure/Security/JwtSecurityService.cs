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

        if (string.IsNullOrWhiteSpace(chavePublicaPem) && File.Exists(caminhoChavePublica))
        {
            chavePublicaPem = File.ReadAllText(caminhoChavePublica);
        }

        if (!string.IsNullOrWhiteSpace(chavePublicaPem))
        {
            try
            {
                var rsaJava = RSA.Create();
                rsaJava.ImportFromPem(chavePublicaPem.Trim());
                chaves.Add(new RsaSecurityKey(rsaJava) { KeyId = "java-rs256-key" });
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "Nao foi possivel carregar a chave publica RSA configurada em Jwt:PublicKey.");
            }
        }

        // 2. Chave Publica RSA para testes e Swagger (RS256)
        chaves.Add(new RsaSecurityKey(RsaDev) { KeyId = "dev-rs256-key" });

        // 3. Chave Simetrica de fallback para retrocompatibilidade e testes de integracao XUnit (HS256)
        var secret = _environment.IsEnvironment("Testing")
            ? "somente-testes-xunit-chave-que-nao-deve-ser-usada-fora"
            : _configuration["Jwt:Secret"] ?? "bairrovivo-secret-key-compartilhada-256bits-fiap-gs-2026";

        if (!string.IsNullOrWhiteSpace(secret) && Encoding.UTF8.GetByteCount(secret) >= 32)
        {
            chaves.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)) { KeyId = "fallback-hs256-key" });
        }

        return chaves;
    }

    public SigningCredentials ObterCredenciaisAssinaturaDev()
    {
        // Emite token RS256 (padrao oficial do contrato da integracao com Java)
        return new SigningCredentials(new RsaSecurityKey(RsaDev), SecurityAlgorithms.RsaSha256);
    }
}
