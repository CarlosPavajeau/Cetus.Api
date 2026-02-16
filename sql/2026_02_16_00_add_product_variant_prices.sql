ALTER TABLE product_variants
    ADD COLUMN cost_price       NUMERIC(12, 2) NULL,
    ADD COLUMN compare_at_price NUMERIC(12, 2) NULL;

ALTER TABLE product_variants
    ADD CONSTRAINT chk_variant_compare_at_price_nonnegative CHECK (compare_at_price >= 0),
    ADD CONSTRAINT chk_variant_cost_price_nonnegative CHECK (cost_price >= 0),
    ADD CONSTRAINT chk_variant_compare_at_price_greater_than_price CHECK (compare_at_price >= price);