    # 💰 API de Controle Financeiro Pessoal com IA Autônoma

API RESTful desenvolvida em **.NET 8** e **C# 12** sob os princípios da **Clean Architecture**, integrada à **OpenAI SDK** para oferecer um ecossistema completo de gestão financeira com recursos avançados de **Visão Computacional (OCR)**, **Busca Vetorial (Embeddings / RAG)**, **Execução de Ações Autônomas (Tool Calling)** e **Orquestração Multiagentes**.

---

## 📌 Sumário
1. [Visão Geral de Negócio](#-visão-geral-de-negócio)
2. [Arquitetura e Tecnologias](#-arquitetura-e-tecnologias)
3. [Módulos do Sistema](#-módulos-do-sistema)
4. [Recursos de Inteligência Artificial](#-recursos-de-inteligência-artificial)
5. [Catálogo de Endpoints REST](#-catálogo-de-endpoints-rest)
6. [Configuração do Ambiente e Execução](#-configuração-do-ambiente-e-execução)

---

## 🏢 Visão Geral de Negócio

O sistema resolve a complexidade no acompanhamento financeiro pessoal, substituindo o preenchimento manual e passivo por uma experiência guiada por Inteligência Artificial autônoma.

### Principais Benefícios:
* **Entrada de Dados Sem Fricção:** Leitura automática de comprovantes físicos (cupons fiscais, recibos, notas) via IA generativa visual, extraindo dados sem necessidade de digitação.
* **Assistente Conversacional RAG:** Consultas em linguagem natural sobre o histórico de movimentações, calculando gastos e agregando valores em tempo real.
* **Agente de Ação (Tool Calling):** Capaz de interpretar comandos operacionais (ex: *"Lance um almoço de R$ 45,50 no Itaú"*) e invocar as regras do sistema de forma segura.
* **Rede Multiagente de Análise:** Três agentes especializados que colaboram sequencialmente para classificar o perfil do usuário, auditar riscos de endividamento e prescrever recomendações financeiras.

---

## 🛠 Arquitetura e Tecnologias

A solução é estruturada em camadas estritas baseadas em **Clean Architecture**, garantindo testabilidade, baixo acoplamento e independência de infraestrutura.

              │   ControleFinanceiro   │
              │          .API          │
              └───────────┬────────────┘
                          │
              ┌───────────▼────────────┐
              │   ControleFinanceiro   │
              │      .Application      │
              └───────────┬────────────┘
     ┌────────────────────┼────────────────────┐
     │                    │                    │
┌────────▼─────────┐ ┌────────▼─────────┐ ┌────────▼─────────┐
│ControleFinanceiro│ │ControleFinanceiro│ │ControleFinanceiro│
│     .Domain      │ │ .Infrastructure  │ │   .AIServices    │
└──────────────────┘ └──────────────────┘ └──────────────────┘

### Stack Tecnológica:
* **Linguagem & Runtime:** .NET 8 / C# 12
* **Banco de Dados:** SQL Server via Entity Framework Core 8
* **Documentação & Testes:** Swagger / OpenAPI UI
* **Integração de IA:** SDK Oficial da OpenAI (`OpenAI.Chat`)
* **Modelos Utilizados:** `gpt-4o` / `gpt-4o-mini` e `text-embedding-3-small`

---

## 🧩 Módulos do Sistema

### Módulo 1 — Domínio e Infraestrutura Base
* Cadastro e gestão de **Contas Bancárias** (Conta Corrente, Poupança, Investimentos) e **Categorias** (Alimentação, Transporte, Moradia, etc.).
* Controle completo de **Transações** (Receitas e Despesas) com atualização em tempo real dos saldos.

### Módulo 2 — Persistência e Mapeamento
* Mapeamento relacional com Fluent API.
* Injeção de Dependências nativa (`Microsoft.Extensions.DependencyInjection`).
* Padrão Repository e Service Layer para isolamento de regras de negócio.

### Módulo 3 — Visão Computacional e Embeddings
* **OCR com GPT-Vision:** Leitura de imagens de comprovantes em formato Base64/arquivo, extraindo automaticamente `Valor`, `Data`, `Estabelecimento` e `Sugestão de Categoria`.
* **Motor de Embeddings:** Vetorização de descrições financeiras para busca semântica e pareamento inteligente.

### Módulo 4 — Inteligência Artificial Avançada
* **US07 (RAG Chat):** Recuperação de contexto financeiro no banco de dados para alimentar prompts da OpenAI com dados reais do usuário.
* **US08 (Tool Calling / Function Calling):** Mapeamento dinâmico dos métodos C# como ferramentas (`ChatTool`), permitindo que o LLM execute chamadas no backend.
* **US09 (Orquestração Multiagentes - Em pipeline):** Pipeline sequencial autônomo unindo Classificador de Perfil, Auditor de Riscos e Consultor Financeiro.

---

## 🤖 Recursos de Inteligência Artificial

### 1. RAG (Retrieval-Augmented Generation)
Combina a capacidade de raciocínio do LLM com o contexto relacional do usuário. O serviço `FinancialChatService` busca as transações filtradas por data/conta e injeta o histórico consolidado na chamada ao GPT.

### 2. Tool Calling (Function Calling)
O `ToolRegistryService` expõe os métodos de negócio para a IA em formato JSON Schema:
* `CriarTransacao`: Registra novas movimentações financeiras.
* `ConsultarContasESaldos`: Obtém a lista atualizada de contas e saldos.

O `ToolCallingService` orquestra o loop de execução:
1. Envia a solicitação do usuário e as `ChatTools` registradas.
2. Intercepta quando o LLM solicita a execução de uma ferramenta (`FinishReason.ToolCalls`).
3. Executa a função no C# e envia o resultado de volta para o LLM gerar a resposta final ao usuário.

### 3. Pipeline Multiagentes (Chaining)
Fluxo de colaboração entre agentes autônomos especializados:

[SQL Server] ──► [Orquestrador] ──► [ClassifierAgent] ──► [AuditRiskAgent] ──► [AdvisorAgent]


---

## 🌐 Catálogo de Endpoints REST

### Contas (`/api/Contas`)
* `GET /api/Contas` — Lista todas as contas cadastradas com seus saldos.
* `POST /api/Contas` — Cadastra uma nova conta bancária.

### Transações (`/api/Transacoes`)
* `GET /api/Transacoes` — Lista o histórico de transações registradas.
* `POST /api/Transacoes` — Registra uma nova transação financeira.
* `POST /api/Transacoes/ocr` — Processa a imagem de um comprovante fiscal e extrai os dados via GPT-Vision.

### Assistente e Agentes IA (`/api/Chat`)
* `POST /api/Chat/perguntar` — Consulta o assistente RAG sobre o histórico financeiro em linguagem natural.
* `POST /api/Chat/acao` — Envia comandos em linguagem natural para execução de ações reais via Tool Calling.
* `POST /api/Chat/analise-multiagente` — Dispara a análise completa de perfil, riscos e recomendações pelo pipeline multiagente.

---

## ⚙️ Configuração do Ambiente e Execução

### Pré-requisitos
* **.NET 8 SDK** instalado.
* **SQL Server** ativo ou LocalDB.
* Chave de API da **OpenAI**.

### Passo a Passo

1. **Clonar o Repositório:**
   ```bash
   git clone <url-do-repositorio>
   cd controle-financeiro-ia
Configurar a Chave da OpenAI:
Adicione sua chave no arquivo src/ControleFinanceiro.API/appsettings.json ou via Secret Manager:

Bash
dotnet user-secrets set "OpenAI:ApiKey" "SUA_CHAVE_OPENAI" --project src/ControleFinanceiro.API
Executar as Migrations do Banco de Dados:

Bash
dotnet ef database update --project src/ControleFinanceiro.Infrastructure --startup-project src/ControleFinanceiro.API
Iniciar a Aplicação:

Bash
dotnet run --project src/ControleFinanceiro.API
Acessar a Documentação Interativa:
Navegue até https://localhost:7184/swagger no seu navegador para interagir com todos os endpoints.


---

<ElicitationsGroup message="O que deseja fazer em seguida com o repositório?">
  <Elicitation label="Salvar o README.md e retornar para a Task 2 da US09" query="Criei o arquivo README.md com a documentação completa. Vamos retornar à US09 e iniciar a Task 2 — Implementação dos Agentes Especializados."/>
  <Elicitation label="Registrar um commit no Git com a documentação do projeto" query="Como registramos no Git a criação do arquivo README.md com a documentação técnica e de negócio?"/>
</ElicitationsGroup>

<FollowUp label="Quer que eu ori