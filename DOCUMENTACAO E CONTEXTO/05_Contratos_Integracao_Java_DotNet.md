# Bairro Vivo | Contratos de integração Java ↔ .NET

**Versão de alinhamento:** Oracle v1.2 / GS 2026-2 / turma 2TDSPO.  
**Status:** contratos **propostos**, a serem fechados e publicados como OpenAPI quando as APIs estiverem implementadas. Não representam URLs ou serviços já disponíveis.

## 1. Quem é dono de cada recurso?

| Recurso | Java | .NET |
|---|---|---|
| Criar conta, senha, login, autenticação e geração de JWT | **Responsável** | Validar token Java |
| Preferências e locais favoritos | **Responsável** | Sem CRUD |
| IA e fontes oficiais (Spring AI/RAG) | **Responsável** | Disponibilizar dados canônicos da ocorrência |
| Categorias, municípios, ocorrências e mapa | Consumir dados necessários | **Responsável** |
| Comentários, votos por ciclo, fotos e histórico | Sem escrita | **Responsável** |
| Relatórios agregados | Sem escrita | **Responsável**, ADMIN somente leitura |
| Oracle | `TB_USUARIO`, `TB_PREFERENCIA_USUARIO`, `TB_LOCAL_FAVORITO`, domínio IA | Domínio da ocorrência e comunidade; `TB_MUNICIPIO` referência compartilhada |

## 2. Identidade compartilhada

Formato inicial recomendado do JWT emitido por Java:

```json
{
  "iss": "bairro-vivo-java",
  "aud": "bairro-vivo-apis",
  "sub": "123",
  "roles": ["CIDADAO"],
  "iat": 1794319200,
  "exp": 1794322800
}
```

- `sub` equivale a `TB_USUARIO.ID_USUARIO NUMBER(19)`, representado por dígitos decimais em string.
- Java assina com chave privada (sugestão RS256). .NET valida com chave pública, emissor, audiência, expiração e papel. A chave pública pode ser fornecida por configuração segura ou JWKS público, após acordo.
- Registro público cria apenas `CIDADAO`; contas `ADMIN` devem ser provisionadas de forma controlada.
- Java não envia senha/hash à API .NET; .NET não cria conta nem aceita `usuarioId` de clientes como prova de autoria.
- Chamadas públicas de leitura não exigem JWT. Escritas exigem `CIDADAO`; relatórios exigem `ADMIN`.
- Não expor `ID_USUARIO` de autor anônimo nas respostas públicas; o usuário reconhece sua autoria pelo endpoint autenticado `Minhas ocorrências`.

## 3. Consulta de ocorrência para a IA (Java → .NET)

**Objetivo:** Java recebe `{ "ocorrenciaId": "1" }` do frontend e obtém dados canônicos na API .NET, sem pedir ao celular que envie detalhes modificados.

Rota sugerida: `GET /api/v1/ocorrencias/{id}` com resposta pública sanitizada. Para IA que necessitar outras propriedades, criar rota interna somente de leitura com autenticação de serviço, em vez de expor dados privados no endpoint público.

Exemplo do contrato DTO de leitura:

```json
{
  "id": "1",
  "titulo": "Arvore caida perto da calcada",
  "descricao": "Uma arvore caiu e ocupa parte da passagem.",
  "status": "ABERTA",
  "categoria": { "id": "3", "nome": "Arvores e vegetacao" },
  "municipio": { "id": "1", "codigoIbge": "3550308", "nome": "Sao Paulo", "uf": "SP" },
  "latitude": -23.561414,
  "longitude": -46.655881,
  "enderecoReferencia": "Regiao da Avenida Paulista, Sao Paulo",
  "autorExibicao": "Cidadao anonimo"
}
```

Valores demonstrativos; não são dados obrigatórios de API implementada. Não retornar email, senha, `ID_USUARIO` ou `ID_AUTOR_TRANSICAO`. `ID_MUNICIPIO` é obrigatório no Oracle. Se o ID não existe ou foi excluído logicamente, responder `404`.

A resposta Java de orientação deve ser criada usando **fontes oficiais verificadas** relacionadas ao município/categoria, e não inferir telefone/horário.

## 4. Solicitar e consultar orientação (Frontend → Java)

`POST /api/v1/ia/orientacoes` com JWT `CIDADAO` e `{ "ocorrenciaId": "1" }`. Java chama .NET e consulta `VW_TRECHOS_RAG_APROVADOS` em Oracle, usa Spring AI/RAG e registra o resultado em `TB_ORIENTACAO_IA` via `PKG_BASE_IA.PR_REGISTRAR_ORIENTACAO`.

`GET /api/v1/ia/orientacoes/ocorrencias/{id}` retorna apenas orientações explicitamente públicas; conteúdo pessoal não pode vazar.

Estados Oracle existentes: `DISPONIVEL`, `PARCIAL`, `SEM_FONTE`, `LOCALIDADE_NAO_CONFIRMADA`, `ERRO`. Os contatos e horários devem ser opcionais quando não confirmados.

**Nenhuma chamada a modelo de IA deve ser implementada pelo .NET.** Java não altera status nem contabiliza voto.

## 5. Atualizações comunitárias (.NET → Oracle)

`POST /api/v1/ocorrencias/{id}/atualizacoes` recebe:

```json
{ "tipo": "RESOLVIDO", "texto": "A passagem foi liberada.", "anonimoPublico": true }
```

O .NET extrai `ID_USUARIO` do JWT e chama `PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO`. A procedure só conta `RESOLVIDO` quando `ABERTA` e `PERSISTE` quando `RESOLVIDA`, com dois autores distintos por ciclo. Ao atualizar status, a trigger grava auditoria. O .NET devolve estado e contadores consultados no banco após a chamada.

O frontend não envia nem muda `status`, `numeroCiclo` ou `idUsuario` de forma autoritativa. O `ADMIN` não altera o ciclo nem produz votos.

## 6. Contratos para integração Mobile

| Item | Convenção sugerida |
|---|---|
| Identificadores | `NUMBER(19)` em Oracle; **string decimal** nos JSONs para precisão em JavaScript |
| Datas | ISO 8601 com fuso explícito no JSON; Oracle `TIMESTAMP` exige política comum de conversão |
| Booleans | converter `S/N` Oracle em JSON `true/false` |
| Erros | `{ "codigo": "...", "mensagem": "...", "detalhes": [], "correlationId": "..." }` |
| Paginação | `page` e `pageSize` + `totalItems`/`totalPages`; definir limites |
| Repetição de POST | `Idempotency-Key` estável; replays não geram duplicidade |
| Rate limiting | `429` e `Retry-After` quando aplicável |
| Fotos | storage externo; API .NET valida autor, tipo e metadados |

## 7. Alinhamentos pendentes antes de programar as integrações

- Acordar JWT real: segredo **não compartilhado**, assinatura/chave pública, formato exato de claims e validade.
- Definir URLs e formato final dos endpoints no Swagger/OpenAPI de cada API; a lista deste documento é um **contrato sugerido**.
- Resolver como a ocorrência receberá um município cadastrado no Oracle (`ID_MUNICIPIO` obrigatório). GPS é coordenada, não cidade verificada por si só.
- Definir storage de mídia e como vincular upload a criação de ocorrência/atualização com compensação para falhas.
- Validar segurança das chamadas Java → .NET quando precisarem de contexto não público.
- Testar fluxo integrado Java (login) → .NET (registrar) → .NET (ler) → Java (orientação), incluindo 401/403/404/409/429 e ausência de fontes.

**Não usar os scripts Oracle de `DROP TABLE` em banco compartilhado que já possua dados das APIs.**
