using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonCourtBooking.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationAndUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Venues",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlayerUserId",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NormalizedPhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlayArea = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPhoneVerified = table.Column<bool>(type: "bit", nullable: false),
                    ReceiveBookingConfirm = table.Column<bool>(type: "bit", nullable: false),
                    ReceivePlayReminder = table.Column<bool>(type: "bit", nullable: false),
                    ReceivePromo = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSignInAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Venues_OwnerUserId",
                table: "Venues",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PlayerUserId",
                table: "Bookings",
                column: "PlayerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedPhoneNumber",
                table: "Users",
                column: "NormalizedPhoneNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_PlayerUserId",
                table: "Bookings",
                column: "PlayerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Users_OwnerUserId",
                table: "Venues",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_PlayerUserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Venues_Users_OwnerUserId",
                table: "Venues");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Venues_OwnerUserId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PlayerUserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "PlayerUserId",
                table: "Bookings");
        }
    }
}
