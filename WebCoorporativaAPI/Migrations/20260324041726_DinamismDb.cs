using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCoorporativaAPI.Migrations
{
    /// <inheritdoc />
    public partial class DinamismDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Clave",
                table: "Modulos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Clave",
                table: "Modulos");
        }
    }
}
