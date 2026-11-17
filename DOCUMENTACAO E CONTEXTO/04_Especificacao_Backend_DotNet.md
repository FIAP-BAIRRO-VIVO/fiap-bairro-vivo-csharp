# Bairro Vivo | Especificação técnica do backend .NET

**Referência:** GS 2026/2 - turma 2TDSPO - Oracle SQL v1.2 validado.  
**Destinatário:** equipe responsável por Advanced Business Development with .NET.  
**Situação:** especificação de implementação; as rotas são **sugeridas**, não endpoints que já estejam funcionando.

## 1. Objetivo e limites de responsabilidade

O Bairro Vivo permite consultar um mapa público de problemas urbanos, criar ocorrências, anexar fotos, registrar atualizações comunitárias, votar sobre resolução/persistência e acompanhar o histórico. A API .NET é a proprietária dos dados operacionais das ocorrências, categorias, mídia, votos, histórico, consultas geográficas e relatórios.

**Java:** cria usuários, autentica, emite JWT, controla preferências/favoritos e implementa **toda a IA** com Spring AI/RAG. A API Java consultará uma ocorrência pela API .NET antes de produzir orientação. **Não implementar IA, geração de texto nem RAG em .NET.**

**Oracle:** fonte relacional do sistema. O script `Banco/BairroVivo_SQL_Oracle_v1_2.sql` já foi executado e validado pelo grupo no SQL Developer. **Não rodar esse script de reset em ambientes com dados que precisam ser preservados**, porque ele apaga e recria as tabelas. Para integração, usar o banco/schema provisionado e estável, com credenciais em variáveis de ambiente.

**MongoDB:** exercício acadêmico de representação documental do domínio de Banco de Dados; **não é a persistência transacional da API .NET**. Não exigir sincronização Oracle/MongoDB para o MVP.

## 2. Funcionalidades sob responsabilidade .NET

| Domínio | Ações | Regras fundamentais |
|---|---|---|
| Consultas públicas | categorias, municípios, listagem/mapa, detalhe, imagens, atualizações, histórico | `GET` públicos sem JWT; não vazar identidade por meio do anonimato |
| Ocorrências | criar, listar as próprias, atualizar e excluir logicamente conforme política do produto | somente `CIDADAO`; autor extraído do token, nunca do corpo |
| Mapa | consultar área visível, filtros, opcionalmente raio em km | coordenadas persistidas na ocorrência; filtro por bairro não existe |
| Atualizações | `COMENTARIO`, `PERSISTE`, `RESOLVIDO` com texto e/ou fotos | registrar por `PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO` |
| Participação | impedir contagem duplicada, mudar status, atualizar ciclo | apenas via procedure Oracle, com auditoria por triggers |
| Mídia | upload para storage de imagens e gravação de metadados no Oracle | validar tipo/tamanho e autoria; arquivo binário fora do Oracle |
| Relatórios | resumo por status/categoria/município/período e indicadores | `ADMIN` apenas lê; não altera ocorrências/status |
| Integração Java | disponibilizar dados canônicos da ocorrência para geração de orientação | endpoint de leitura sem dados privados; autenticação de serviço a decidir |

**Status possíveis:** `ABERTA` e `RESOLVIDA`. Não implementar gestor de status, estado `EM_ANALISE` ou regras de triagem automática com IA.

## 3. Autenticação, papéis e anonimato

- Java é o **único emissor de JWT**. Configuração proposta para acordo técnico: `RS256`, `iss=bairro-vivo-java`, `aud=bairro-vivo-apis`, `sub` numérico decimal em **string**, `roles=["CIDADAO"]` ou `["ADMIN"]`, `exp` e `iat`.
- .NET apenas **valida** algoritmo/assinatura com a chave pública do Java, emissor, audiência e expiração. Pode usar chave pública configurada ou JWKS, conforme definição conjunta. Nunca armazenar chave privada Java no .NET.
- `TB_USUARIO.ID_USUARIO` é `NUMBER(19)`. Converter o `sub` validado para `long`/`Int64`, rejeitando `sub` inválido; não inventar UUID.
- Cadastro/login/senha/perfil/favoritos/IA não são endpoints .NET.
- `CIDADAO` pode publicar e atualizar. `ADMIN` no MVP só consulta relatórios; não pode escrever ocorrências, atualizar status ou votar.
- Visitantes acessam **somente leituras públicas**. Não existe usuário visitante autenticado.
- Publicação anônima é só **anonimato público**: `FL_ANONIMO='S'` com `ID_USUARIO` real preservado. `Minhas ocorrências` sempre busca pelo `sub` do JWT. As respostas públicas devem usar `VW_OCORRENCIAS_PUBLICAS` e `VW_ATUALIZACOES_PUBLICAS` (ou uma projeção com a mesma proteção), sem `ID_USUARIO` nem email.
- JWT validado não comprova, por si, que a conta continua ativa neste instante: se houver exigência de bloqueio imediato de contas desativadas, combinar consulta de estado com Java. Uma expiração curta reduz essa janela.

## 4. Requisitos obrigatórios da disciplina .NET

Conforme a seção Advanced Business Development with .NET do enunciado da GS 2026/2:

| Critério | Peso na rubrica técnica | Implementação esperada |
|---|---:|---|
| ASP.NET Core **Minimal APIs** + validação, HTTP correto, Swagger/OpenAPI | 20 pts | endpoints reais documentados e utilizáveis |
| Autenticação e autorização JWT | 20 pts | validar token **emitido pelo Java**, proteger rotas e papéis |
| Idempotência em requisições repetidas | 15 pts | não duplicar operação ou efeito colateral |
| Rate Limiting e resposta HTTP 429 | 15 pts | política ASP.NET Core por rota/identidade/IP; verificar em testes |
| Testes unitários e de integração **xUnit** | 20 pts | negócios e endpoints |
| HealthChecks | 10 pts | API, Oracle e storage/serviços externos efetivamente usados |

**Entregas acadêmicas:** conferir no enunciado o repositório GitHub com README, ZIP com link e identificação do grupo, além da apresentação online. Não expor segredos no repositório.

## 5. Modelo Oracle utilizado pelo .NET

| Objeto físico | Uso no .NET |
|---|---|
| `TB_CATEGORIA`, `TB_MUNICIPIO` | filtros e referências; município obrigatório ao salvar ocorrência |
| `TB_OCORRENCIA` | título, `CLOB` descrição, coordenadas, autor, `FL_ANONIMO`, status, ciclo, exclusão lógica e datas |
| `TB_ATUALIZACAO` | comentário/relato; tipos `COMENTARIO`, `PERSISTE`, `RESOLVIDO` |
| `TB_MIDIA` | referências a fotos, com provedor/chave/storage e eventual `ID_ATUALIZACAO` |
| `TB_VOTO_SITUACAO` | votos do ciclo corrente; evitar manipulação direta pela API |
| `TB_HISTORICO_STATUS` | trilha de abertura/transição por triggers |
| `TB_REQUISICAO_IDEMPOTENTE` | chave/idempotência do usuário por rota |
| `VW_OCORRENCIAS_PUBLICAS`, `VW_ATUALIZACOES_PUBLICAS` | exposição pública com autor mascarado |
| `VW_RELATORIO_OCORRENCIAS` | contagem agregada base para ADMIN |
| `PKG_COMUNIDADE` | regras de participação comunitária |

Outras tabelas, inclusive `TB_USUARIO` e domínio de IA, são **pertencentes ao Java** ou referências compartilhadas: não fazer CRUD de usuário via .NET. Consultar o arquivo `Dicionario_Dados_Integracao.md` para todas as colunas/tipos e constraints.

### Município e geolocalização

- `TB_OCORRENCIA.ID_MUNICIPIO NUMBER(19) NOT NULL` é obrigatório. **GPS não fornece automaticamente um ID de município Oracle.** O backend/frontend devem acordar um fluxo: usuário escolhe município numa lista validada, ou a geocodificação localiza município e API resolve o ID; se não houver município confiável, não persistir ocorrência com valor arbitrário.
- Latitude `NUMBER(10,7)` em `[-90,90]`; longitude `NUMBER(10,7)` em `[-180,180]`.
- `ENDERECO_REFERENCIA` é opcional. Não há `TB_BAIRRO`.
- Mapa: GET da área visível via `minLat,maxLat,minLon,maxLon` com paginação/limites e filtros; raio em km como melhoria adicional. Sem API de Google Maps obrigatória. O frontend renderiza OpenStreetMap/MapLibre; .NET fornece coordenadas.
- Evitar retornar todos os registros nacionais de uma vez. Limitar área/volume e normalizar bounding boxes, incluindo o caso de cruzar meridiano 180 se um dia necessário.

## 6. Votação comunitária — implementação canônica

O endpoint de atualização deve chamar **`PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO`**, assinatura real:

```sql
PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO(
    P_ID_OCORRENCIA  IN NUMBER,
    P_ID_USUARIO     IN NUMBER,
    P_TIPO           IN VARCHAR2,
    P_TEXTO          IN CLOB,
    P_FL_ANONIMO     IN CHAR,
    P_ID_ATUALIZACAO OUT NUMBER
);
```

- A procedure valida cidadão ativo, bloqueia a ocorrência com `FOR UPDATE`, cria a atualização e, quando o voto é válido para o status atual, registra em `TB_VOTO_SITUACAO`.
- Em `ABERTA`, `RESOLVIDO` conta para fechamento. Em `RESOLVIDA`, `PERSISTE` conta para reabertura. `COMENTARIO` não conta. Tipos contrários ao estágio atual podem registrar informação, mas não votam.
- Dois usuários diferentes no mesmo ciclo provocam transição e avanço de `NUM_CICLO`. Os índices/constraints impedem voto duplicado.
- `TRG_OCORRENCIA_BIU` bloqueia alterações diretas de `STATUS`/`NUM_CICLO`, preserva `ID_USUARIO` e protege a autoria da transição. `TRG_OCORRENCIA_AI` e `TRG_AUDITORIA_STATUS` registram o histórico.
- **Não duplicar contagem ou atualização de status no C#.** Após executar a procedure, consultar o estado atual da ocorrência em Oracle e devolvê-lo ao app.
- O .NET controla a transação da conexão que executa a procedure. Sem `COMMIT` dentro da procedure, sucesso = commit do fluxo; falha = rollback. Tratar exceções Oracle por categoria, sem mostrar códigos internos ao usuário final.
- O autor da ocorrência pode votar, mas não consegue gerar sozinho os dois votos necessários. Uma pessoa autenticada não equivale necessariamente a uma pessoa física distinta; esta é a proteção prevista para MVP.

### Fotos e vínculo com atualizações

`TB_MIDIA` não guarda imagem binária. Quando `ID_ATUALIZACAO` for `NULL`, a foto pertence diretamente à ocorrência e `TRG_MIDIA_BIU` exige que o usuário seja o **autor original da ocorrência**. Quando `ID_ATUALIZACAO` for informado, a FK composta exige correspondência entre atualização, ocorrência e usuário.

Fluxo recomendado: validar tipo `image/jpeg`, `image/png` ou `image/webp`, limites e conteúdo; receber via upload autenticado; gravar no storage; armazenar `PROVEDOR`, `CHAVE_STORAGE`, `URL_EXIBICAO` (opcional), MIME e tamanho. Para foto de atualização, primeiro criar atualização via procedure e depois vincular a imagem, em operação consistente, limpando uploads órfãos se algum passo falhar. Não enviar segredo do storage ao app nem confiar em URL arbitrária fornecida pelo cliente.

## 7. Endpoints REST sugeridos (.NET)

**Prefixo ilustrativo `/api/v1`**. Os contratos são propostas e devem ser publicados via OpenAPI; não devem ser tratados como implementados.

| Método | Rota sugerida | Acesso | Função |
|---|---|---|---|
| GET | `/categorias` | Público | categorias ativas |
| GET | `/municipios?nome=&uf=` | Público | municípios cadastrados/seleção |
| GET | `/ocorrencias?minLat=&maxLat=&minLon=&maxLon=&categoriaId=&status=&page=&pageSize=` | Público | mapa/lista com filtros e paginação |
| GET | `/ocorrencias/{id}` | Público | detalhe com autor público, mídia, status e votos |
| GET | `/ocorrencias/{id}/atualizacoes` | Público | histórico paginado e anonimizado |
| GET | `/ocorrencias/{id}/historico` | Público | alterações de status e datas; sem IDs privados |
| GET | `/ocorrencias/minhas` | CIDADAO | ocorrências do sujeito JWT, inclusive anônimas |
| POST | `/ocorrencias` | CIDADAO | criar ocorrência `ABERTA` |
| PUT | `/ocorrencias/{id}` | Autor CIDADAO | editar campos permitidos, nunca status/autor/ciclo |
| DELETE | `/ocorrencias/{id}` | Autor CIDADAO | exclusão lógica com política de histórico |
| POST | `/ocorrencias/{id}/atualizacoes` | CIDADAO | comentário/persistência/resolução via procedure |
| POST | `/ocorrencias/{id}/midias` | Autor CIDADAO | foto direta da ocorrência |
| POST | `/ocorrencias/{id}/atualizacoes/{atualizacaoId}/midias` | Autor da atualização | foto da atualização |
| GET | `/admin/relatorios/ocorrencias` | ADMIN | filtros e indicadores agregados |
| GET | `/health` | Público operacional | health checks, detalhamento interno protegido |

**Atenção à precedência das rotas:** registrar `GET /ocorrencias/minhas` antes de rotas que aceitam `{id}` genérico ou impor constraint `{id:long}`.

### Contratos de entrada/saída propostos

**POST /ocorrencias** (`CIDADAO`, `Idempotency-Key` para requisições de criação):

```json
{
  "titulo": "Arvore caida em via publica",
  "descricao": "Galhos impedem a passagem de pedestres",
  "categoriaId": "3",
  "municipioId": "1",
  "latitude": -23.5614140,
  "longitude": -46.6558810,
  "enderecoReferencia": "Proximo ao cruzamento principal",
  "anonimoPublico": true
}
```

Resposta `201 Created` + `Location`, ID **Oracle `NUMBER(19)` serializado como string** (evita perda de precisão em JavaScript), `status: "ABERTA"`, `numeroCiclo: 1`, datas ISO 8601. Não aceitar `usuarioId`, `status` ou `numeroCiclo` enviados pelo frontend.

**POST /ocorrencias/{id}/atualizacoes** (`CIDADAO`):

```json
{
  "tipo": "RESOLVIDO",
  "texto": "A arvore foi removida",
  "anonimoPublico": true
}
```

Resposta sugerida `201 Created` com `atualizacaoId`, `contabilizadaComoVoto`, `statusAtual`, `numeroCiclo` e `confirmacoesNoCiclo`. O valor `contabilizadaComoVoto` deve vir dos dados do Oracle (verificar voto inserido), não de suposição baseada apenas no tipo informado.

**GET /ocorrencias/{id}**: retornar apenas campos públicos, categoria/município, mídias e autor mascarado. Não retornar `ID_USUARIO` real, `ID_AUTOR_TRANSICAO`, email ou dados de outros participantes. Dados privados (ex.: `souAutor`, `podeEditar`) devem ser fornecidos em contexto autenticado, com controle de acesso.

**GET /admin/relatorios/ocorrencias**: contagens, intervalos, categorias/municípios. Sem nomes/emails de autores. O admin não pode alterar status.

### HTTP e falhas

| Situação | Resposta HTTP recomendada |
|---|---|
| validação de dados, coordenadas ou município | `400` com campos inválidos |
| ausência/token inválido | `401` |
| papel inadequado ou operação de outro autor | `403` (ou `404` para ocultar existência de recurso privado) |
| ocorrência inexistente/excluída | `404` |
| voto duplicado, conflito da política de edição ou idempotência divergente | `409` |
| solicitação repetida com mesmo `Idempotency-Key` e corpo idêntico | retornar recurso/resultado anterior, sem duplicar |
| limite de requisições | `429` com `Retry-After` quando aplicável |
| indisponibilidade real do Oracle/storage | `503` ou `500` conforme natureza da falha |

Formato sugerido comum com Java: `{ "codigo": "...", "mensagem": "...", "detalhes": [], "correlationId": "..." }`.

## 8. Idempotência real e concorrência

- `TB_REQUISICAO_IDEMPOTENTE`: `ID_USUARIO`, `CHAVE`, `ROTA`, `METODO_HTTP`, `HASH_CORPO` (64), `ID_RECURSO`, `DT_CRIACAO`; `UNIQUE(ID_USUARIO, CHAVE, ROTA)`.
- Solicitações de criação e ações com efeitos colaterais devem aceitar `Idempotency-Key` gerado pelo cliente e estável em retries. O .NET precisa comparar método e hash do corpo e devolver resultado anterior no replay. **A tabela não armazena resposta HTTP completa**, então o serviço deve reconstruir a resposta pelo recurso persistido ou ampliar a estratégia de acordo com a equipe (sem alterar o SQL neste pacote).
- A criação do recurso e registro da chave devem ocorrer na **mesma transação**. Em concorrência, tratar violação da constraint unique e consultar a operação vencedora. Se uma transação ainda estiver pendente, evitar retornar sucesso fictício.
- Rate limiting e `UNIQUE` de voto por usuário/ciclo também são proteções, mas **não substituem idempotência de requisição**.
- Testar pelo menos requisição idêntica duplicada, mesmo key com body distinto, duas requisições concorrentes, retry após timeout e falha com rollback.

## 9. Segurança, validação e operações

- Validar tipos e limites (título 150, descrição CLOB com limite lógico, categoria ativa, município existente, coordenadas, status filtro, paginação). Parametrizar SQL via EF Core ou Dapper.
- CORS apenas para origens necessárias; HTTPS; logs sem tokens, senhas ou identidades de publicações anônimas.
- Limitar uploads por tipo, bytes, contagem, autenticação e varreduras básicas; não confiar em extensão. Retirar metadados sensíveis da imagem quando possível.
- `AddRateLimiter` / `UseRateLimiter` com políticas explícitas e respostas `429`; política por IP para públicos e por conta para autenticados.
- `AddHealthChecks` para processo/Oracle e storage se utilizado; `/health` não deve expor segredos, strings de conexão ou stack traces.
- Swagger/OpenAPI deve explicar autenticação Bearer, exemplos de respostas, erros `400/401/403/404/409/429/500`, paginação, filtros e upload.
- Testes xUnit: validação JWT Java (incluindo issuer, aud, assinatura e expiração), permissões, anonimato nas views, voto duplicado, fechamento/reabertura após dois votos, concorrência/idempotência, rate limit e health.
- Usar config externa (ex.: `appsettings.Development.json` apenas sem segredos) e README com como rodar, ambiente Oracle, Swagger, testes e health. Não armazenar credenciais reais em commits.

## 10. Contratos a fechar com Java e frontend

1. Assinatura JWT (RS256), disponibilidade de JWKS/chave pública, `iss`, `aud`, `roles`, `sub`, clock skew e expiração.
2. Autenticação de serviço **Java → .NET** para busca canônica de ocorrência; GET público pode bastar ao MVP, com proteção adicional se dados não públicos forem necessários.
3. Modelo dos IDs (`NUMBER(19)` no Oracle e string no JSON), paginação, filtros, envelopes e erros.
4. Escolha de storage, limite de fotos, formato de upload e política de exclusão/edição.
5. Fluxo de seleção/identificação de `ID_MUNICIPIO` obrigatório e lista de categorias.
6. Política de disponibilidade de orientação pública via Java, sem expor dados privados no .NET.
7. Rate limiting e HealthChecks de dependências externas efetivamente utilizadas.

**Fonte de verdade dos campos e nomes:** `Banco/BairroVivo_SQL_Oracle_v1_2.sql`. Em caso de divergência, ajustar contratos/documentação em conjunto antes de implementar; não editar silenciosamente o banco validado.
