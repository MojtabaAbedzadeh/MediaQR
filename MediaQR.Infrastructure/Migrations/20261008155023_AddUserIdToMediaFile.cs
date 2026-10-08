using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaQR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToMediaFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MediaFiles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MediaFiles");
        }
    }
}
