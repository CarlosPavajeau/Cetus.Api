BEGIN;

ALTER TABLE orders
    ADD COLUMN subtotal DECIMAL NOT NULL DEFAULT 0,
ADD COLUMN discount DECIMAL NOT NULL DEFAULT 0;

UPDATE orders
SET subtotal = total - delivery_fee,
    discount = 0
WHERE subtotal = 0;

COMMIT;
