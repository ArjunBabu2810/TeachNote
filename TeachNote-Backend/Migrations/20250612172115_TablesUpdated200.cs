using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachNote_Backend.Migrations
{
    /// <inheritdoc />
    public partial class TablesUpdated200 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "userId",
                table: "Notes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_userId",
                table: "Notes",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Users_userId",
                table: "Notes",
                column: "userId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Users_userId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_userId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "userId",
                table: "Notes");
        }
    }
}
