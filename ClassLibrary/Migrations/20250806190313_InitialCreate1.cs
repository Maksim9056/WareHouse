using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassLibrary.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "TypeDoc",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Condition",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Condition",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "Подписан");

            migrationBuilder.UpdateData(
                table: "Condition",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "Отозван");

            migrationBuilder.UpdateData(
                table: "Condition",
                keyColumn: "Id",
                keyValue: 3,
                column: "Code",
                value: "Не подписан");

            migrationBuilder.UpdateData(
                table: "Condition",
                keyColumn: "Id",
                keyValue: 4,
                column: "Code",
                value: "В наличии");

            migrationBuilder.UpdateData(
                table: "Condition",
                keyColumn: "Id",
                keyValue: 5,
                column: "Code",
                value: "Закончился");

            migrationBuilder.UpdateData(
                table: "Condition",
                keyColumn: "Id",
                keyValue: 6,
                column: "Code",
                value: "Закупка");

            migrationBuilder.UpdateData(
                table: "Condition",
                keyColumn: "Id",
                keyValue: 7,
                column: "Code",
                value: "Активный");

            migrationBuilder.UpdateData(
                table: "Condition",
                keyColumn: "Id",
                keyValue: 8,
                column: "Code",
                value: "Новый");

            migrationBuilder.UpdateData(
                table: "Condition",
                keyColumn: "Id",
                keyValue: 9,
                column: "Code",
                value: "Готов");

            migrationBuilder.InsertData(
                table: "Condition",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[] { 10, "Архив", "Архив" });

            migrationBuilder.UpdateData(
                table: "TypeDoc",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "Поступление");

            migrationBuilder.UpdateData(
                table: "TypeDoc",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "Отгрузка");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Condition",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DropColumn(
                name: "Code",
                table: "TypeDoc");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Condition");
        }
    }
}
