-- =============================================================================
-- Brings an existing database (for example on Neon) onto the new migration layout.
--
-- BACKGROUND
-- The old migration chain was broken: inside 20250117200906_mig_1 the CreateTable
-- calls for AspNetUsers and the other core tables had been commented out by hand,
-- presumably to apply it against an already existing database. As a result the
-- chain could never build a database from scratch. All migrations have been
-- squashed into a single InitialSchema migration.
--
-- This script aligns a database that already has the schema with the new layout:
--   1. Renames the columns that changed.
--   2. Creates the new indexes.
--   3. Marks the InitialSchema migration as applied, so EF Core does not try to
--      create the existing tables again.
--
-- USAGE
--   psql "<connection-string>" -f Scripts/migrate-existing-database.sql
--
-- Back the database up before running this. The script is idempotent and can be
-- run more than once.
-- =============================================================================

BEGIN;

-- -----------------------------------------------------------------------------
-- 1. Column renames
-- Entity fields that started with a lowercase letter were changed to PascalCase.
-- The API's JSON output is unaffected: ASP.NET Core already serialises in camelCase.
-- -----------------------------------------------------------------------------

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_name = 'AspNetUsers' AND column_name = 'status') THEN
        ALTER TABLE "AspNetUsers" RENAME COLUMN "status" TO "Status";
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_name = 'StandardRoutes' AND column_name = 'name') THEN
        ALTER TABLE "StandardRoutes" RENAME COLUMN "name" TO "Name";
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_name = 'StandardRoutes' AND column_name = 'status') THEN
        ALTER TABLE "StandardRoutes" RENAME COLUMN "status" TO "Status";
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_name = 'TravelDays' AND column_name = 'approxPrice') THEN
        ALTER TABLE "TravelDays" RENAME COLUMN "approxPrice" TO "ApproxPrice";
    END IF;
END $$;

-- -----------------------------------------------------------------------------
-- 2. New indexes
-- Frequently filtered columns had no index, so every query did a full scan.
-- -----------------------------------------------------------------------------

CREATE INDEX IF NOT EXISTS "IX_AllWorldCities_city"
    ON "AllWorldCities" ("city");

CREATE INDEX IF NOT EXISTS "IX_AllWorldCities_country"
    ON "AllWorldCities" ("country");

CREATE INDEX IF NOT EXISTS "IX_Currencies_AlphabeticCode"
    ON "Currencies" ("AlphabeticCode");

CREATE INDEX IF NOT EXISTS "IX_StandardRoutes_Status"
    ON "StandardRoutes" ("Status");

CREATE INDEX IF NOT EXISTS "IX_UserFriendships_AddresseeId_Status"
    ON "UserFriendships" ("AddresseeId", "Status");

CREATE INDEX IF NOT EXISTS "IX_UserFriendships_RequesterId_Status"
    ON "UserFriendships" ("RequesterId", "Status");

-- This index is unique. Creation fails if more than one friendship row exists
-- between the same two users; in that case the duplicates must be cleaned up
-- first. To find them:
--
--   SELECT "RequesterId", "AddresseeId", COUNT(*)
--   FROM "UserFriendships"
--   GROUP BY "RequesterId", "AddresseeId"
--   HAVING COUNT(*) > 1;
CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserFriendships_RequesterId_AddresseeId"
    ON "UserFriendships" ("RequesterId", "AddresseeId");

-- -----------------------------------------------------------------------------
-- 3. Rebaselining the migration history
-- The old migration rows are removed and replaced by the single InitialSchema row.
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

DELETE FROM "__EFMigrationsHistory";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260904165651_InitialSchema', '8.0.10');

COMMIT;

-- Verification: the query below should return exactly one row.
SELECT * FROM "__EFMigrationsHistory";
