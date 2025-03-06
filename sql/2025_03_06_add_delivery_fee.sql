CREATE TABLE states
(
    id         UUID PRIMARY KEY,
    name       VARCHAR(256) NOT NULL,
    created_at TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP
);

CREATE TABLE cities
(
    id         UUID PRIMARY KEY,
    name       VARCHAR(256) NOT NULL,
    state_id   UUID         NOT NULL,
    created_at TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP,
    FOREIGN KEY (state_id) REFERENCES states (id)
);

ALTER TABLE orders
    ADD COLUMN delivery_fee DECIMAL NOT NULL DEFAULT 0;

ALTER TABLE orders
    ADD COLUMN city_id UUID;

ALTER TABLE orders
    ADD FOREIGN KEY (city_id) REFERENCES cities (id);
