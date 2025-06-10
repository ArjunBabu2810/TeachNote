using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachNote_Backend.Migrations
{
    /// <inheritdoc />
    public partial class userTableChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marks_Subjects_subjecyId",
                table: "Marks");

            migrationBuilder.DropIndex(
                name: "IX_Marks_subjecyId",
                table: "Marks");

            migrationBuilder.DropColumn(
                name: "subjecyId",
                table: "Marks");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "email");

            migrationBuilder.CreateIndex(
                name: "IX_Marks_subjectId",
                table: "Marks",
                column: "subjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marks_Subjects_subjectId",
                table: "Marks",
                column: "subjectId",
                principalTable: "Subjects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marks_Subjects_subjectId",
                table: "Marks");

            migrationBuilder.DropIndex(
                name: "IX_Marks_subjectId",
                table: "Marks");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.AddColumn<int>(
                name: "subjecyId",
                table: "Marks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Marks_subjecyId",
                table: "Marks",
                column: "subjecyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marks_Subjects_subjecyId",
                table: "Marks",
                column: "subjecyId",
                principalTable: "Subjects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
