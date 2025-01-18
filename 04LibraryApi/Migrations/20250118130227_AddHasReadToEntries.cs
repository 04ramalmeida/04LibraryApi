using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _04LibraryApi.Migrations
{
    /// <inheritdoc />
    public partial class AddHasReadToEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasRead",
                table: "LibraryEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasRead",
                table: "LibraryEntries");
        }
    }
}
