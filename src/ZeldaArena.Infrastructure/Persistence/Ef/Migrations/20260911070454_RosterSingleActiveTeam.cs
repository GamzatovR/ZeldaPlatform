using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZeldaArena.Infrastructure.Persistence.Ef.Migrations;

/// <summary>
/// Игрок не может состоять в двух командах одновременно: частичный уникальный
/// индекс по открытым записям состава.
/// </summary>
public partial class RosterSingleActiveTeam : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_RosterEntries_PlayerId_Active",
            table: "RosterEntries",
            column: "PlayerId",
            unique: true,
            filter: "\"LeftAt\" IS NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_RosterEntries_PlayerId_Active",
            table: "RosterEntries");
    }
}