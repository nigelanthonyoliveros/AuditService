using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerformanceAuditing.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblAuditResults",
                columns: table => new
                {
                    AuditID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Reachable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblAuditResults", x => x.AuditID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblAuditResults");
        }
    }
}
