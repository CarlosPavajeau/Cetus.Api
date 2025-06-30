BEGIN;

ALTER TABLE categories
DROP
COLUMN organization_id;

ALTER TABLE products
DROP
COLUMN organization_id;

ALTER TABLE orders
DROP
COLUMN organization_id;

ALTER TABLE delivery_fees
DROP
COLUMN organization_id;

ALTER TABLE categories
    ADD COLUMN store_id UUID;

ALTER TABLE products
    ADD COLUMN store_id UUID;

ALTER TABLE orders
    ADD COLUMN store_id UUID;

ALTER TABLE delivery_fees
    ADD COLUMN store_id UUID;

COMMIT;
