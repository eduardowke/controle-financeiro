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
builder.Services.AddScoped<IBancoService, BancoService>();
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

// Adiciona a política de CORS liberando acesso local para o Frontend React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Portas padrões do React/Vite
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =========================================================================
// CONSTRUÇÃO DA APLICAÇÃO (Após esta linha, builder.Services fica Read-Only)
// =========================================================================
var app = builder.Build();

// Ativa a política de CORS "AllowReactApp" (configurada acima em builder.Services)
app.UseCors("AllowReactApp");

// Configuração do Pipeline HTTP / Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapGet("/", () => "API do Controle Financeiro Rodando!");

// --- ENDPOINTS DE CONTAS ---
// --- ENDPOINTS DE BANCOS ---
app.MapGet("/api/bancos", async (string? filtro, IBancoService service) =>
{
    var bancos = await service.ObterTodosAsync(filtro);
    return Results.Ok(bancos);
});

app.MapGet("/api/bancos/{numBanco}", async (string numBanco, IBancoService service) =>
{
    var banco = await service.ObterPorNumeroAsync(numBanco);
    return banco is null ? Results.NotFound("Banco não encontrado.") : Results.Ok(banco);
});

// --- ENDPOINTS DE CONTAS ---
app.MapPost("/api/contas", async (CriarContaDto dto, IContaService service) =>
{
    try
    {
        var conta = await service.CriarAsync(dto);
        return Results.Created($"/api/contas/{conta.Id}", conta);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }
});

app.MapGet("/api/contas", async (bool? incluirInativas, IContaService service) =>
{
    var contas = await service.ObterTodasAsync(incluirInativas ?? false);
    return Results.Ok(contas);
});

app.MapGet("/api/contas/{id:int}", async (int id, IContaService service) =>
{
    var conta = await service.ObterPorIdAsync(id);
    return conta is null ? Results.NotFound("Conta não encontrada.") : Results.Ok(conta);
});

app.MapPut("/api/contas/{id:int}", async (int id, AtualizarContaDto dto, IContaService service) =>
{
    try
    {
        var conta = await service.AtualizarAsync(id, dto);
        return conta is null ? Results.NotFound("Conta não encontrada.") : Results.Ok(conta);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }
});

app.MapPatch("/api/contas/{id:int}/status", async (int id, bool ativa, IContaService service) =>
{
    var atualizado = await service.AlterarStatusAsync(id, ativa);
    return atualizado ? Results.NoContent() : Results.NotFound("Conta não encontrada.");
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