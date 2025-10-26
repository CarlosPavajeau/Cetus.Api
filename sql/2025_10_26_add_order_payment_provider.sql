CREATE TYPE order_payment_provider AS ENUM ('mercado_pago', 'wompi');

ALTER TABLE orders
    ADD COLUMN payment_provider order_payment_provider;

UPDATE orders
SET payment_provider = 'wompi'
WHERE transaction_id IS NOT NULL
  AND transaction_id LIKE '%-%';

UPDATE orders
SET payment_provider = 'mercado_pago'
WHERE transaction_id IS NOT NULL
  AND transaction_id NOT LIKE '%-%';