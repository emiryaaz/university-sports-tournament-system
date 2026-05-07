using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsTournament.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundAndMatchNumberToFixture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MatchNumber",
                table: "Fixtures",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "Fixtures",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchNumber",
                table: "Fixtures");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "Fixtures");
        }
    }
}
