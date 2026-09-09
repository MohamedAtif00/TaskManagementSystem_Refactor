# Database structure

Per-table DDL lives under `Structure/{schema}/Tables/`. The numbered scripts in `Scripts/Migrations/` are what **DbUp** executes and include foreign keys/indexes for each schema.

When changing the schema:

1. Edit or add the table file under `Structure/`.
2. Add a new numbered migration script (or update the initial schema script during bootstrap only).
3. Run `DatabaseMigrator`.

Initial bootstrap uses migrations `000`–`011` (including optional reference data in `010_seeds_ReferenceData.sql`). Additional seeds live under `Scripts/Seeds/` for manual use (`002_IntegrationTestData.sql` mirrors `011_identity_SeedUsers.sql`).
