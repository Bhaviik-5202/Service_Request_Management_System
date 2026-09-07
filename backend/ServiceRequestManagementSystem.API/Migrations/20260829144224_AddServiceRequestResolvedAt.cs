using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceRequestManagementSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceRequestResolvedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ServiceRequests', N'ResolvedAt') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[ServiceRequests]
                    ADD [ResolvedAt] datetime2 NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ResolvedAt is part of the current InitialCreate model, so it must
            // remain if this synchronisation migration is rolled back.
        }
    }
}
