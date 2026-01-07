CREATE TYPE order_payment_method AS ENUM (
    'cash',
    'credit_card',
    'pse',
    'cash_reference',
    'cash_on_delivery',
    'bank_transfer'
    );

ALTER TABLE orders
    ADD COLUMN payment_method order_payment_method;

UPDATE orders
SET payment_method = 'credit_card'
WHERE payment_provider IN ('mercado_pago', 'wompi');