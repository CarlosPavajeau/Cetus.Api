ALTER TABLE products
    ADD COLUMN sales_count INTEGER NOT NULL DEFAULT 0;

CREATE INDEX idx_products_sales_count ON products (sales_count);