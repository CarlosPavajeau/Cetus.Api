CREATE TABLE IF NOT EXISTS order_timeline
(
    id            UUID PRIMARY KEY      DEFAULT gen_random_uuid(),
    order_id      UUID         NOT NULL,
    from_status   order_status,
    to_status     order_status NOT NULL,
    changed_by_id TEXT,
    notes         TEXT,
    created_at    TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_order_timeline_order
        FOREIGN KEY (order_id) REFERENCES orders (id) ON DELETE CASCADE
);

CREATE INDEX idx_order_timeline_order_id ON order_timeline (order_id);
CREATE INDEX idx_order_timeline_order_id_created_at ON order_timeline (order_id, created_at);