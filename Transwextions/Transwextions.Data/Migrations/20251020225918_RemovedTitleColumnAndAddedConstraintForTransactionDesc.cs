using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transwextions.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTitleColumnAndAddedConstraintForTransactionDesc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
