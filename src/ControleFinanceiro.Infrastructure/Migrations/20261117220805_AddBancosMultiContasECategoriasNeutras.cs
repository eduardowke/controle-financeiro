using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBancosMultiContasECategoriasNeutras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Contas",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Agencia",
                table: "Contas",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativa",
                table: "Contas",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "NumBanco",
                table: "Contas",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "999");

            migrationBuilder.AddColumn<string>(
                name: "NumConta",
                table: "Contas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoConta",
                table: "Contas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Titular",
                table: "Contas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Titular Padrão");

            migrationBuilder.AddColumn<int>(
                name: "TipoCategoria",
                table: "Categorias",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateTable(
                name: "Bancos",
                columns: table => new
                {
                    NumBanco = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    NomeReduzido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RazaoSocial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bancos", x => x.NumBanco);
                });

            migrationBuilder.InsertData(
                table: "Bancos",
                columns: new[] { "NumBanco", "LogoUrl", "NomeReduzido", "RazaoSocial" },
                values: new object[,]
                {
                    { "001", null, "Banco do Brasil", "BANCO DO BRASIL S.A." },
                    { "003", null, "Banco da Amazônia", "BANCO DA AMAZÔNIA S.A." },
                    { "004", null, "Banco do Nordeste", "BANCO DO NORDESTE DO BRASIL S.A." },
                    { "033", null, "Santander", "BANCO SANTANDER (BRASIL) S.A." },
                    { "041", null, "Banrisul", "BANCO DO ESTADO DO RIO GRANDE DO SUL S.A." },
                    { "070", null, "BRB", "BRB - BANCO DE BRASÍLIA S.A." },
                    { "077", null, "Inter", "BANCO INTER S.A." },
                    { "104", null, "Caixa", "CAIXA ECONÔMICA FEDERAL" },
                    { "197", null, "Stone", "STONE INSTITUIÇÃO DE PAGAMENTO S.A." },
                    { "208", null, "BTG Pactual", "BANCO BTG PACTUAL S.A." },
                    { "212", null, "Banco Original", "BANCO ORIGINAL S.A." },
                    { "218", null, "BS2", "BANCO BS2 S.A." },
                    { "237", null, "Bradesco", "BANCO BRADESCO S.A." },
                    { "260", null, "Nubank", "NU PAGAMENTOS S.A. - INSTITUIÇÃO DE PAGAMENTO" },
                    { "290", null, "PagBank", "PAGSEGURO INTERNET INSTITUIÇÃO DE PAGAMENTO S.A." },
                    { "318", null, "BMG", "BANCO BMG S.A." },
                    { "323", null, "Mercado Pago", "MERCADO PAGO INSTITUIÇÃO DE PAGAMENTO LTDA." },
                    { "336", null, "C6 Bank", "BANCO C6 S.A." },
                    { "341", null, "Itaú", "ITAÚ UNIBANCO S.A." },
                    { "380", null, "PicPay", "PICPAY INSTITUIÇÃO DE PAGAMENTO S.A." },
                    { "403", null, "Cora", "CORA SOCIEDADE DE CRÉDITO DIRETO S.A." },
                    { "422", null, "Safra", "BANCO SAFRA S.A." },
                    { "536", null, "Neon", "NEON PAGAMENTOS S.A. INSTITUIÇÃO DE PAGAMENTO" },
                    { "623", null, "Banco Pan", "BANCO PAN S.A." },
                    { "637", null, "Sofisa Direto", "BANCO SOFISA S.A." },
                    { "655", null, "Votorantim / BV", "BANCO VOTORANTIM S.A." },
                    { "707", null, "Daycoval", "BANCO DAYCOVAL S.A." },
                    { "745", null, "Citibank", "BANCO CITIBANK S.A." },
                    { "748", null, "Sicredi", "BANCO COOPERATIVO SICREDI S.A." },
                    { "756", null, "Sicoob", "BANCO COOPERATIVO SICOOB S.A." },
                    { "999", null, "Outros / Carteira", "OUTRAS INSTITUIÇÕES OU CARTEIRA FÍSICA" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contas_NumBanco",
                table: "Contas",
                column: "NumBanco");

            migrationBuilder.AddForeignKey(
                name: "FK_Contas_Bancos_NumBanco",
                table: "Contas",
                column: "NumBanco",
                principalTable: "Bancos",
                principalColumn: "NumBanco",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.Sql("UPDATE Categorias SET TipoCategoria = CASE WHEN IsReceita = 1 THEN 1 ELSE 2 END;");
            migrationBuilder.Sql(@"
                                    IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nome = 'Transferência')
                                    INSERT INTO Categorias (Nome, Descricao, IsReceita, TipoCategoria) VALUES ('Transferência', 'Movimentação de saldo entre contas próprias', 0, 3);
                                    IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nome = 'Pagamento de Fatura')
                                    INSERT INTO Categorias (Nome, Descricao, IsReceita, TipoCategoria) VALUES ('Pagamento de Fatura', 'Quitação de fatura de cartão de crédito', 0, 3);");
            migrationBuilder.Sql("UPDATE Contas SET TipoConta = 1 WHERE TipoConta = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contas_Bancos_NumBanco",
                table: "Contas");

            migrationBuilder.DropTable(
                name: "Bancos");

            migrationBuilder.DropIndex(
                name: "IX_Contas_NumBanco",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "Agencia",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "Ativa",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "NumBanco",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "NumConta",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "TipoConta",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "Titular",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "TipoCategoria",
                table: "Categorias");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Contas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);
        }
    }
}
