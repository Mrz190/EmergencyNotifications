using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Migration_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contact_ContactGroup_ContactGroupId",
                table: "Contact");

            migrationBuilder.DropTable(
                name: "ContactGroup");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Contact_ContactGroupId",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "ContactGroupId",
                table: "Contact");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Contact",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Contact");

            migrationBuilder.AddColumn<int>(
                name: "ContactGroupId",
                table: "Contact",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotoficationDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotoficationName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "ContactGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotificationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactGroup_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "NotificationId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contact_ContactGroupId",
                table: "Contact",
                column: "ContactGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroup_NotificationId",
                table: "ContactGroup",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contact_ContactGroup_ContactGroupId",
                table: "Contact",
                column: "ContactGroupId",
                principalTable: "ContactGroup",
                principalColumn: "Id");
        }
    }
}
