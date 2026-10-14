using ControleFinanceiro.Domain;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Conta> Contas => Set<Conta>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapeamentos adicionais e configurações de relacionamentos (Fluent API)
        modelBuilder.Entity<Transacao>()
            .Property(t => t.Valor)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Conta>()
            .Property(c => c.SaldoInicial)
            .HasPrecision(18, 2);
    }
}