BEGIN;

ALTER TYPE order_status RENAME VALUE 'pending' TO 'pending_payment';
ALTER TYPE order_status RENAME VALUE 'paid' TO 'payment_confirmed';

ALTER TYPE order_status ADD VALUE 'processing' AFTER 'payment_confirmed';
ALTER TYPE order_status ADD VALUE 'ready_for_pickup' AFTER 'processing';
ALTER TYPE order_status ADD VALUE 'shipped' AFTER 'ready_for_pickup';
ALTER TYPE order_status ADD VALUE 'failed_delivery' AFTER 'delivered';
ALTER TYPE order_status ADD VALUE 'returned' AFTER 'canceled';

COMMIT;