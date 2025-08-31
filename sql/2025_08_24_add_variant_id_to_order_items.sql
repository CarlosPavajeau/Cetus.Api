ALTER TABLE order_items
    ADD COLUMN variant_id BIGINT;

UPDATE
    order_items oi
SET variant_id = pv.id
FROM product_variants pv
WHERE oi.product_id = pv.product_id;

ALTER TABLE order_items
    ALTER COLUMN variant_id SET NOT NULL;

ALTER TABLE order_items
    ADD CONSTRAINT fk_order_item_variant
        FOREIGN KEY (variant_id)
            REFERENCES product_variants (id)
            ON DELETE RESTRICT;

CREATE INDEX IF NOT EXISTS idx_order_items_variant_id ON order_items (variant_id);

ALTER TABLE order_items
    DROP COLUMN product_id;