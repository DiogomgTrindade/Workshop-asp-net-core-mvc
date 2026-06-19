using Microsoft.EntityFrameworkCore.Migrations;

namespace SalesWebMvc.Migrations
{
    public partial class SalesRecordIdAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemCart_SalesRecord_SalesRecordId",
                table: "ItemCart");

            migrationBuilder.AlterColumn<int>(
                name: "SalesRecordId",
                table: "ItemCart",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCart_SalesRecord_SalesRecordId",
                table: "ItemCart",
                column: "SalesRecordId",
                principalTable: "SalesRecord",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemCart_SalesRecord_SalesRecordId",
                table: "ItemCart");

            migrationBuilder.AlterColumn<int>(
                name: "SalesRecordId",
                table: "ItemCart",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCart_SalesRecord_SalesRecordId",
                table: "ItemCart",
                column: "SalesRecordId",
                principalTable: "SalesRecord",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
