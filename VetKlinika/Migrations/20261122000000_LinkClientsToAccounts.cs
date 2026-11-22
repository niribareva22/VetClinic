using Microsoft.EntityFrameworkCore.Migrations;

namespace VetKlinika.Migrations;

public partial class LinkClientsToAccounts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "IdentityUserId",
            table: "Clients",
            type: "text",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Clients_IdentityUserId",
            table: "Clients",
            column: "IdentityUserId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Clients_AspNetUsers_IdentityUserId",
            table: "Clients",
            column: "IdentityUserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Clients_AspNetUsers_IdentityUserId",
            table: "Clients");

        migrationBuilder.DropIndex(
            name: "IX_Clients_IdentityUserId",
            table: "Clients");

        migrationBuilder.DropColumn(
            name: "IdentityUserId",
            table: "Clients");
    }
}
