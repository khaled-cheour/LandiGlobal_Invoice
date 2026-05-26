using LandiGlobalTemplate.Data;
using Microsoft.EntityFrameworkCore;

namespace LandiGlobalTemplate.Services
{
    public static class ProductCatalogDatabaseInitializer
    {
        public static async Task EnsureReadyAsync(ApplicationDbContext db)
        {
            await db.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID(N'[dbo].[ProductCatalogItems]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ProductCatalogItems] (
                        [Id] int NOT NULL IDENTITY,
                        [ProductNumber] nvarchar(50) NOT NULL,
                        [FamilyName] nvarchar(120) NOT NULL,
                        [Platform] nvarchar(120) NOT NULL,
                        [Model] nvarchar(max) NOT NULL,
                        [MainDisplay] nvarchar(50) NOT NULL,
                        [SecondDisplay] nvarchar(50) NOT NULL,
                        [PaymentType] nvarchar(80) NOT NULL,
                        [MemoryPlan] nvarchar(80) NOT NULL,
                        [G4] nvarchar(40) NOT NULL,
                        [GMS] nvarchar(40) NOT NULL,
                        [HSCode] nvarchar(20) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_ProductCatalogItems] PRIMARY KEY ([Id])
                    );

                    CREATE UNIQUE INDEX [IX_ProductCatalogItems_ProductNumber]
                    ON [dbo].[ProductCatalogItems] ([ProductNumber]);
                END

                IF COL_LENGTH(N'[dbo].[ProductCatalogItems]', N'HSCode') IS NOT NULL
                   AND EXISTS (
                        SELECT 1
                        FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'[dbo].[ProductCatalogItems]')
                          AND name = N'HSCode'
                          AND is_nullable = 0
                   )
                BEGIN
                    ALTER TABLE [dbo].[ProductCatalogItems]
                    ALTER COLUMN [HSCode] nvarchar(20) NULL;
                END

                IF NOT EXISTS (
                    SELECT 1
                    FROM [dbo].[ProductCatalogItems]
                    WHERE [ProductNumber] = N'WX01000012'
                )
                BEGIN
                    INSERT INTO [dbo].[ProductCatalogItems]
                        ([ProductNumber], [FamilyName], [Platform], [Model], [MainDisplay], [SecondDisplay],
                         [PaymentType], [MemoryPlan], [G4], [GMS], [HSCode], [CreatedAt])
                    VALUES
                        (N'WX01000012', N'Accessory', N'Accessory', N'ECRPowerCordUK', N'-', N'-',
                         N'-', N'-', N'-', N'-', NULL, SYSUTCDATETIME());
                END

                UPDATE [dbo].[ProductCatalogItems]
                SET [HSCode] = NULL
                WHERE [ProductNumber] = N'WX01000012';

                IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
                   AND NOT EXISTS (
                        SELECT 1
                        FROM [dbo].[__EFMigrationsHistory]
                        WHERE [MigrationId] = N'20260526112000_AddProductCatalogItems'
                   )
                BEGIN
                    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES (N'20260526112000_AddProductCatalogItems', N'10.0.0');
                END
                """);
        }
    }
}
