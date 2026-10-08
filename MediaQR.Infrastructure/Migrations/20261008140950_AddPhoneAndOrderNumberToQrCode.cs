using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaQR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneAndOrderNumberToQrCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "QrCodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "QrCodes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "QrCodes");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "QrCodes");
        }
    }
}
