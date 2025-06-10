using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachNote_Backend.Migrations
{
    /// <inheritdoc />
    public partial class TablesUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Departments_departmentId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_departmentId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "departmentId",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "semesterExam",
                table: "Marks",
                newName: "external");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "Notes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "pdfFile",
                table: "Notes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "pdfFile",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "external",
                table: "Marks",
                newName: "semesterExam");

            migrationBuilder.AddColumn<int>(
                name: "departmentId",
                table: "Notes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_departmentId",
                table: "Notes",
                column: "departmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Departments_departmentId",
                table: "Notes",
                column: "departmentId",
                principalTable: "Departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
