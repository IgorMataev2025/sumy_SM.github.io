-- Портал послуг Safina — міграція 002: поля для реєстрації/авторизації (етап 2).
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_002_auth.sql
-- (з cmd.exe через байтовий редірект `<` — НЕ через PowerShell pipe, інакше кирилиця б'ється)
--
-- Примітка: "ADD COLUMN IF NOT EXISTS" на MySQL 8.4.9 (перевірено) дає
-- синтаксичну помилку — тому міграція звичайна, розрахована на одноразовий
-- запуск (стандартна практика для migration-скриптів).

ALTER TABLE Users
    ADD COLUMN EmailConfirmed        BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN EmailConfirmationToken VARCHAR(100) NULL,
    ADD COLUMN PasswordResetToken     VARCHAR(100) NULL,
    ADD COLUMN PasswordResetExpires   DATETIME NULL;

ALTER TABLE Users
    ADD INDEX IX_Users_EmailConfirmationToken (EmailConfirmationToken),
    ADD INDEX IX_Users_PasswordResetToken (PasswordResetToken);
