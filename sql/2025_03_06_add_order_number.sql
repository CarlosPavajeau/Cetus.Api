ALTER TABLE orders
    ADD COLUMN order_number SERIAL;

WITH ordered_rows AS (
    SELECT
        id,
        ROW_NUMBER() OVER (ORDER BY created_at ASC) AS row_num
    FROM orders
)
UPDATE orders
SET order_number = ordered_rows.row_num
    FROM ordered_rows
WHERE orders.id = ordered_rows.id;

ALTER TABLE orders
    ALTER COLUMN order_number SET NOT NULL;

DO $$
DECLARE
max_order_number INTEGER;
BEGIN
SELECT COALESCE(MAX(order_number), 0) INTO max_order_number FROM orders;
EXECUTE 'ALTER SEQUENCE orders_order_number_seq RESTART WITH ' || (max_order_number + 1);
END $$;
