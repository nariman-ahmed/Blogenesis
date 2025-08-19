using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogenesis.Data.Migrations
{
    /// <inheritdoc />
    public partial class updated_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "AspNetUsers",
                newName: "ProfilePicUrl");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ProfilePicUrl",
                table: "AspNetUsers",
                newName: "DisplayName");
        }
    }
}
