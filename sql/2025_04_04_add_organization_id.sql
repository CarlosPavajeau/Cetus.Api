ALTER TABLE categories
    ADD COLUMN organization_id VARCHAR(64);

ALTER TABLE products
    ADD COLUMN organization_id VARCHAR(64);

ALTER TABLE orders
    ADD COLUMN organization_id VARCHAR(64);

CREATE INDEX idx_categories_organization ON categories (organization_id);
CREATE INDEX idx_products_organization ON products (organization_id);
CREATE INDEX idx_orders_organization ON orders (organization_id);
