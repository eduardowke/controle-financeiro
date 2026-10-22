using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransacoesController(
    ITransacaoService transacaoService,
    IAICategorizationService aiService) : ControllerBase
{
    /// <summary>
    /// Cria uma transação diretamente.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarTransacaoDto dto)
    {
        var transacao = await transacaoService.CriarTransacaoAsync(dto); //‘ITransacaoService’ não contém uma definição para "CriarTransacaoAsync"
        return CreatedAtAction(nameof(ObterPorId), new { id = transacao.Id }, transacao);
    }

    /// <summary>
    /// Obtém uma transação por Id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var transacao = await transacaoService.ObterPorIdAsync(id); //ITransacaoService’ não contém uma definição para "ObterPorIdAsync"
        if (transacao == null) return NotFound("Transação não encontrada.");
        return Ok(transacao);
    }

    /// <summary>
    /// Categoriza a transação com IA e salva no banco de dados.
    /// </summary>
    [HttpPost("categorizar-e-salvar")]
    public async Task<IActionResult> CategorizarESalvar(
        [FromBody] AnaliseGastoRequestDto dto,
        [FromQuery] int contaId,
        [FromQuery] int categoriaId)
    {
        // 1. Processa o texto com a IA
        var analise = await aiService.CategorizarGastoAsync(dto);

        // 2. Converte o resultado da IA para o DTO de criação
        var criarDto = new CriarTransacaoDto
        {
            Descricao = analise.DescricaoSugerida,
            Valor = analise.Valor,
            Tipo = (ControleFinanceiro.Domain.TipoTransacao)analise.TipoTransacao,
            Data = DateTime.UtcNow,
            IsPago = true,
            ContaId = contaId,
            CategoriaId = categoriaId,
            ConfiancaIA = analise.Confianca
        };

        // 3. Salva no banco de dados via serviço
        var transacaoSalva = await transacaoService.CriarTransacaoAsync(criarDto);

        return Ok(transacaoSalva);
    }
}