using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDataCompensacaoTransacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataCompensacao",
                table: "Transacoes",
                type: "datetime2",
                nullable: true);

            // Deve ficar AQUI, antes de fechar a chave '}' do método Up
            migrationBuilder.Sql("UPDATE Transacoes SET DataCompensacao = Data WHERE IsPago = 1 AND DataCompensacao IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataCompensacao",
                table: "Transacoes");
        }
    }
}