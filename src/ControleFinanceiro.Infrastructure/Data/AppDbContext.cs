using ControleFinanceiro.Domain;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Banco> Bancos => Set<Banco>();
    public DbSet<Conta> Contas => Set<Conta>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- BANCO ---
        modelBuilder.Entity<Banco>(entity =>
        {
            entity.HasKey(b => b.NumBanco);
            entity.Property(b => b.NumBanco).HasMaxLength(3).IsFixedLength(false);
            entity.Property(b => b.NomeReduzido).HasMaxLength(50).IsRequired();
            entity.Property(b => b.RazaoSocial).HasMaxLength(150).IsRequired();
            entity.Property(b => b.LogoUrl).HasMaxLength(255).IsRequired(false);

            entity.HasData(ObterBancosIniciais());
        });

        // --- CONTA ---
        modelBuilder.Entity<Conta>(entity =>
        {
            entity.Property(c => c.Nome).HasMaxLength(80).IsRequired();
            entity.Property(c => c.NumBanco).HasMaxLength(3).HasDefaultValue("999").IsRequired();
            entity.Property(c => c.Titular).HasMaxLength(100).HasDefaultValue("Titular Padrão").IsRequired();
            entity.Property(c => c.Agencia).HasMaxLength(10).IsRequired(false);
            entity.Property(c => c.NumConta).HasMaxLength(20).IsRequired(false);
            entity.Property(c => c.SaldoInicial).HasPrecision(18, 2);
            entity.Property(c => c.Ativa).HasDefaultValue(true);

            entity.HasOne(c => c.Banco)
                  .WithMany(b => b.Contas)
                  .HasForeignKey(c => c.NumBanco)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- CATEGORIA ---
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.Property(c => c.TipoCategoria).HasDefaultValue(TipoCategoria.Despesa);
        });

        // --- TRANSAÇÃO ---
        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.Property(t => t.Valor).HasPrecision(18, 2);
            entity.Property(t => t.ConfiancaIA).IsRequired(false);
        });
    }

    private static Banco[] ObterBancosIniciais() =>
    [
        new() { NumBanco = "001", NomeReduzido = "Banco do Brasil", RazaoSocial = "BANCO DO BRASIL S.A." },
        new() { NumBanco = "003", NomeReduzido = "Banco da Amazônia", RazaoSocial = "BANCO DA AMAZÔNIA S.A." },
        new() { NumBanco = "004", NomeReduzido = "Banco do Nordeste", RazaoSocial = "BANCO DO NORDESTE DO BRASIL S.A." },
        new() { NumBanco = "033", NomeReduzido = "Santander", RazaoSocial = "BANCO SANTANDER (BRASIL) S.A." },
        new() { NumBanco = "041", NomeReduzido = "Banrisul", RazaoSocial = "BANCO DO ESTADO DO RIO GRANDE DO SUL S.A." },
        new() { NumBanco = "070", NomeReduzido = "BRB", RazaoSocial = "BRB - BANCO DE BRASÍLIA S.A." },
        new() { NumBanco = "077", NomeReduzido = "Inter", RazaoSocial = "BANCO INTER S.A." },
        new() { NumBanco = "104", NomeReduzido = "Caixa", RazaoSocial = "CAIXA ECONÔMICA FEDERAL" },
        new() { NumBanco = "197", NomeReduzido = "Stone", RazaoSocial = "STONE INSTITUIÇÃO DE PAGAMENTO S.A." },
        new() { NumBanco = "208", NomeReduzido = "BTG Pactual", RazaoSocial = "BANCO BTG PACTUAL S.A." },
        new() { NumBanco = "212", NomeReduzido = "Banco Original", RazaoSocial = "BANCO ORIGINAL S.A." },
        new() { NumBanco = "218", NomeReduzido = "BS2", RazaoSocial = "BANCO BS2 S.A." },
        new() { NumBanco = "237", NomeReduzido = "Bradesco", RazaoSocial = "BANCO BRADESCO S.A." },
        new() { NumBanco = "260", NomeReduzido = "Nubank", RazaoSocial = "NU PAGAMENTOS S.A. - INSTITUIÇÃO DE PAGAMENTO" },
        new() { NumBanco = "290", NomeReduzido = "PagBank", RazaoSocial = "PAGSEGURO INTERNET INSTITUIÇÃO DE PAGAMENTO S.A." },
        new() { NumBanco = "318", NomeReduzido = "BMG", RazaoSocial = "BANCO BMG S.A." },
        new() { NumBanco = "323", NomeReduzido = "Mercado Pago", RazaoSocial = "MERCADO PAGO INSTITUIÇÃO DE PAGAMENTO LTDA." },
        new() { NumBanco = "336", NomeReduzido = "C6 Bank", RazaoSocial = "BANCO C6 S.A." },
        new() { NumBanco = "341", NomeReduzido = "Itaú", RazaoSocial = "ITAÚ UNIBANCO S.A." },
        new() { NumBanco = "380", NomeReduzido = "PicPay", RazaoSocial = "PICPAY INSTITUIÇÃO DE PAGAMENTO S.A." },
        new() { NumBanco = "403", NomeReduzido = "Cora", RazaoSocial = "CORA SOCIEDADE DE CRÉDITO DIRETO S.A." },
        new() { NumBanco = "422", NomeReduzido = "Safra", RazaoSocial = "BANCO SAFRA S.A." },
        new() { NumBanco = "536", NomeReduzido = "Neon", RazaoSocial = "NEON PAGAMENTOS S.A. INSTITUIÇÃO DE PAGAMENTO" },
        new() { NumBanco = "623", NomeReduzido = "Banco Pan", RazaoSocial = "BANCO PAN S.A." },
        new() { NumBanco = "637", NomeReduzido = "Sofisa Direto", RazaoSocial = "BANCO SOFISA S.A." },
        new() { NumBanco = "655", NomeReduzido = "Votorantim / BV", RazaoSocial = "BANCO VOTORANTIM S.A." },
        new() { NumBanco = "707", NomeReduzido = "Daycoval", RazaoSocial = "BANCO DAYCOVAL S.A." },
        new() { NumBanco = "745", NomeReduzido = "Citibank", RazaoSocial = "BANCO CITIBANK S.A." },
        new() { NumBanco = "748", NomeReduzido = "Sicredi", RazaoSocial = "BANCO COOPERATIVO SICREDI S.A." },
        new() { NumBanco = "756", NomeReduzido = "Sicoob", RazaoSocial = "BANCO COOPERATIVO SICOOB S.A." },
        new() { NumBanco = "999", NomeReduzido = "Outros / Carteira", RazaoSocial = "OUTRAS INSTITUIÇÕES OU CARTEIRA FÍSICA" }
    ];
}