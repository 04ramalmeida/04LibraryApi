using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _04LibraryApi.Migrations
{
    /// <inheritdoc />
    public partial class ReviewNullToEmpty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Review",
                table: "LibraryEntries",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(260)",
                oldMaxLength: 260,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Review",
                table: "LibraryEntries",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(260)",
                oldMaxLength: 260);
        }
    }
}
