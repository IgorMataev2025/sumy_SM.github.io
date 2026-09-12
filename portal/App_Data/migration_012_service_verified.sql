-- Портал послуг Safina — міграція 012: позначка "Перевірено адміном" (п.22,
-- наступна фіча понад MVP після базового SEO, постановка робочої тестової версії,
-- 2026-09-12). Не замінює звичайну модерацію (Status) — додатковий сигнал довіри,
-- який адмін виставляє на власний розсуд (напр. підтвердив особу/ФОП постачальника
-- поза системою), не автоматична перевірка.
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_012_service_verified.sql

ALTER TABLE Services
    ADD COLUMN IsVerified TINYINT(1) NOT NULL DEFAULT 0;
