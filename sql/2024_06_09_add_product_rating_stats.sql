ALTER TABLE products
    ADD COLUMN rating DECIMAL(3, 2) NOT NULL DEFAULT 0,
    ADD COLUMN reviews_count INTEGER NOT NULL DEFAULT 0;

CREATE INDEX idx_products_rating ON products (rating);

CREATE
OR REPLACE FUNCTION update_product_rating_stats()
RETURNS TRIGGER AS $$
BEGIN
UPDATE products
SET rating        = (SELECT COALESCE(AVG(rating), 0)
                     FROM product_reviews
                     WHERE product_id = NEW.product_id
                       AND status = 'approved'),
    reviews_count = (SELECT COUNT(*)
                     FROM product_reviews
                     WHERE product_id = NEW.product_id
                       AND status = 'approved')
WHERE id = NEW.product_id;

RETURN NEW;
END;
$$
LANGUAGE plpgsql;

CREATE TRIGGER update_product_rating_stats_on_review
    AFTER INSERT OR
UPDATE OF status
ON product_reviews
    FOR EACH ROW
    WHEN (NEW.status = 'approved')
    EXECUTE FUNCTION update_product_rating_stats(); 
