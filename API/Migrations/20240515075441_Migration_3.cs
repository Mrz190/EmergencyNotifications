using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Migration_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationId",
                table: "ContactGroup",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotoficationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotoficationDescription = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroup_NotificationId",
                table: "ContactGroup",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactGroup_Notification_NotificationId",
                table: "ContactGroup",
                column: "NotificationId",
                principalTable: "Notification",
                principalColumn: "NotificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactGroup_Notification_NotificationId",
                table: "ContactGroup");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_ContactGroup_NotificationId",
                table: "ContactGroup");

            migrationBuilder.DropColumn(
                name: "NotificationId",
                table: "ContactGroup");
        }
    }
}
