using ControleFinanceiro.AIServices;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Infrastructure.Data;
using ControleFinanceiro.Infrastructure.Repositories;
using ControleFinanceiro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext com SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro dos Serviços de Domínio e Infraestrutura na Injeção de Dependência
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IAICategorizationService, AICategorizationService>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IReceiptOCRService, ReceiptOCRService>();

// Registro dos Controllers
builder.Services.AddControllers();

// Configuração da OpenAI (EmbeddingService e ChatClient)
var openAiApiKey = builder.Configuration["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException("A chave 'OpenAI:ApiKey' não foi configurada no appsettings.json.");

builder.Services.AddSingleton<IEmbeddingService>(new EmbeddingService(openAiApiKey));
builder.Services.AddSingleton(new ChatClient(model: "gpt-4o-mini", apiKey: openAiApiKey));

// Configuração do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registra os Agentes Especializados da US-254
builder.Services.AddScoped<IClassifierAgent, ClassifierAgent>();
builder.Services.AddScoped<IAuditRiskAgent, AuditRiskAgent>();
builder.Services.AddScoped<IAdvisorAgent, AdvisorAgent>();

// Registra o Orquestrador Multiagentes
builder.Services.AddScoped<IMultiAgentOrchestratorService, MultiAgentOrchestratorService>();

// Registro da interface e serviço do chat
builder.Services.AddScoped<IFinancialChatService, FinancialChatService>();

// Tool Calling Services
builder.Services.AddScoped<IToolCallingService, ToolCallingService>();
builder.Services.AddScoped<IToolRegistryService, ToolRegistryService>();

// =========================================================================
// CONSTRUÇÃO DA APLICAÇÃO (Após esta linha, builder.Services fica Read-Only)
// =========================================================================
var app = builder.Build();

// Configuração do Pipeline HTTP / Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapGet("/", () => "API do Controle Financeiro Rodando!");

// --- ENDPOINTS DE CONTAS ---
app.MapPost("/api/contas", async (CriarContaDto dto, IContaService service) =>
{
    var conta = await service.CriarAsync(dto);
    return Results.Created($"/api/contas/{conta.Id}", conta);
});

app.MapGet("/api/contas", async (IContaService service) =>
{
    var contas = await service.ObterTodasAsync();
    return Results.Ok(contas);
});

// --- ENDPOINTS DE CATEGORIAS ---
app.MapPost("/api/categorias", async (CriarCategoriaDto dto, ICategoriaService service) =>
{
    var categoria = await service.CriarAsync(dto);
    return Results.Created($"/api/categorias/{categoria.Id}", categoria);
});

app.MapGet("/api/categorias", async (ICategoriaService service) =>
{
    var categorias = await service.ObterTodasAsync();
    return Results.Ok(categorias);
});

// --- ENDPOINTS DE TRANSAÇÕES ---
app.MapPost("/api/transacoes", async (CriarTransacaoDto dto, ITransacaoService service) =>
{
    var transacao = await service.CriarTransacaoAsync(dto);
    return Results.Created($"/api/transacoes/{transacao.Id}", transacao);
});

app.MapGet("/api/transacoes", async (ITransacaoService service) =>
{
    var transacoes = await service.ObterTodasAsync();
    return Results.Ok(transacoes);
});

// --- ENDPOINT DE IA (CATEGORIZADOR) ---
app.MapPost("/api/ia/categorizar", async (AnaliseGastoRequestDto dto, IAICategorizationService aiService, CancellationToken cancellationToken) =>
{
    var resultado = await aiService.CategorizarDescricaoAsync(dto.TextoLivre, cancellationToken);
    return Results.Ok(resultado);
});

app.Run();