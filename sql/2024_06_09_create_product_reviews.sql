DROP TYPE IF EXISTS product_review_status;
CREATE TYPE product_review_status AS ENUM ('pending_approval', 'approved', 'rejected');

CREATE TABLE product_reviews
(
    id                UUID PRIMARY KEY,
    comment           VARCHAR(1024)         NOT NULL,
    rating            SMALLINT              NOT NULL CHECK (rating >= 1 AND rating <= 5),
    is_verified       BOOLEAN               NOT NULL DEFAULT false,
    status            product_review_status NOT NULL DEFAULT 'pending_approval',
    moderator_notes   VARCHAR(1024),
    review_request_id UUID                  NOT NULL,
    product_id        UUID                  NOT NULL,
    customer_id       VARCHAR(50)           NOT NULL,
    created_at        TIMESTAMP             NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at        TIMESTAMP             NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (review_request_id) REFERENCES review_requests (id),
    FOREIGN KEY (product_id) REFERENCES products (id),
    FOREIGN KEY (customer_id) REFERENCES customers (id)
);

CREATE INDEX idx_product_reviews_review_request ON product_reviews (review_request_id);
CREATE INDEX idx_product_reviews_product ON product_reviews (product_id);
CREATE INDEX idx_product_reviews_customer ON product_reviews (customer_id);
CREATE INDEX idx_product_reviews_status ON product_reviews (status);

CREATE TRIGGER update_product_reviews_updated_at
    BEFORE UPDATE
    ON product_reviews
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column(); 
