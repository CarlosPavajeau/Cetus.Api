ALTER TABLE products
    ADD COLUMN slug VARCHAR(256) NOT NULL DEFAULT '';

UPDATE products
SET slug = LOWER(
        REGEXP_REPLACE(name, '[^a-zA-Z0-9]', '-', 'g') ||
        '-' ||
        SUBSTRING(id::text, LENGTH(id::text) - 3, 4)
           );

CREATE UNIQUE INDEX idx_products_slug ON products (slug);

ALTER TABLE products
    ALTER COLUMN slug DROP DEFAULT; 
