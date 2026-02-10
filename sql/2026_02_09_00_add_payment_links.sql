CREATE TYPE payment_link_status AS ENUM ('active', 'paid', 'expired');

CREATE TABLE payment_links
(
    id         UUID PRIMARY KEY     DEFAULT gen_random_uuid(),
    order_id   UUID        NOT NULL,
    token      VARCHAR(64) NOT NULL UNIQUE,
    status     payment_link_status  DEFAULT 'active' NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (order_id) REFERENCES orders (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX idx_payment_links_order_active
    ON payment_links (order_id, status)
    WHERE status = 'active';