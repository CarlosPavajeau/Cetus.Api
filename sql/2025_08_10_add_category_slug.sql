ALTER TABLE categories
    ADD COLUMN slug VARCHAR(256) NOT NULL DEFAULT '';

UPDATE categories
SET slug = LOWER(
        REGEXP_REPLACE(name, '[^a-zA-Z0-9]', '-', 'g') ||
        '-' ||
        SUBSTRING(id::text, LENGTH(id::text) - 3, 4) ||
        '-' ||
        SUBSTRING(store_id::text, LENGTH(store_id::text) - 3, 4)
           )
WHERE slug = '';

CREATE UNIQUE INDEX idx_categories_slug ON categories (slug);

ALTER TABLE categories
    ALTER COLUMN slug DROP DEFAULT;
