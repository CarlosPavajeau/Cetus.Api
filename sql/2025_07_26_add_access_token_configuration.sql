ALTER TABLE stores
    ADD COLUMN mercado_pago_access_token TEXT;

ALTER TABLE stores
    ADD COLUMN mercado_pago_refresh_token TEXT;

ALTER TABLE stores
    ADD COLUMN mercado_pago_expires_at TIMESTAMP;
