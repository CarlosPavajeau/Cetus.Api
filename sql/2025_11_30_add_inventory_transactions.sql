CREATE TYPE inventory_transaction_type AS ENUM ('sale', 'adjustment', 'return', 'purchase', 'transfer');

CREATE TABLE IF NOT EXISTS inventory_transactions
(
    id           UUID PRIMARY KEY,
    variant_id   BIGINT                     NOT NULL,
    type         inventory_transaction_type NOT NULL,
    quantity     INTEGER                    NOT NULL,
    stock_after  INTEGER                    NOT NULL,
    reason       VARCHAR(255),
    reference_id VARCHAR(100),
    user_id      VARCHAR(100),
    created_at   TIMESTAMP                  NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT quantity_not_zero CHECK (quantity <> 0)
);

CREATE INDEX idx_inventory_transactions_variant_id ON inventory_transactions (variant_id);
CREATE INDEX idx_inventory_transactions_created_at ON inventory_transactions (created_at);
CREATE INDEX idx_inventory_transactions_reference_id ON inventory_transactions (reference_id);

ALTER TABLE inventory_transactions
    ADD CONSTRAINT fk_inventory_variant
        FOREIGN KEY (variant_id) REFERENCES product_variants (id)
            ON DELETE RESTRICT;