CREATE TYPE order_status_new AS ENUM ('pending', 'paid', 'delivered', 'canceled');

ALTER TABLE orders ALTER COLUMN status DROP DEFAULT;

ALTER TABLE orders
ALTER COLUMN status TYPE order_status_new 
  USING (
    CASE status::text
      WHEN 'PENDING' THEN 'pending'::order_status_new
      WHEN 'PAID' THEN 'paid'::order_status_new
      WHEN 'DELIVERED' THEN 'delivered'::order_status_new
      WHEN 'CANCELED' THEN 'canceled'::order_status_new
    END
  );

ALTER TABLE orders ALTER COLUMN status SET DEFAULT 'pending'::order_status_new;

DROP TYPE order_status;

ALTER TYPE order_status_new RENAME TO order_status;
