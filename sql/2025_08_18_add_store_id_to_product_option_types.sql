ALTER TABLE product_option_types
    ADD COLUMN store_id uuid;

CREATE INDEX IF NOT EXISTS idx_product_option_types_store_id ON product_option_types (store_id);

ALTER TABLE product_option_types
    ALTER COLUMN store_id SET NOT NULL;
