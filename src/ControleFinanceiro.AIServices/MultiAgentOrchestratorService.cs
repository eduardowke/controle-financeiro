using System.Text;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;

namespace ControleFinanceiro.AIServices;

public class MultiAgentOrchestratorService : IMultiAgentOrchestratorService
{
    private readonly IClassifierAgent _classifierAgent;
    private readonly IAuditRiskAgent _auditRiskAgent;
    private readonly IAdvisorAgent _advisorAgent;
    private readonly ITransacaoRepository _transacaoRepository;

    public MultiAgentOrchestratorService(
        IClassifierAgent classifierAgent,
        IAuditRiskAgent auditRiskAgent,
        IAdvisorAgent advisorAgent,
        ITransacaoRepository transacaoRepository)
    {
        _classifierAgent = classifierAgent;
        _auditRiskAgent = auditRiskAgent;
        _advisorAgent = advisorAgent;
        _transacaoRepository = transacaoRepository;
    }

    public async Task<MultiAgentAnalysisResponseDto> ExecutarAnaliseCompletaAsync(
        MultiAgentAnalysisRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        // 0. Retrieval: Busca o histórico de transações no banco de dados com base nos filtros informados
        var transacoes = await _transacaoRepository.ObterTodasAsync();

        if (request.ContaId.HasValue)
        {
            transacoes = transacoes.Where(t => t.ContaId == request.ContaId.Value);
        }

        if (request.DataInicio.HasValue)
        {
            transacoes = transacoes.Where(t => t.Data >= request.DataInicio.Value);
        }

        if (request.DataFim.HasValue)
        {
            transacoes = transacoes.Where(t => t.Data <= request.DataFim.Value);
        }

        var listaTransacoes = transacoes.ToList();

        if (!listaTransacoes.Any())
        {
            return new MultiAgentAnalysisResponseDto(
                PerfilFinanceiroIdentificado: "Sem dados suficientes",
                AvaliacaoRiscoEConsumo: "Não foram encontradas transações para o período informado.",
                RecomendacoesEstrategicas: "Cadastre movimentações para habilitar a análise multiagente.",
                ExecucaoDosAgentes: new List<AgentStepResultDto>(),
                ProcessadoEm: DateTime.UtcNow
            );
        }

        // Formata o histórico de transações para texto plano
        var sb = new StringBuilder();
        foreach (var t in listaTransacoes)
        {
            var tipoStr = (int)t.Tipo == 2 ? "Receita" : "Despesa";
            sb.AppendLine($"- ID: {t.Id} | Data:  {t.Data:dd/MM/yyyy} | Tipo: {tipoStr} | Valor: R$ {t.Valor:F2} | Descrição: {t.Descricao}");
        }

        var contextoTransacoes = sb.ToString();
        var etapas = new List<AgentStepResultDto>();

        // 1. Passo 1: Execução do ClassifierAgent
        var perfil = await _classifierAgent.ClassificarPerfilAsync(contextoTransacoes, cancellationToken);
        etapas.Add(new AgentStepResultDto("ClassifierAgent", "Classificação de Perfil", perfil, DateTime.UtcNow));

        // 2. Passo 2: Execução do AuditRiskAgent
        var riscos = await _auditRiskAgent.AuditarRiscosAsync(contextoTransacoes, perfil, cancellationToken);
        etapas.Add(new AgentStepResultDto("AuditRiskAgent", "Auditoria de Riscos", riscos, DateTime.UtcNow));

        // 3. Passo 3: Execução do AdvisorAgent
        var recomendacoes = await _advisorAgent.GerarRecomendacoesAsync(perfil, riscos, cancellationToken);
        etapas.Add(new AgentStepResultDto("AdvisorAgent", "Consultoria e Recomendações", recomendacoes, DateTime.UtcNow));

        // Consolida e retorna o resultado estruturado
        return new MultiAgentAnalysisResponseDto(
            PerfilFinanceiroIdentificado: perfil,
            AvaliacaoRiscoEConsumo: riscos,
            RecomendacoesEstrategicas: recomendacoes,
            ExecucaoDosAgentes: etapas,
            ProcessadoEm: DateTime.UtcNow
        );
    }
}