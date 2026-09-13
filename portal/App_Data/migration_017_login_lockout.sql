-- Портал послуг Safina — міграція 017: захист від перебору паролів
-- (наступна фіча понад MVP, обрано автономно циклом /loop, 2026-09-13).
-- Портал тепер повністю публічний (відкритий перегляд для USER, п.12) —
-- Login.aspx досі не мав жодного обмеження на кількість спроб входу.
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_017_login_lockout.sql

ALTER TABLE Users
    ADD COLUMN FailedLoginAttempts INT NOT NULL DEFAULT 0,
    ADD COLUMN LockedUntil DATETIME NULL;
