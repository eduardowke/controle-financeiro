namespace ControleFinanceiro.Application.DTOs;

public record MultiAgentAnalysisRequestDto(
    int? ContaId,
    DateTime? DataInicio,
    DateTime? DataFim
);

public record AgentStepResultDto(
    string NomeAgente,
    string Papel,
    string ResumoOutput,
    DateTime Timestamp
);

public record MultiAgentAnalysisResponseDto(
    string PerfilFinanceiroIdentificado,
    string AvaliacaoRiscoEConsumo,
    string RecomendacoesEstrategicas,
    List<AgentStepResultDto> ExecucaoDosAgentes,
    DateTime ProcessadoEm
);