using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransacoesController(
    ITransacaoService transacaoService,
    IAICategorizationService aiService,
    IReceiptOCRService receiptOcrService) : ControllerBase
{
    /// <summary>
    /// Cria uma transação diretamente.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarTransacaoDto dto)
    {
        var transacao = await transacaoService.CriarTransacaoAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = transacao.Id }, transacao);
    }

    /// <summary>
    /// Obtém uma transação por Id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var transacao = await transacaoService.ObterPorIdAsync(id);
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
        [FromQuery] int categoriaId,
        CancellationToken cancellationToken)
    {
        // 1. Processa a descrição com a IA (Busca Vetorial + Fallback LLM)
        var analise = await aiService.CategorizarDescricaoAsync(dto.TextoLivre, cancellationToken);

        // 2. Converte o resultado da IA e requisição para o DTO de criação
        var criarDto = new CriarTransacaoDto
        {
            Descricao = dto.TextoLivre,
            Valor = 0.0m,
            Tipo = ControleFinanceiro.Domain.TipoTransacao.Despesa,
            Data = DateTime.UtcNow,
            IsPago = true,
            ContaId = contaId,
            CategoriaId = analise.CategoriaId ?? categoriaId,
            ConfiancaIA = analise.ConfiancaIA
        };

        // 3. Salva no banco de dados via serviço
        var transacaoSalva = await transacaoService.CriarTransacaoAsync(criarDto);

        return Ok(transacaoSalva);
    }

    /// <summary>
    /// Envia uma imagem de recibo/comprovante para extração automática via OCR (Vision).
    /// </summary>
    [HttpPost("upload-recibo")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ReciboExtraidoDto>> UploadRecibo(
        IFormFile arquivo,
        CancellationToken cancellationToken)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest("É necessário fornecer uma imagem válida de recibo.");

        // Validação básica de extensão do arquivo
        var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();

        if (!extensoesPermitidas.Contains(extensao))
            return BadRequest("Formato de arquivo não suportado. Envie uma imagem JPG, PNG ou WEBP.");

        // Validação de tamanho máximo (ex: 5 MB)
        if (arquivo.Length > 5 * 1024 * 1024)
            return BadRequest("O arquivo de imagem excede o limite máximo permitido de 5 MB.");

        await using var stream = arquivo.OpenReadStream();
        
        var resultado = await receiptOcrService.ProcessarReciboAsync(
            stream, 
            arquivo.ContentType, 
            cancellationToken);

        return Ok(resultado);
    }
}