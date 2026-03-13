using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourierTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CouriersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Couriers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VehicleType = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentLatitude = table.Column<double>(type: "REAL", nullable: true),
                    CurrentLongitude = table.Column<double>(type: "REAL", nullable: true),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rating = table.Column<decimal>(type: "TEXT", precision: 3, scale: 2, nullable: true),
                    TotalDeliveries = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Couriers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Couriers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Couriers_UserId",
                table: "Couriers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Couriers");
        }
    }
}
