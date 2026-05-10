using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20260510170000_AddPublicationUrgentFlag")]
    public partial class AddPublicationUrgentFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Publications', 'IsUrgentRatingRequested') IS NULL
                BEGIN
                    ALTER TABLE [Publications]
                    ADD [IsUrgentRatingRequested] bit NOT NULL
                    CONSTRAINT [DF_Publications_IsUrgentRatingRequested] DEFAULT CAST(0 AS bit);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = 'IX_Publications_IsUrgentRatingRequested'
                      AND [object_id] = OBJECT_ID('dbo.Publications')
                )
                BEGIN
                    CREATE INDEX [IX_Publications_IsUrgentRatingRequested]
                    ON [Publications] ([IsUrgentRatingRequested]);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = 'IX_Publications_IsUrgentRatingRequested'
                      AND [object_id] = OBJECT_ID('dbo.Publications')
                )
                BEGIN
                    DROP INDEX [IX_Publications_IsUrgentRatingRequested] ON [Publications];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Publications', 'IsUrgentRatingRequested') IS NOT NULL
                BEGIN
                    DECLARE @constraintName nvarchar(128);

                    SELECT @constraintName = dc.[name]
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c
                        ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('dbo.Publications')
                      AND c.[name] = 'IsUrgentRatingRequested';

                    IF @constraintName IS NOT NULL
                    BEGIN
                        EXEC('ALTER TABLE [Publications] DROP CONSTRAINT [' + @constraintName + ']');
                    END

                    ALTER TABLE [Publications] DROP COLUMN [IsUrgentRatingRequested];
                END
                """);
        }
    }
}
