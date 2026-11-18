# FIAP — Global Solution 2026/2

## Bairro Vivo API (.NET 8)
**Disciplina:** Advanced Business Development with .NET  
**Turma:** 2TDSPO (2º Semestre)  
**Tema:** CitizenTech — Zeladoria Urbana Participativa e Consenso Comunitário  

---

## 📌 Links Oficiais do Projeto

* **Repositório GitHub:** [https://github.com/FIAP-BAIRRO-VIVO/fiap-bairro-vivo-csharp](https://github.com/FIAP-BAIRRO-VIVO/fiap-bairro-vivo-csharp)
* **Ambiente de Produção (OnRender):** [https://fiap-bairro-vivo-csharp.onrender.com](https://fiap-bairro-vivo-csharp.onrender.com)
* **Documentação Interativa Swagger / OpenAPI:** [https://fiap-bairro-vivo-csharp.onrender.com/swagger](https://fiap-bairro-vivo-csharp.onrender.com/swagger)
* **Health Checks Oficiais:**
  * Liveness (`/health/live`): [https://fiap-bairro-vivo-csharp.onrender.com/health/live](https://fiap-bairro-vivo-csharp.onrender.com/health/live)
  * Banco Oracle (`/health/db`): [https://fiap-bairro-vivo-csharp.onrender.com/health/db](https://fiap-bairro-vivo-csharp.onrender.com/health/db)
  * Serviço Externo ViaCEP (`/health/external`): [https://fiap-bairro-vivo-csharp.onrender.com/health/external](https://fiap-bairro-vivo-csharp.onrender.com/health/external)
  * Readiness Geral (`/health/ready`): [https://fiap-bairro-vivo-csharp.onrender.com/health/ready](https://fiap-bairro-vivo-csharp.onrender.com/health/ready)

---

## 🏛️ Visão Geral e Arquitetura do Ecossistema

O **Bairro Vivo** é uma plataforma colaborativa que empodera cidadãos no monitoramento e resolução de problemas urbanos (iluminação pública, buracos, descarte irregular de lixo, vazamentos e árvores caídas). O ecossistema é formado por microserviços especializados que compartilham a mesma base de dados transacional **Oracle Database 21c** e o mesmo contrato de segurança:

1. **Auth Service (Java Spring Boot):** Gestão cadastral de munícipes e emissão de tokens JWT assimétricos (**RS256**).
2. **Bairro Vivo API (.NET 8 — este repositório):** Core operacional da plataforma. Responsável pelo ciclo de vida das ocorrências, envio de mídias, histórico de status, consenso comunitário de votação, relatórios executivos por município e integração com procedimentos PL/SQL.
3. **Frontend (React Native / Web):** Interface responsiva hospedada na Vercel e consumindo os endpoints RESTful.

```
       [ Dispositivos Móveis / Web Frontend ]
                         │
        ┌────────────────┴────────────────┐
        ▼                                 ▼
[ Java Auth Service ]            [ .NET 8 Bairro Vivo API ]
(Cadastro / Login JWT)           (Ocorrências / Votação / HATEOAS)
        │                                 │
        │      Chave Pública RSA (RS256)  │
        └────────────────────────────────►│
                                          ▼
                         [ Oracle Database 21c ]
                         ├── Schema v1.2 (19 Tabelas)
                         ├── Package PL/SQL (PKG_COMUNIDADE)
                         ├── Idempotência (TB_REQUISICAO_IDEMPOTENTE)
                         └── Views Executivas (VW_RELATORIO_OCORRENCIAS)
```

---

## 🏗️ Estrutura do Projeto (Clean Architecture)

A solução foi estruturada rigorosamente nos princípios de **Clean Architecture**, desacoplando regras de domínio de infraestrutura externa e frameworks:

```text
BairroVivo.sln
├── BairroVivo.API/
│   ├── Domain/
│   │   ├── Entities/             # Entidades mapeadas para o Oracle Schema v1.2
│   │   ├── Interfaces/           # Contratos abstratos dos repositórios
│   │   └── Rules/                # Regras puras de negócio (OcorrenciaRules, AtualizacaoRules)
│   ├── Application/
│   │   ├── Dtos/                 # Contratos de entrada/saída, paginação e LinkDto
│   │   ├── Interfaces/           # Contratos dos Use Cases da aplicação
│   │   ├── Mappers/              # Conversores entre Entidades e DTOs
│   │   └── UseCases/             # Orquestração e fluxos de aplicação
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationContext.cs # Mapeamento Fluent API completo para Oracle
│   │   │   └── Repositories/     # Repositórios EF Core e execução PL/SQL (Oracle/InMemory)
│   │   ├── Health/               # Verificadores de saúde customizados (ViaCepHealthCheck)
│   │   └── Security/             # Gestão de chaves RSA/RS256 e validação de JWT
│   ├── Presentation/
│   │   ├── Controllers/          # Controllers RESTful com anotações OpenAPI/Swagger
│   │   ├── Filters/              # Filtros de Acesso (OcorrenciaAccessFilter) e Idempotência
│   │   └── HateoasLinks.cs       # Geração centralizada de links de hipermídia RFC 8288
│   ├── Program.cs                # Configuração do pipeline HTTP, DI, Rate Limiting e CORS
│   └── appsettings.json          # Configurações de conexões, JWT e limites operacionais
└── BairroVivo.Tests/
    ├── Domain/                   # Testes unitários de regras de domínio
    ├── App/                      # Testes unitários de Use Cases e Repositórios com Moq
    └── Integration/              # Testes de integração de ponta a ponta com WebApplicationFactory
```

---

## 🚀 Requisitos Técnicos Implementados

### 1. Autenticação e Autorização Assimétrica (JWT RS256)
- **Integração com Java:** Validação de tokens emitidos com algoritmo assimétrico **RS256**, utilizando chave pública RSA (carregada via `Jwt:PublicKey` ou arquivo PEM).
- **Claims Padronizadas:** Mapeamento de identidade via claim `sub` (ID do munícipe) e autorização por perfil via claim `roles` (`CIDADAO` ou `ADMIN`).
- **Validação no Oracle:** O filtro `OcorrenciaAccessFilter` verifica o vínculo do usuário em `TB_USUARIO` e bloqueia contas inativas (`ATIVO = 'N'`).
- **Retrocompatibilidade:** Mantido suporte seguro a tokens `HS256` para homologação e execução dos testes automatizados.

### 2. Hipermídia e Navegabilidade (HATEOAS)
- Todos os recursos e coleções retornam links de hipermídia no padrão **RFC 8288**, orientando o cliente sobre as ações disponíveis:
  - Recursos individuais: `self`, `update`, `delete`, `atualizacoes`, `atualizar_ou_votar`, `historico`, `adicionar_midia`.
  - Coleções paginadas: `self`, `first`, `prev`, `next`, `last`.

### 3. Paginação Padronizada
- Implementada via `PageResponseDto<T>` com os parâmetros de consulta `pagina` e `tamanho` (Offset/Limit).
- O retorno JSON padronizado inclui `pagina`, `tamanho`, `total`, a coleção de `dados` e os links de navegação.

### 4. Idempotência de Requisições (`IdempotencyFilter`)
- Proteção contra requisições duplicadas em métodos de escrita (`POST` e `PUT`) através do cabeçalho HTTP `Idempotency-Key`.
- Persistência das chaves e do hash SHA-256 do payload em `TB_REQUISICAO_IDEMPOTENTE`.
- Em caso de reenvio com o mesmo corpo, a API responde imediatamente com o recurso original e o cabeçalho `X-Idempotent-Replay: true`. Caso o payload divirja para a mesma chave, retorna `HTTP 409 Conflict`.

### 5. Rate Limiting (Controle de Vazão)
- Middleware nativo `Microsoft.AspNetCore.RateLimiting` configurado com política de janela fixa (`FixedWindowLimiter`) vinculada ao endereço IP do cliente.
- Caso o limite seja ultrapassado, retorna `HTTP 429 Too Many Requests` com corpo padronizado.

### 6. Consenso Comunitário e Procedimentos PL/SQL Oracle
- A transição do status de ocorrências (`ABERTA` <-> `RESOLVIDA`) é orientada por consenso da vizinhança: são necessários **2 votos válidos de cidadãos distintos** no mesmo ciclo.
- Registro de votos e comentários através do pacote PL/SQL `PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO` e consulta de votos ativos com `PKG_COMUNIDADE.FN_TOTAL_VOTOS_ATIVOS`.
- Histórico completo de auditoria persistido em `TB_HISTORICO_STATUS`.

### 7. Observabilidade e Logs Estruturados
- **Serilog:** Logs estruturados enriquecidos com `TraceId`, `SpanId`, `Environment` e carimbo de data/hora em formato JSON/Console.
- **Health Checks:** Monitoramento detalhado das dependências (`self`, `oracle` e `viacep`) exposto em endpoints padronizados.

### 8. Política de CORS Dinâmica
- Liberação configurada para aplicações Web hospedadas na **Vercel** (`*.vercel.app`), serviços no **Render** (`*.onrender.com`) e ambientes locais de desenvolvimento (`localhost`).

---

## 📋 Catálogo Completo de Endpoints

### 🔐 Autenticação e Testes
| Método | Endpoint | Acesso | Descrição |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/token` | Público | Emite token JWT de teste (RS256) compatível com o contrato Java. |

### 📍 Ocorrências
| Método | Endpoint | Acesso | Descrição |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/ocorrencias` | Público | Lista ocorrências públicas com paginação, filtros e HATEOAS. |
| `GET` | `/api/ocorrencias/{id}` | Público | Consulta os detalhes de uma ocorrência por ID. |
| `GET` | `/api/ocorrencias/{id}/historico` | Público | Consulta a linha do tempo e transições de status da ocorrência. |
| `POST` | `/api/ocorrencias` | Cidadão | Cadastra uma nova ocorrência vinculada ao usuário autenticado. |
| `PUT` | `/api/ocorrencias/{id}` | Autor / Admin | Atualiza os dados de uma ocorrência existente. |
| `DELETE` | `/api/ocorrencias/{id}` | Autor / Admin | Realiza a exclusão lógica da ocorrência (`FL_EXCLUIDA = 'S'`). |
| `POST` | `/api/ocorrencias/{id}/midias` | Autor | Anexa metadados de fotos diretamente à ocorrência (`TB_MIDIA`). |

### 👤 Minhas Ocorrências
| Método | Endpoint | Acesso | Descrição |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/ocorrencias/minhas` | Cidadão | Lista as ocorrências abertas pelo cidadão autenticado via JWT. |

### 💬 Atualizações e Votação Comunitária
| Método | Endpoint | Acesso | Descrição |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/ocorrencias/{id}/atualizacoes` | Público | Lista atualizações, comentários e votos de uma ocorrência. |
| `GET` | `/api/atualizacoes/{id}` | Público | Obtém detalhes de uma atualização específica. |
| `POST` | `/api/ocorrencias/{id}/atualizacoes` | Cidadão | Registra comentário ou voto (`PERSISTE` ou `RESOLVIDO`). |
| `POST` | `/api/atualizacoes/{id}/midias` | Autor Atualização | Anexa foto comprobatória à atualização comunitária. |

### 🏷️ Categorias e Municípios
| Método | Endpoint | Acesso | Descrição |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/categorias` | Público | Lista as categorias ativas de zeladoria urbana. |
| `GET` | `/api/categorias/{id}` | Público | Consulta categoria por ID. |
| `POST` | `/api/categorias` | Admin | Cadastra nova categoria de ocorrência. |
| `PUT` | `/api/categorias/{id}` | Admin | Atualiza categoria existente. |
| `DELETE` | `/api/categorias/{id}` | Admin | Remove ou desativa uma categoria. |
| `GET` | `/api/municipios` | Público | Lista municípios atendidos pela plataforma (código IBGE e UF). |
| `GET` | `/api/municipios/{id}` | Público | Consulta município por ID. |

### 📊 Relatórios e Indicadores
| Método | Endpoint | Acesso | Descrição |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/admin/relatorios/ocorrencias` | Admin | Retorna dados agregados baseados na `VW_RELATORIO_OCORRENCIAS`. |

### ⚙️ Integração Direta com Oracle PL/SQL
| Método | Endpoint | Acesso | Descrição |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/ocorrencias/{id}/integracao-oracle/registrar-atualizacao` | Cidadão | Invoca `PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO`. |
| `GET` | `/api/ocorrencias/{id}/integracao-oracle/total-votos-ativos` | Público | Invoca `PKG_COMUNIDADE.FN_TOTAL_VOTOS_ATIVOS`. |

### 💓 Verificação de Saúde (Health Checks)
| Método | Endpoint | Acesso | Descrição |
| :--- | :--- | :--- | :--- |
| `GET` | `/health/live` | Público | Liveness probe para orquestradores de nuvem. |
| `GET` | `/health/db` | Público | Verifica conectividade com o Oracle Database. |
| `GET` | `/health/external` | Público | Verifica disponibilidade da API externa ViaCEP. |
| `GET` | `/health/ready` | Público | Readiness probe consolidada de todos os serviços. |

---

## 🧪 Testes Automatizados

A solução conta com **60 testes automatizados** cobrindo testes unitários de regras de negócio, repositórios mockados e testes de integração com `WebApplicationFactory`:

```bash
dotnet test
```

### Resultados da Suíte de Testes:
```text
Passed!  - Failed: 0, Passed: 60, Skipped: 0, Total: 60, Duration: 2 s - BairroVivo.Tests.dll
```

### Principais Cenários Cobertos:
* **Domínio:** Validação de coordenadas geográficas, regras de transição de status e consenso de 2 votos.
* **Segurança:** Rejeição de requisições não autenticadas (`401`), bloqueio de edição por não-autores (`403`) e restrição de abertura de ocorrências por administradores (`403`).
* **JWT RS256 e Java:** Emissão e validação de tokens assimétricos RS256 contendo claims `sub` e `roles`.
* **Idempotência:** Validação de replay de requisições com a mesma chave e detecção de conflitos de payload (`409`).
* **HATEOAS e Paginação:** Verificação de integridade dos links de navegação e metadados de paginação.

---

## 💻 Instruções de Execução Local

### Pré-requisitos
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* Acesso ao banco Oracle (ou execução via InMemory para testes)

### 1. Clonar o Repositório
```bash
git clone https://github.com/FIAP-BAIRRO-VIVO/fiap-bairro-vivo-csharp.git
cd fiap-bairro-vivo-csharp
```

### 2. Restaurar Dependências e Compilar
```bash
dotnet restore
dotnet build
```

### 3. Executar os Testes
```bash
dotnet test
```

### 4. Executar a Aplicação
```bash
dotnet run --project BairroVivo.API
```
A API estará acessível em `http://localhost:5222` e o Swagger em `http://localhost:5222/swagger`.

---

## 📦 Deploy em Nuvem (Render)

A aplicação está configurada para deploy automatizado através do [Dockerfile](file:///home/antigravity/FIAP-BAIRRO-VIVO/fiap-bairro-vivo-csharp/Dockerfile) multi-stage na nuvem do Render:
- **Build Stage:** `mcr.microsoft.com/dotnet/sdk:8.0` para compilação e publicação.
- **Runtime Stage:** `mcr.microsoft.com/dotnet/aspnet:8.0` em imagem Linux otimizada.
- **Variáveis de Ambiente Configuradas no Render:**
  - `PORT`: Porta dinâmica gerenciada pela plataforma.
  - `ConnectionStrings__OracleConnection`: Conexão com o banco Oracle da FIAP.
  - `Jwt__Issuer`: `bairro-vivo-java`.
  - `Jwt__Audience`: `bairro-vivo-apis`.
  - `Jwt__PublicKey`: Chave pública RSA para validação dos tokens RS256.
