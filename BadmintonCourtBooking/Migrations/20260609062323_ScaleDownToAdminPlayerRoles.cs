using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonCourtBooking.Migrations
{
    /// <inheritdoc />
    public partial class ScaleDownToAdminPlayerRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Users] SET [Role] = 'Player' WHERE [Role] = 'Owner'");
            migrationBuilder.Sql("UPDATE [Venues] SET [Status] = 'Approved' WHERE [Status] = 'PendingApproval'");

            migrationBuilder.DropForeignKey(
                name: "FK_Venues_Users_OwnerUserId",
                table: "Venues");

            migrationBuilder.DropTable(
                name: "UserSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_Venues_OwnerUserId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Venues");

            migrationBuilder.RenameColumn(
                name: "OwnerPhone",
                table: "Venues",
                newName: "ContactPhone");

            migrationBuilder.RenameColumn(
                name: "OwnerName",
                table: "Venues",
                newName: "ContactName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContactPhone",
                table: "Venues",
                newName: "OwnerPhone");

            migrationBuilder.RenameColumn(
                name: "ContactName",
                table: "Venues",
                newName: "OwnerName");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Venues",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserSnapshots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    RoleLabel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Venues_OwnerUserId",
                table: "Venues",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Users_OwnerUserId",
                table: "Venues",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
