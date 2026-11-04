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
* **Agente de Ação (Tool Calling):** Capaz de interpretar comandos operacionais (ex: *"Lance um almoço de R$ 45,50 na conta Itaú"*) e invocar as regras do sistema de forma segura.
* **Rede Multiagente de Análise:** Três agentes especializados que colaboram sequencialmente para classificar o perfil do usuário, auditar riscos de endividamento e prescrever recomendações financeiras personalizadas.

---

## 🛠 Arquitetura e Tecnologias

A solução é estruturada em camadas estritas baseadas em **Clean Architecture**, garantindo testabilidade, baixo acoplamento e independência de infraestrutura.

```
              │   ControleFinanceiro   │
              │          .API          │
              └───────────┬────────────┘
                          │
              ┌───────────▼────────────┐
              │   ControleFinanceiro   │
              │      .Application      │
              └───────────┬────────────┘
                          │___________________________________________________
                          │                         │                         │
              ┌───────────▼────────────┐┌───────────▼────────────┐┌───────────▼────────────┐
              │   ControleFinanceiro   ││   ControleFinanceiro   ││   ControleFinanceiro   │
              │      .Domain           ││      .Infrastructure   ││      .AIServices       │
              └────────────────────────┘└────────────────────────┘└────────────────────────┘
```

### Stack Tecnológica:
* **Linguagem & Runtime:** .NET 8 / C# 12
* **Banco de Dados:** SQL Server via Entity Framework Core 8
* **Documentação & Testes:** Swagger / OpenAPI UI
* **Integração de IA:** SDK Oficial da OpenAI (`OpenAI.Chat` / `OpenAI.Embeddings`)
* **Modelos Utilizados:** `gpt-4o` / `gpt-4o-mini` e `text-embedding-3-small`

---

## 🧩 Módulos do Sistema

### Módulo 1 — Domínio e Infraestrutura Base
* Cadastro e gestão de **Contas Bancárias** (Conta Corrente, Poupança, Investimentos) e **Categorias** (Alimentação, Transporte, Moradia, etc.).
* Controle completo de **Transações** (Receitas e Despesas) com atualização de saldos em tempo real e isolamento seguro de tipos (`TipoTransacao`).

### Módulo 2 — Persistência e Mapeamento
* Mapeamento relacional com Entity Framework Core e Fluent API.
* Injeção de Dependências nativa (`Microsoft.Extensions.DependencyInjection`) configurada no `Program.cs`.
* Padrão Repository e Service Layer para isolamento completo das regras de negócio.

### Módulo 3 — Visão Computacional e Embeddings
* **OCR com GPT-Vision:** Leitura de imagens de comprovantes em formato Base64/arquivo, extraindo automaticamente `Valor`, `Data`, `Estabelecimento` e `Sugestão de Categoria`.
* **Motor de Embeddings:** Vetorização de descrições financeiras para busca semântica e pareamento inteligente.

### Módulo 4 — Inteligência Artificial Avançada e Agentes
* **RAG Chat (`IFinancialChatService`):** Recuperação do contexto financeiro no banco relacional para alimentar os prompts da OpenAI com dados reais e atualizados do usuário.
* **Tool Calling (`IToolCallingService`):** Mapeamento dinâmico de métodos C# como ferramentas (`ChatTool`), permitindo que o LLM execute ações operacionais no backend.
* **Pipeline Multiagente Autônomo (`IMultiAgentOrchestratorService`):** Orquestração sequencial encadeada unindo **ClassifierAgent**, **AuditRiskAgent** e **AdvisorAgent** para relatórios preditivos completos.

---

## 🤖 Recursos de Inteligência Artificial

### 1. RAG (Retrieval-Augmented Generation)
Combina a capacidade de raciocínio do LLM com o contexto relacional do usuário. O serviço `FinancialChatService` busca as transações filtradas por data/conta e injeta o histórico consolidado na chamada ao GPT.

### 2. Tool Calling (Function Calling)
O `ToolRegistryService` expõe os métodos de negócio para a IA em formato JSON Schema:
* `CriarTransacao`: Registra novas movimentações financeiras no banco.
* `ConsultarContasESaldos`: Obtém a lista atualizada de contas e saldos.

O `ToolCallingService` orquestra o loop de execução:
1. Envia a solicitação do usuário e as `ChatTools` registradas.
2. Intercepta quando o LLM solicita a execução de uma ferramenta (`FinishReason.ToolCalls`).
3. Executa a função C# no backend e devolve o resultado para o LLM gerar a resposta final formatada ao usuário.

### 3. Pipeline Multiagentes (Chaining) — Concluído ✅
Fluxo de colaboração sequencial entre três agentes autônomos especializados:

```
[SQL Server] ──► [MultiAgentOrchestrator] ──► [ClassifierAgent] ──► [AuditRiskAgent] ──► [AdvisorAgent] ──► [Resultado Consolidado]
```

1. **`ClassifierAgent`:** Analisa as receitas e despesas filtradas e classifica o perfil financeiro do usuário (ex: Conservador, Moderado, Agressivo).
2. **`AuditRiskAgent`:** Consome as transações e o perfil atribuído pelo classificador para auditar riscos (endividamento, concentração de gastos, ausência de reserva de emergência).
3. **`AdvisorAgent`:** Avalia as saídas dos dois agentes anteriores para elaborar um plano de ação estratégico com recomendações práticas.

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
* `POST /api/Chat/analise-multiagente` — Dispara o pipeline autônomo dos três agentes de IA (Classificador, Auditoria e Consultor) para análise financeira completa.

---

## ⚙️ Configuração do Ambiente e Execução

### Pré-requisitos
* **.NET 8 SDK** instalado.
* **Visual Studio 2022 / Community Edition** (com carga de trabalho de Desenvolvimento Web).
* **SQL Server** ativo (ou LocalDB/EF Core In-Memory).
* Chave de API ativa da **OpenAI**.

### Passo a Passo

1. **Clonar o Repositório:**
   ```bash
   git clone https://github.com/eduardowke/controle-financeiro.git
   cd controle-financeiro
   ```

2. **Configurar a Chave da OpenAI:**
   Adicione sua chave no arquivo `src/ControleFinanceiro.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ControleFinanceiroDb;Trusted_Connection=True;"
     },
     "OpenAI": {
       "ApiKey": "SUA_CHAVE_OPENAI_AQUI"
     }
   }
   ```
   *(Ou utilize o User Secrets do .NET em ambiente de desenvolvimento):*
   ```bash
   dotnet user-secrets set "OpenAI:ApiKey" "SUA_CHAVE_OPENAI" --project src/ControleFinanceiro.API
   ```

3. **Executar as Migrations do Banco de Dados:**
   ```bash
   dotnet ef database update --project src/ControleFinanceiro.Infrastructure --startup-project src/ControleFinanceiro.API
   ```

4. **Executar e Depurar via Visual Studio Community:**
   * Abra a solução `controle-financeiro-ia.sln`.
   * Defina o projeto **`ControleFinanceiro.API`** como **Projeto de Inicialização** (*Set as Startup Project*).
   * Pressione **`F5`** para iniciar o servidor e abrir o **Swagger UI**.

5. **Testar os Agentes no Swagger UI:**
   * Navegue até o endpoint **`POST /api/Chat/analise-multiagente`**.
   * Clique em **Try it out** e execute a requisição.
   * Para depuração linha a linha, coloque um *breakpoint* (`F9`) na Action `AnaliseMultiagente` do `ChatController.cs` e navegue com `F11` (Step Into) / `F10` (Step Over).