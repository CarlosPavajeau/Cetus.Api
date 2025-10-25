ALTER TABLE stores
    ADD COLUMN wompi_public_key    TEXT,
    ADD COLUMN wompi_private_key   TEXT,
    ADD COLUMN wompi_events_key    TEXT,
    ADD COLUMN wompi_integrity_key TEXT;