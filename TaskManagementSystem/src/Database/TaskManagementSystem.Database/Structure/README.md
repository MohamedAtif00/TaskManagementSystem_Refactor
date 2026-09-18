# Database structure

Per-table DDL lives under `Structure/{schema}/Tables/`. The numbered scripts in `Scripts/Migrations/` are what **DbUp** executes and include foreign keys/indexes for each schema.

When changing the schema:

1. Edit or add the table file under `Structure/`.
2. Add a new numbered migration script (or update the initial schema script during bootstrap only).
3. Run `DatabaseMigrator`.

Initial bootstrap runs migrations `000`–`010` and `012`–`018` (there is no `011` — test/dev seed data was moved out of the migration chain). Migration `018_outbox_inbox_Tables.sql` adds `{schema}.OutboxMessages` and `{schema}.InboxMessages` to all eight module schemas.

Integration test data lives in `Scripts/Seeds/002_IntegrationTestData.sql` and is applied separately via `DatabaseMigrator --seed`, `./scripts/seed-database.ps1`, or the integration-test bootstrap. See [`docs/DATABASE.md`](../../../../docs/DATABASE.md) for full setup and seed commands.
