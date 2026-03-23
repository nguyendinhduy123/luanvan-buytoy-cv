using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace buytoy.Migrations
{
    public partial class RatingsMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xoá index cũ (unique)
            migrationBuilder.DropIndex(
                name: "IX_Ratings_ProductId",
                table: "Ratings");

            // Tạo lại index nhưng KHÔNG unique
            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ProductId",
                table: "Ratings",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xoá index thường
            migrationBuilder.DropIndex(
                name: "IX_Ratings_ProductId",
                table: "Ratings");

            // Tạo lại index UNIQUE (nếu rollback)
            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ProductId",
                table: "Ratings",
                column: "ProductId",
                unique: true);
        }
    }
}
