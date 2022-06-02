using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecretMessageSharingWebApp.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GetLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestCreatorIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestClientInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecretMessageId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SecretMessageExisted = table.Column<bool>(type: "bit", nullable: false),
                    SecretMessageCreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecretMessageCreatorIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecretMessageCreatorClientInfo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecretMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeleteOnRetrieve = table.Column<bool>(type: "bit", nullable: false),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatorClientInfo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecretMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GetLogs_SecretMessageId",
                table: "GetLogs",
                column: "SecretMessageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GetLogs");

            migrationBuilder.DropTable(
                name: "SecretMessages");
        }
    }
}
