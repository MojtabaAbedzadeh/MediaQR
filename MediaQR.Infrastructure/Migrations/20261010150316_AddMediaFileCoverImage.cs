using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaQR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaFileCoverImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CoverImageId",
                table: "MediaFiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_CoverImageId",
                table: "MediaFiles",
                column: "CoverImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_MediaFiles_CoverImageId",
                table: "MediaFiles",
                column: "CoverImageId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_MediaFiles_CoverImageId",
                table: "MediaFiles");

            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_CoverImageId",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "CoverImageId",
                table: "MediaFiles");
        }
    }
}
