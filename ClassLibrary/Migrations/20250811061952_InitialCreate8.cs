using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassLibrary.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Client_ClientId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_Condition_ConditionId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_TypeDoc_TypeDocId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_resource_Resource_ResourceId",
                table: "Document_resource");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_resource_Unit_UnitId",
                table: "Document_resource");

            migrationBuilder.DropIndex(
                name: "IX_Document_resource_DocumentId",
                table: "Document_resource");

            migrationBuilder.CreateIndex(
                name: "IX_Document_resource_DocumentId_ResourceId_UnitId",
                table: "Document_resource",
                columns: new[] { "DocumentId", "ResourceId", "UnitId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Client_ClientId",
                table: "Document",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Condition_ConditionId",
                table: "Document",
                column: "ConditionId",
                principalTable: "Condition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_TypeDoc_TypeDocId",
                table: "Document",
                column: "TypeDocId",
                principalTable: "TypeDoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_resource_Resource_ResourceId",
                table: "Document_resource",
                column: "ResourceId",
                principalTable: "Resource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_resource_Unit_UnitId",
                table: "Document_resource",
                column: "UnitId",
                principalTable: "Unit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Client_ClientId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_Condition_ConditionId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_TypeDoc_TypeDocId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_resource_Resource_ResourceId",
                table: "Document_resource");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_resource_Unit_UnitId",
                table: "Document_resource");

            migrationBuilder.DropIndex(
                name: "IX_Document_resource_DocumentId_ResourceId_UnitId",
                table: "Document_resource");

            migrationBuilder.CreateIndex(
                name: "IX_Document_resource_DocumentId",
                table: "Document_resource",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Client_ClientId",
                table: "Document",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Condition_ConditionId",
                table: "Document",
                column: "ConditionId",
                principalTable: "Condition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_TypeDoc_TypeDocId",
                table: "Document",
                column: "TypeDocId",
                principalTable: "TypeDoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_resource_Resource_ResourceId",
                table: "Document_resource",
                column: "ResourceId",
                principalTable: "Resource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_resource_Unit_UnitId",
                table: "Document_resource",
                column: "UnitId",
                principalTable: "Unit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
