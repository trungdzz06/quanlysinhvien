using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHashColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "MSSV",
                keyValue: "18710204",
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "MSSV",
                keyValue: "18710205",
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teachers",
                keyColumn: "TeacherId",
                keyValue: "GV_001",
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teachers",
                keyColumn: "TeacherId",
                keyValue: "GV_002",
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teachers",
                keyColumn: "TeacherId",
                keyValue: "GV_003",
                column: "PasswordHash",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Students");
        }
    }
}
