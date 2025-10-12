ALTER TABLE orders
    ADD COLUMN cancellation_reason TEXT      NULL,
    ADD COLUMN cancelled_at        TIMESTAMP NULL;