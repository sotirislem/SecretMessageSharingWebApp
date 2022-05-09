using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecretsManagerWebApp.Migrations
{
    public partial class AddedCreatorClientInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorClientInfo",
                table: "SecretMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorClientInfo",
                table: "SecretMessages");
        }
    }
}
