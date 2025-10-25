CREATE INDEX idx_product_variants_lookup ON product_variants (id, deleted_at, stock);
CREATE INDEX idx_delivery_feeds_lookup ON delivery_fees (city_id, store_id, deleted_at);
CREATE INDEX idx_products_store_deleted ON products (store_id, deleted_at);