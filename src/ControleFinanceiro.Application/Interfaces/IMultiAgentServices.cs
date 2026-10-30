using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

/// <summary>
/// Agente 1: Analisa transações e classifica o perfil de consumo do usuário.
/// </summary>
public interface IClassifierAgent
{
    Task<string> ClassificarPerfilAsync(string contextoTransacoes, CancellationToken cancellationToken = default);
}

/// <summary>
/// Agente 2: Examina os dados e o perfil para identificar anomalias, riscos e gastos atípicos.
/// </summary>
public interface IAuditRiskAgent
{
    Task<string> AuditarRiscosAsync(string contextoTransacoes, string perfilClassificado, CancellationToken cancellationToken = default);
}

/// <summary>
/// Agente 3: Gera plano de ação e orientações de investimentos com base no perfil e riscos detectados.
/// </summary>
public interface IAdvisorAgent
{
    Task<string> GerarRecomendacoesAsync(string perfil, string analiseRisco, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service Orquestrador que coordena o pipeline entre os três agentes.
/// </summary>
public interface IMultiAgentOrchestratorService
{
    Task<MultiAgentAnalysisResponseDto> ExecutarAnaliseCompletaAsync(
        MultiAgentAnalysisRequestDto request, 
        CancellationToken cancellationToken = default);
}