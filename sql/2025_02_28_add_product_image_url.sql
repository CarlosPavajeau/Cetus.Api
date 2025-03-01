ALTER TABLE products
    ADD COLUMN image_url VARCHAR(512);

ALTER TABLE order_items
    ADD COLUMN image_url VARCHAR(512);
