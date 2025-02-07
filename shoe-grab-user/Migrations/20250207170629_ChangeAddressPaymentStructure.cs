using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoeGrabUserManagement.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAddressPaymentStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Profiles",
                newName: "Address_Street");

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "Profiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "Profiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_PostalCode",
                table: "Profiles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Address_PostalCode",
                table: "Profiles");

            migrationBuilder.RenameColumn(
                name: "Address_Street",
                table: "Profiles",
                newName: "Address");
        }
    }
}
