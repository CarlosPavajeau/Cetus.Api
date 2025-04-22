CREATE TABLE delivery_fee
(
    id              UUID PRIMARY KEY,
    fee             DECIMAL   NOT NULL DEFAULT 0,
    city_id         UUID      NOT NULL,
    organization_id VARCHAR(64),
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at      TIMESTAMP,
    FOREIGN KEY (city_id) REFERENCES cities (id)
);

-- Create index for better performance
CREATE INDEX idx_delivery_fee_city ON delivery_fee (city_id);
CREATE INDEX idx_delivery_fee_organization ON delivery_fee (organization_id);
