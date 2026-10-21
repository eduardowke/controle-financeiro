using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain;
using ControleFinanceiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Services;

public class CategoriaService : ICategoriaService
{
    private readonly AppDbContext _context;

    public CategoriaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CategoriaResponseDto> CriarAsync(CriarCategoriaDto dto)
    {
        var categoria = new Categoria
        {
            Nome = dto.Nome
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return new CategoriaResponseDto
        {
            Id = categoria.Id,
            Nome = categoria.Nome
        };
    }

    public async Task<IEnumerable<CategoriaResponseDto>> ObterTodasAsync()
    {
        return await _context.Categorias
            .Select(c => new CategoriaResponseDto
            {
                Id = c.Id,
                Nome = c.Nome
            })
            .ToListAsync();
    }
}