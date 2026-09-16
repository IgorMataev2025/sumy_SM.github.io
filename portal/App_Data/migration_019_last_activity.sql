-- Портал послуг Safina — міграція 019: моніторинг залогінених користувачів
-- у реальному часі (LastActivityAt оновлюється Global.asax на кожен
-- автентифікований запит, throttled), реалізовано за прямим запитом
-- користувача, 2026-09-16.
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_019_last_activity.sql

ALTER TABLE Users
    ADD COLUMN LastActivityAt DATETIME NULL;
