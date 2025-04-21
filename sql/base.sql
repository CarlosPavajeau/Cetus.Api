-- Create States table
CREATE TABLE states
(
    id         UUID PRIMARY KEY,
    name       VARCHAR(256) NOT NULL,
    created_at TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP
);

-- Create Cities table
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

-- Create OrderStatus enum type
CREATE TYPE order_status AS ENUM ('pending', 'paid', 'delivered', 'canceled');

-- Create Categories table
CREATE TABLE categories
(
    id              UUID PRIMARY KEY,
    name            VARCHAR(256) NOT NULL,
    organization_id VARCHAR(64),
    created_at      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at      TIMESTAMP
);

-- Create Customers table
CREATE TABLE customers
(
    id         VARCHAR(50) PRIMARY KEY,
    name       VARCHAR(256) NOT NULL,
    email      VARCHAR(256) NOT NULL,
    phone      VARCHAR(256) NOT NULL,
    address    VARCHAR(256) NOT NULL,
    created_at TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT customers_email_check CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'
)
    );

-- Create Products table
CREATE TABLE products
(
    id              UUID PRIMARY KEY,
    name            VARCHAR(256) NOT NULL,
    description     VARCHAR(512),
    price           DECIMAL      NOT NULL DEFAULT 0,
    stock           INTEGER      NOT NULL DEFAULT 0,
    enabled         BOOLEAN      NOT NULL DEFAULT true,
    category_id     UUID         NOT NULL,
    image_url       VARCHAR(512),
    organization_id VARCHAR(64),
    created_at      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at      TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES categories (id)
);

-- Create Orders table
CREATE TABLE orders
(
    id              UUID PRIMARY KEY,
    status          order_status NOT NULL DEFAULT 'pending',
    address         VARCHAR(256) NOT NULL,
    total           DECIMAL      NOT NULL DEFAULT 0,
    customer_id     VARCHAR(50)  NOT NULL,
    transaction_id  VARCHAR(256),
    delivery_fee    DECIMAL      NOT NULL DEFAULT 0,
    city_id         UUID,
    order_number    SERIAL       NOT NULL,
    organization_id VARCHAR(64),
    created_at      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (customer_id) REFERENCES customers (id),
    FOREIGN KEY (city_id) REFERENCES cities (id)
);

-- Create OrderItems table
CREATE TABLE order_items
(
    id           UUID PRIMARY KEY,
    product_name VARCHAR(256) NOT NULL,
    quantity     INTEGER      NOT NULL DEFAULT 0,
    price        DECIMAL      NOT NULL DEFAULT 0,
    image_url    VARCHAR(512),
    product_id   UUID         NOT NULL,
    order_id     UUID         NOT NULL,
    FOREIGN KEY (product_id) REFERENCES products (id),
    FOREIGN KEY (order_id) REFERENCES orders (id)
);

-- Create indexes for better performance
CREATE INDEX idx_products_category ON products (category_id);
CREATE INDEX idx_orders_customer ON orders (customer_id);
CREATE INDEX idx_order_items_order ON order_items (order_id);
CREATE INDEX idx_order_items_product ON order_items (product_id);
CREATE INDEX idx_products_deleted_at ON products (deleted_at) WHERE deleted_at IS NOT NULL;
CREATE INDEX idx_categories_deleted_at ON categories (deleted_at) WHERE deleted_at IS NOT NULL;
CREATE INDEX idx_categories_organization ON categories (organization_id);
CREATE INDEX idx_products_organization ON products (organization_id);
CREATE INDEX idx_orders_organization ON orders (organization_id);

-- Create function to update updated_at timestamp
CREATE
OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at
= CURRENT_TIMESTAMP;
RETURN NEW;
END;
$$
language 'plpgsql';

-- Create triggers for updating updated_at
CREATE TRIGGER update_categories_updated_at
    BEFORE UPDATE
    ON categories
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_customers_updated_at
    BEFORE UPDATE
    ON customers
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_products_updated_at
    BEFORE UPDATE
    ON products
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_orders_updated_at
    BEFORE UPDATE
    ON orders
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Create triggers for updating updated_at on new tables
CREATE TRIGGER update_states_updated_at
    BEFORE UPDATE
    ON states
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_cities_updated_at
    BEFORE UPDATE
    ON cities
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
