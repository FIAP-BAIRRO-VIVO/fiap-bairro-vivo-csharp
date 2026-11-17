# Bairro Vivo — Contexto e contratos para backend .NET

**Global Solution 2026/2 | Turma 2TDSPO (2º semestre)**  
**Pacote de alinhamento:** versão 1.2, atualizado em 17/11/2026.

## Por onde começar

1. Leia `01_Escopo_Funcional_Bairro_Vivo.md` para entender o produto, os papéis, as funcionalidades e a regra das duas confirmações.
2. Leia **`04_Especificacao_Backend_DotNet.md`**: é o documento principal para a implementação da API ASP.NET Core Minimal API.
3. Leia **`05_Contratos_Integracao_Java_DotNet.md`** para compartilhar autenticação JWT e contexto das ocorrências com Java.
4. Use `Dicionario_Dados_Integracao.md` e **`Banco/BairroVivo_SQL_Oracle_v1_2.sql`** para os nomes/tipos exatos do schema.
5. Consulte `02_Documentacao_Frontend_React_Native.md` para as telas e os campos que o app espera; `03_Especificacao_Backend_Java_Spring_Boot.md` explica a parte de login, perfis e IA no outro serviço.
6. O enunciado oficial e a documentação do banco validado estão nas pastas `Requisitos/` e `Referencias/`.

## Arquivos

```text
Bairro_Vivo_Backend_DotNet_Atualizado/
├── README.md
├── 01_Escopo_Funcional_Bairro_Vivo.md
├── 02_Documentacao_Frontend_React_Native.md
├── 03_Especificacao_Backend_Java_Spring_Boot.md
├── 04_Especificacao_Backend_DotNet.md
├── 05_Contratos_Integracao_Java_DotNet.md
├── Dicionario_Dados_Integracao.md
├── Banco/
│   └── BairroVivo_SQL_Oracle_v1_2.sql
├── Requisitos/
│   └── Global_Solution_2026_2_Enunciado_Original.pdf
└── Referencias/
    └── Documentacao_Banco_de_Dados_2TDSPO_A4.pdf
```

## Regras já aprovadas

- **Java:** único responsável por cadastro, login, JWT, identidade/perfil/favoritos e **toda a IA de orientação cidadã (Spring AI/RAG)**.
- **.NET:** ocorrências, categorias, mapa, fotos, atualizações, votos, histórico, relatórios e validação do JWT Java. **Sem IA em .NET**.
- **Oracle:** fonte principal, esquema v1.2 validado pelo grupo com 19 tabelas e packages/triggers. **Não usar MongoDB como banco transacional das APIs**; o MongoDB foi parte da entrega acadêmica de dados.
- **Leitura pública, participação autenticada**; publicação anônima é anonimato de exibição, com autor ainda vinculado no Oracle.
- **Só existem ABERTA/RESOLVIDA**. O fechamento e a reabertura ocorrem pela procedure Oracle após duas confirmações válidas de usuários distintos por ciclo. Nenhum usuário ou ADMIN altera status manualmente.
- A ocorrência exige `LATITUDE`, `LONGITUDE` e `ID_MUNICIPIO` válido. **Não existe `TB_BAIRRO`.**
- **Fotos:** binários no storage externo; Oracle armazena metadados. Foto direta exige o autor original; fotos de terceiros vão em atualizações.
- **ADMIN:** relatórios e indicadores somente leitura; não modera nem publica.
- O .NET tem que atender aos requisitos acadêmicos: **Minimal API, Swagger/OpenAPI, JWT, idempotência, rate limiting 429, HealthChecks e testes xUnit**.

## Estado do projeto

**Banco Oracle v1.2:** estrutura e testes PL/SQL executados e validados pelo grupo. O arquivo SQL incluído corresponde à versão padronizada aprovada (mesmo conteúdo da versão final guardada no repositório de Banco de Dados). Os testes do banco **não comprovam** que o backend .NET ou Java já esteja pronto.

**Frontend/Java/.NET:** o escopo está alinhado, mas rotas, requests, responses, ambientes e autenticação entre serviços devem ser implementados e publicados no Swagger/OpenAPI. Qualquer rota do pacote é **sugerida**, não uma URL ativa.

**Fonte de verdade para tipos e constraints:** SQL v1.2. Em caso de divergência entre um exemplo de DTO e o DDL, a equipe deve reconciliar o contrato antes de codificar; não presumir a alteração do banco.

## Cuidado importante ao executar o SQL

O script Oracle possui uma seção de `DROP` que apaga as tabelas/objetos e recria a carga. **Executá-lo apenas em schema descartável de desenvolvimento**. Em banco compartilhado da equipe, usar a base já criada; não resetar dados de outras APIs. As APIs devem utilizar conexão segura e variáveis de ambiente para credenciais, nunca hardcode no repositório.

## O que foi retirado do pacote anterior

- `script_oracle_bairrovivo.sql` — modelo antigo com tabela de bairros e status descartados.
- `bairro_vivo_oracle_v1_1.sql` — substituído pelo SQL v1.2 validado.
- `Proposta_Bairro_Vivo_Global_Solution_2026.pdf` — material de ideação inicial, com divisão de responsabilidades antiga, não adequado para orientar implementação.

O enunciado original mantém o nome de arquivo fornecido pela faculdade, mas neste pacote foi renomeado para facilitar localização. **A turma correta é 2TDSPO, segundo semestre de 2026**, independentemente do rótulo antigo “Fevereiro” no PDF institucional.
