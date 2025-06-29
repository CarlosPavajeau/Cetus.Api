CREATE TABLE stores
(
    id            UUID PRIMARY KEY,
    external_id   VARCHAR(50)  NOT NULL UNIQUE,
    name          VARCHAR(255) NOT NULL,
    description   TEXT NULL,
    slug          VARCHAR(255) NOT NULL UNIQUE,
    custom_domain VARCHAR(255) NULL,
    is_active     BOOLEAN      NOT NULL DEFAULT true,
    logo_url      TEXT NULL,
    address       VARCHAR(255) NULL,
    phone         VARCHAR(50) NULL,
    email         VARCHAR(255) NULL,
    created_at    TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at    TIMESTAMP NULL
);

CREATE UNIQUE INDEX idx_stores_custom_domain ON stores (custom_domain) WHERE custom_domain IS NOT NULL;

CREATE TRIGGER update_stores_updated_at
    BEFORE UPDATE
    ON stores
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
