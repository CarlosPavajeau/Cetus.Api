CREATE TRIGGER update_coupon_usage_count_insert
    AFTER INSERT
    ON coupon_usages
    FOR EACH ROW
    EXECUTE FUNCTION update_coupon_usage_count();

CREATE TRIGGER update_coupon_usage_count_delete
    AFTER DELETE
    ON coupon_usages
    FOR EACH ROW
    EXECUTE FUNCTION update_coupon_usage_count();
