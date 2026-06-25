using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TmsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToStudent : Migration
    {
    
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Students");
        }
    }
}
