BEGIN;

CREATE TYPE document_type AS ENUM ('CC', 'CE', 'NIT', 'PP', 'OTHER');
CREATE TYPE order_channel AS ENUM ('ecommerce', 'whatsapp', 'messenger', 'in_store', 'other');
CREATE TYPE payment_status AS ENUM ('pending', 'awaiting_verification', 'verified', 'rejected', 'refunded');

ALTER TYPE order_payment_method ADD VALUE IF NOT EXISTS 'nequi';

CREATE TABLE customers_new
(
    id              UUID      DEFAULT gen_random_uuid() PRIMARY KEY,
    document_type   document_type,
    document_number VARCHAR(20),
    name            VARCHAR(256)                        NOT NULL,
    email           VARCHAR(256),
    phone           VARCHAR(20)                         NOT NULL,
    address         VARCHAR(256),
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,


    CONSTRAINT customers_new_phone_unique UNIQUE (phone),
    CONSTRAINT customers_new_document_unique UNIQUE (document_type, document_number),
    CONSTRAINT customers_new_email_check CHECK (
        email IS NULL OR email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'
        ),

    CONSTRAINT customers_new_document_check CHECK (
        (document_type IS NULL AND document_number IS NULL) OR
        (document_type IS NOT NULL AND document_number IS NOT NULL)
        )
);

CREATE INDEX idx_customers_new_phone ON customers_new (phone);
CREATE INDEX idx_customers_new_document ON customers_new (document_type, document_number)
    WHERE document_number IS NOT NULL;
CREATE INDEX idx_customers_new_email ON customers_new (email)
    WHERE email IS NOT NULL;

INSERT INTO customers_new (id,
                           document_type,
                           document_number,
                           name,
                           email,
                           phone,
                           address,
                           created_at,
                           updated_at)
SELECT gen_random_uuid(),
       'CC'::document_type,
       c.id,
       c.name,
       c.email,
       c.phone,
       NULLIF(c.address, ''),
       c.created_at,
       c.updated_at
FROM customers c;

CREATE TEMP TABLE customer_id_mapping AS
SELECT c.id  AS old_id,
       cn.id AS new_id
FROM customers c
         JOIN customers_new cn ON c.id = cn.document_number;

DO
$$
    DECLARE
        old_count    INTEGER;
        mapped_count INTEGER;
    BEGIN
        SELECT COUNT(*) INTO old_count FROM customers;
        SELECT COUNT(*) INTO mapped_count FROM customer_id_mapping;

        IF old_count != mapped_count THEN
            RAISE EXCEPTION 'Error: No todos los customers fueron mapeados. Esperados: %, Mapeados: %', old_count, mapped_count;
        END IF;

        RAISE NOTICE '✓ Mapeo de customers completado: % registros', mapped_count;
    END
$$;

ALTER TABLE orders
    ADD COLUMN customer_id_new UUID;

UPDATE orders o
SET customer_id_new = m.new_id
FROM customer_id_mapping m
WHERE o.customer_id = m.old_id;

DO
$$
    DECLARE
        unmapped_count INTEGER;
    BEGIN
        SELECT COUNT(*)
        INTO unmapped_count
        FROM orders
        WHERE customer_id_new IS NULL;

        IF unmapped_count > 0 THEN
            RAISE EXCEPTION 'Error: % órdenes no tienen mapeo de customer_id', unmapped_count;
        END IF;

        RAISE NOTICE '✓ Migración de orders.customer_id completada';
    END
$$;

ALTER TABLE orders
    DROP CONSTRAINT IF EXISTS orders_customer_id_fkey;
ALTER TABLE orders
    DROP COLUMN customer_id;

ALTER TABLE orders
    RENAME COLUMN customer_id_new TO customer_id;

ALTER TABLE orders
    ALTER COLUMN customer_id SET NOT NULL;
ALTER TABLE orders
    ADD CONSTRAINT orders_customer_id_fkey
        FOREIGN KEY (customer_id) REFERENCES customers_new (id);

ALTER TABLE orders
    ADD COLUMN channel order_channel DEFAULT 'ecommerce' NOT NULL;
ALTER TABLE orders
    ADD COLUMN payment_status payment_status DEFAULT 'pending' NOT NULL;
ALTER TABLE orders
    ADD COLUMN payment_verified_at TIMESTAMP;
ALTER TABLE orders
    ADD COLUMN payment_verification_notes TEXT;
ALTER TABLE orders
    ALTER COLUMN city_id DROP NOT NULL;
ALTER TABLE orders
    ALTER COLUMN address DROP NOT NULL;

ALTER TABLE review_requests
    ADD COLUMN customer_id_new UUID;

UPDATE review_requests rr
SET customer_id_new = m.new_id
FROM customer_id_mapping m
WHERE rr.customer_id = m.old_id;

DO
$$
    DECLARE
        unmapped_count INTEGER;
        total_count    INTEGER;
    BEGIN
        SELECT COUNT(*) INTO total_count FROM review_requests;
        SELECT COUNT(*)
        INTO unmapped_count
        FROM review_requests
        WHERE customer_id_new IS NULL;

        IF unmapped_count > 0 THEN
            RAISE EXCEPTION 'Error: % review_requests no tienen mapeo de customer_id', unmapped_count;
        END IF;

        RAISE NOTICE '✓ Migración de review_requests.customer_id completada: % registros', total_count;
    END
$$;

ALTER TABLE review_requests
    DROP CONSTRAINT IF EXISTS review_requests_customer_id_fkey;
ALTER TABLE review_requests
    DROP COLUMN customer_id;
ALTER TABLE review_requests
    RENAME COLUMN customer_id_new TO customer_id;
ALTER TABLE review_requests
    ALTER COLUMN customer_id SET NOT NULL;
ALTER TABLE review_requests
    ADD CONSTRAINT review_requests_customer_id_fkey
        FOREIGN KEY (customer_id) REFERENCES customers_new (id);

ALTER TABLE coupon_usages
    ADD COLUMN customer_id_new UUID;

UPDATE coupon_usages cu
SET customer_id_new = m.new_id
FROM customer_id_mapping m
WHERE cu.customer_id = m.old_id;

DO
$$
    DECLARE
        unmapped_count INTEGER;
        total_count    INTEGER;
    BEGIN
        SELECT COUNT(*) INTO total_count FROM coupon_usages;
        SELECT COUNT(*)
        INTO unmapped_count
        FROM coupon_usages
        WHERE customer_id_new IS NULL;

        IF unmapped_count > 0 THEN
            RAISE EXCEPTION 'Error: % coupon_usages no tienen mapeo de customer_id', unmapped_count;
        END IF;

        RAISE NOTICE '✓ Migración de coupon_usages.customer_id completada: % registros', total_count;
    END
$$;

ALTER TABLE coupon_usages
    DROP CONSTRAINT IF EXISTS coupon_usages_customer_id_fkey;
ALTER TABLE coupon_usages
    DROP COLUMN customer_id;
ALTER TABLE coupon_usages
    RENAME COLUMN customer_id_new TO customer_id;
ALTER TABLE coupon_usages
    ALTER COLUMN customer_id SET NOT NULL;
ALTER TABLE coupon_usages
    ADD CONSTRAINT coupon_usages_customer_id_fkey
        FOREIGN KEY (customer_id) REFERENCES customers_new (id);

ALTER TABLE product_reviews
    ADD COLUMN customer_id_new UUID;

UPDATE product_reviews cu
SET customer_id_new = m.new_id
FROM customer_id_mapping m
WHERE cu.customer_id = m.old_id;

DO
$$
    DECLARE
        unmapped_count INTEGER;
        total_count    INTEGER;
    BEGIN
        SELECT COUNT(*) INTO total_count FROM product_reviews;
        SELECT COUNT(*)
        INTO unmapped_count
        FROM product_reviews
        WHERE customer_id_new IS NULL;

        IF unmapped_count > 0 THEN
            RAISE EXCEPTION 'Error: % product_reviews no tienen mapeo de customer_id', unmapped_count;
        END IF;

        RAISE NOTICE '✓ Migración de product_reviews.customer_id completada: % registros', total_count;
    END
$$;

ALTER TABLE product_reviews
    DROP CONSTRAINT IF EXISTS product_reviews_customer_id_fkey;
ALTER TABLE product_reviews
    DROP COLUMN customer_id;
ALTER TABLE product_reviews
    RENAME COLUMN customer_id_new TO customer_id;
ALTER TABLE product_reviews
    ALTER COLUMN customer_id SET NOT NULL;
ALTER TABLE product_reviews
    ADD CONSTRAINT product_reviews_customer_id_fkey
        FOREIGN KEY (customer_id) REFERENCES customers_new (id);

UPDATE orders
SET payment_status = CASE
                         WHEN status = 'pending_payment' THEN 'pending'::payment_status
                         WHEN status = 'canceled' THEN 'rejected'::payment_status
                         WHEN status = 'returned' THEN 'refunded'::payment_status
                         ELSE 'verified'::payment_status -- payment_confirmed, processing, shipped, delivered, etc.
    END;

UPDATE orders
SET payment_verified_at = updated_at
WHERE payment_status = 'verified';

DROP TABLE customers;
ALTER TABLE customers_new
    RENAME TO customers;

ALTER TABLE customers
    RENAME CONSTRAINT customers_new_phone_unique TO customers_phone_unique;
ALTER TABLE customers
    RENAME CONSTRAINT customers_new_document_unique TO customers_document_unique;
ALTER TABLE customers
    RENAME CONSTRAINT customers_new_email_check TO customers_email_check;
ALTER TABLE customers
    RENAME CONSTRAINT customers_new_document_check TO customers_document_check;

ALTER INDEX idx_customers_new_phone RENAME TO idx_customers_phone;
ALTER INDEX idx_customers_new_document RENAME TO idx_customers_document;
ALTER INDEX idx_customers_new_email RENAME TO idx_customers_email;

CREATE INDEX idx_orders_payment_pending ON orders (payment_status, created_at)
    WHERE payment_status IN ('pending', 'awaiting_verification');
CREATE INDEX idx_orders_channel ON orders (channel);
CREATE INDEX idx_orders_customer_id ON orders (customer_id);

CREATE INDEX idx_review_requests_customer_id ON review_requests (customer_id);
CREATE INDEX idx_coupon_usages_customer_id ON coupon_usages (customer_id);

DROP TRIGGER IF EXISTS update_customers_updated_at ON customers;
CREATE TRIGGER update_customers_updated_at
    BEFORE UPDATE
    ON customers
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

DO
$$
    DECLARE
        customers_count               INTEGER;
        orders_count                  INTEGER;
        orders_with_customer          INTEGER;
        review_requests_count         INTEGER;
        review_requests_with_customer INTEGER;
        coupon_usages_count           INTEGER;
        coupon_usages_with_customer   INTEGER;
    BEGIN
        SELECT COUNT(*) INTO customers_count FROM customers;
        SELECT COUNT(*) INTO orders_count FROM orders;
        SELECT COUNT(*) INTO review_requests_count FROM review_requests;
        SELECT COUNT(*) INTO coupon_usages_count FROM coupon_usages;

        SELECT COUNT(*)
        INTO orders_with_customer
        FROM orders o
                 JOIN customers c ON o.customer_id = c.id;

        SELECT COUNT(*)
        INTO review_requests_with_customer
        FROM review_requests rr
                 JOIN customers c ON rr.customer_id = c.id;

        SELECT COUNT(*)
        INTO coupon_usages_with_customer
        FROM coupon_usages cu
                 JOIN customers c ON cu.customer_id = c.id;

        RAISE NOTICE '';
        RAISE NOTICE '============================================';
        RAISE NOTICE '✓ MIGRACIÓN COMPLETADA EXITOSAMENTE';
        RAISE NOTICE '============================================';
        RAISE NOTICE '';
        RAISE NOTICE 'Resumen de datos migrados:';
        RAISE NOTICE '  - Customers:        %', customers_count;
        RAISE NOTICE '  - Orders:           % (con FK válida: %)', orders_count, orders_with_customer;
        RAISE NOTICE '  - Review Requests:  % (con FK válida: %)', review_requests_count, review_requests_with_customer;
        RAISE NOTICE '  - Coupon Usages:    % (con FK válida: %)', coupon_usages_count, coupon_usages_with_customer;
        RAISE NOTICE '';

        IF orders_count != orders_with_customer THEN
            RAISE EXCEPTION 'Error: Hay órdenes sin customer válido';
        END IF;

        IF review_requests_count != review_requests_with_customer THEN
            RAISE EXCEPTION 'Error: Hay review_requests sin customer válido';
        END IF;

        IF coupon_usages_count != coupon_usages_with_customer THEN
            RAISE EXCEPTION 'Error: Hay coupon_usages sin customer válido';
        END IF;

        RAISE NOTICE '✓ Todas las relaciones verificadas correctamente';
        RAISE NOTICE '============================================';
    END
$$;

COMMIT;