using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsTournament.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakeWinnerTeamNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchResults_Teams_WinnerTeamId",
                table: "MatchResults");

            migrationBuilder.AlterColumn<int>(
                name: "WinnerTeamId",
                table: "MatchResults",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResults_Teams_WinnerTeamId",
                table: "MatchResults",
                column: "WinnerTeamId",
                principalTable: "Teams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchResults_Teams_WinnerTeamId",
                table: "MatchResults");

            migrationBuilder.AlterColumn<int>(
                name: "WinnerTeamId",
                table: "MatchResults",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResults_Teams_WinnerTeamId",
                table: "MatchResults",
                column: "WinnerTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
