using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Webdienlanh.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianToBookingOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WalletBalance",
                table: "Technicians",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "BookingOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "BookingOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingOrders_TechnicianId",
                table: "BookingOrders",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingOrders_Technicians_TechnicianId",
                table: "BookingOrders",
                column: "TechnicianId",
                principalTable: "Technicians",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingOrders_Technicians_TechnicianId",
                table: "BookingOrders");

            migrationBuilder.DropIndex(
                name: "IX_BookingOrders_TechnicianId",
                table: "BookingOrders");

            migrationBuilder.DropColumn(
                name: "WalletBalance",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "BookingOrders");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "BookingOrders");
        }
    }
}
