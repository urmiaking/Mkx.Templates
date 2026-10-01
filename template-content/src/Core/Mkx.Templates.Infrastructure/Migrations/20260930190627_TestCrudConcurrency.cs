using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mkx.Templates.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TestCrudConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "Tests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Existing records need a usable initial version; Guid.Empty is never a valid write token.
            migrationBuilder.Sql("UPDATE [Tests] SET [Version] = NEWID() WHERE [Version] = '00000000-0000-0000-0000-000000000000'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Tests");
        }
    }
}
