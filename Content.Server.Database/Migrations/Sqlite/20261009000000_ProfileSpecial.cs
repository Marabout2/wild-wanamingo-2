using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    // Misfits Change - character SPECIAL stats on the profile (5 = average for existing characters)
    /// <inheritdoc />
    public partial class ProfileSpecial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "special_agility",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "special_charisma",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "special_endurance",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "special_intelligence",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "special_luck",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "special_perception",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "special_strength",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "special_agility",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "special_charisma",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "special_endurance",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "special_intelligence",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "special_luck",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "special_perception",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "special_strength",
                table: "profile");
        }
    }
}
