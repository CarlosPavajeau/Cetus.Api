DROP TYPE IF EXISTS review_request_status;
CREATE TYPE review_request_status AS ENUM ('pending', 'sent', 'completed', 'expired');

CREATE TABLE review_requests
(
    id            UUID PRIMARY KEY,
    status        review_request_status NOT NULL DEFAULT 'pending',
    token         VARCHAR(256)          NOT NULL UNIQUE,
    order_item_id UUID                  NOT NULL,
    customer_id   VARCHAR(50)           NOT NULL,
    send_at       TIMESTAMP             NOT NULL,
    created_at    TIMESTAMP             NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP             NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (order_item_id) REFERENCES order_items (id),
    FOREIGN KEY (customer_id) REFERENCES customers (id)
);

CREATE INDEX idx_review_requests_order_item ON review_requests (order_item_id);
CREATE INDEX idx_review_requests_customer ON review_requests (customer_id);
CREATE INDEX idx_review_requests_status ON review_requests (status);
CREATE INDEX idx_review_requests_token ON review_requests (token);

CREATE TRIGGER update_review_requests_updated_at
    BEFORE UPDATE
    ON review_requests
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column(); 
