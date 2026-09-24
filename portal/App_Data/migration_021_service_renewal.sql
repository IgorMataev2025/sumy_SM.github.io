-- Портал послуг Safina — міграція 021: попередження про автозняття + «Продовжити»
-- (2026-09-24, роль Постачальник). Строк публікації тепер рахується від
-- COALESCE(RenewedAt, ApprovedAt) — окреме поле, а не перезапис ApprovedAt, щоб
-- продовжене оголошення не ставало «новим» для дайджесту/sitemap.
-- ExpiryWarnedAt — лист-попередження вже надіслано в поточному циклі (скидається
-- при продовженні й повторному схваленні).
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_021_service_renewal.sql

ALTER TABLE Services
    ADD COLUMN RenewedAt DATETIME NULL,
    ADD COLUMN ExpiryWarnedAt DATETIME NULL;
