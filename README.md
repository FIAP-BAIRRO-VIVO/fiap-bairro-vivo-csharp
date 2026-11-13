# Bairro Vivo API (.NET 8)

API RESTful em .NET 8 do ecossistema **Bairro Vivo (CitizenTech — Global Solution 2026/2)**, responsável por gerenciar ocorrências de zeladoria urbana, mídias, relatórios executivos de bairro, categorias, municípios e votação/validação comunitária (*"Ainda está lá!"* e *"Já foi resolvido!"*).

## Visão Geral do Ecossistema

O **Bairro Vivo** é composto por aplicações integradas que compartilham o banco **Oracle** e o mesmo contrato de autenticação **JWT**:

1. **Auth Service / Core Backend (Java Spring Boot):** realiza cadastro, login e emissão do token JWT (`HS256`) com claims de identidade (`sub`/`userId`) e perfil (`CIDADAO` ou `ADMIN`).
2. **Bairro Vivo API (.NET 8 — este repositório):** consome o JWT emitido pelo serviço Java, valida vínculo e status ativo do usuário no Oracle (`TB_USUARIO`), aplica regras de autorização por perfil e propriedade da ocorrência, paginação com `Offset/Limit`, links **HATEOAS**, **Idempotência** (`Idempotency-Key` / `TB_REQUISICAO_IDEMPOTENTE`), **Rate Limiting** (`HTTP 429`), **Observabilidade** (`Serilog` + `HealthChecks`) e integração com pacotes PL/SQL (`PKG_COMUNIDADE`).
3. **Serviço de IA & IoT (Python FastAPI):** classificação automática de fotos/descrições e geração de relatórios executivos por bairro.
4. **Aplicativo Mobile (React Native) & Web:** interfaces para cidadãos e gestores públicos.

```text
Mobile / Web -> Java Auth Service (Login -> JWT)
Mobile / Web -> .NET 8 Bairro Vivo API (Authorization: Bearer <JWT>)
                    ├── Oracle (EF Core + PL/SQL PKG_COMUNIDADE)
                    ├── Idempotência (TB_REQUISICAO_IDEMPOTENTE)
                    ├── Rate Limiting (FixedWindow 429)
                    └── ViaCEP HealthCheck (Geolocalização/CEP)
```

---

## Arquitetura do Projeto

O repositório segue **Clean Architecture** organizada em pastas dentro de `BairroVivo.API` e projeto de testes `BairroVivo.Tests`:

```text
BairroVivo.sln
├── BairroVivo.API/
│   ├── Presentation/
│   │   ├── Controllers/          # Endpoints HTTP, paginação e HATEOAS
│   │   ├── Filters/              # OcorrenciaAccessFilter (JWT/Oracle) e IdempotencyFilter
│   │   └── HateoasLinks.cs       # Geração centralizada de links HATEOAS
│   ├── Application/
│   │   ├── Dtos/                 # Contratos de entrada/saída, LinkDto e PageResponseDto
│   │   ├── Interfaces/           # Contratos de UseCases e serviços de acesso
│   │   ├── Mappers/              # Extension methods ToEntity, UpdateEntity e ToResponseDto
│   │   └── UseCases/             # Orquestração dos casos de uso da aplicação
│   ├── Domain/
│   │   ├── Entities/             # Entidades de domínio espelhadas no modelo Oracle v1.1
│   │   ├── Interfaces/           # Contratos de repositórios
│   │   └── Rules/                # Regras puras de domínio (OcorrenciaRules, AtualizacaoRules)
│   └── Infrastructure/
│       ├── Data/
│       │   ├── ApplicationContext.cs # Mapeamento Fluent API completo para o Oracle
│       │   └── Repositories/     # Implementações EF Core e PL/SQL (IntegracaoOracleRepository)
│       └── Health/               # ViaCepHealthCheck (IHealthCheck)
└── BairroVivo.Tests/
    ├── Domain/                   # Testes unitários das regras de domínio (OcorrenciaRulesTest)
    ├── App/                      # Testes unitários de Repositórios, UseCases e HealthChecks
    └── Integration/              # WebApplicationFactory, JWT, HATEOAS, Paginação, Idempotência e Erros
```

---

## Principais Funcionalidades Implementadas

### 1. Autenticação e Autorização com JWT (`OcorrenciaAccessFilter`)
- Validação de assinatura `HS256`, `Issuer` (`bairro-vivo-java`), `Audience` (`bairro-vivo-apis`) e expiração.
- O filtro `OcorrenciaAccessFilter` consulta `TB_USUARIO` no Oracle para garantir que o usuário está ativo (`ATIVO = 'S'`).
- Regras de acesso:
  - **Leitura pública (`[AllowAnonymous]`):** listagem e consulta de ocorrências, atualizações, categorias, municípios, relatórios de bairro e health checks.
  - **Cidadão autenticado (`CIDADAO`):** pode abrir ocorrências (`POST /api/ocorrencias`), consultar suas próprias ocorrências (`GET /api/ocorrencias/me`), anexar mídias, comentar e votar em ocorrências da comunidade (`POST /api/ocorrencias/{id}/atualizacoes`), e editar/excluir apenas ocorrências de sua autoria.
  - **Administrador (`ADMIN`):** pode atualizar status, editar ou remover qualquer ocorrência e gerenciar categorias.

### 2. Paginação + HATEOAS (`PageResponseDto<T>` e `HateoasLinks`)
- Paginação padronizada com cálculo de `Deslocamento = (pagina - 1) * tamanho` e retorno JSON com `pagina`, `tamanho`, `total`, `dados` e `links` (`self`, `first`, `last`, `prev`, `next`).
- Cada ocorrência e atualização comunitária retorna seus próprios links de navegação e ações disponíveis (`self`, `update`, `status`, `delete`, `atualizacoes`, `confirmar_ou_votar`, `adicionar_midia`, `votos_oracle`).

### 3. Votação Comunitária e Integração PL/SQL Oracle (`PKG_COMUNIDADE`)
- Suporte a votos comunitários `CONFIRMACAO` (*"Ainda está lá!"*) e `JA_RESOLVIDO` (*"Já foi resolvido!"*).
- Impede voto duplicado do mesmo cidadão na mesma modalidade para a mesma ocorrência (`HTTP 409 Conflict`).
- Ao atingir **3 confirmações**, a ocorrência recebe automaticamente `VALIDADA_COMUNIDADE = 'S'` e tem sua urgência elevada.
- Endpoints dedicados em `IntegracaoOracleController` para invocar diretamente no Oracle:
  - `POST /api/ocorrencias/{ocorrenciaId}/integracao-oracle/registrar-atualizacao` -> `PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO`
  - `GET /api/ocorrencias/{ocorrenciaId}/integracao-oracle/total-votos-ativos` -> `PKG_COMUNIDADE.FN_TOTAL_VOTOS_ATIVOS`

### 4. Idempotência (`IdempotencyFilter`) e Rate Limiting (`HTTP 429`)
- Requisições `POST`, `PUT` ou `PATCH` que enviam o cabeçalho `Idempotency-Key` têm a resposta persistida em `TB_REQUISICAO_IDEMPOTENTE` por 24 horas. Reenvios com a mesma chave retornam a resposta original com o cabeçalho `X-Idempotent-Replay: true` sem duplicar registros no banco.
- `RateLimiter` configurado por IP com janela fixa retornando `HTTP 429 Too Many Requests` em caso de abuso.

### 5. Observabilidade, Logs Estruturados e HealthChecks
- **Serilog** configurado com enriquecimento de ambiente, thread e correlação (`[TraceId:{TraceId}] [SpanId:{SpanId}]`).
- **HealthChecks** disponíveis em:
  - `/health/live` e `/api/health/live`
  - `/health/db` e `/api/health/db` (Oracle)
  - `/health/external` (ViaCEP)
  - `/health/ready` e `/api/health/ready`

---

## Como Executar e Testar

### Executar a API
```bash
dotnet run --project BairroVivo.API
```
O Swagger estará disponível na raiz (`http://localhost:5222/`).

### Executar os Testes Automatizados (xUnit + Moq + WebApplicationFactory)
```bash
dotnet test BairroVivo.sln
```
