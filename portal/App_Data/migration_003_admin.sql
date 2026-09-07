-- Портал послуг Safina — міграція 003: роль адміністратора (етап 4, ТЗ п.4.3).
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_003_admin.sql
-- (з cmd.exe через байтовий редірект `<` — НЕ через PowerShell pipe)
--
-- Адміністратор — окремий прапорець IsAdmin, а не значення UserType:
-- у ТЗ (розділ 2) "Адміністратор (власник порталу)" — не роль, яку обирають
-- при реєстрації (форма пропонує лише Споживач/Постачальник), а разова
-- ознака, яку власник хостингу вручну виставляє потрібному акаунту в БД.

ALTER TABLE Users
    ADD COLUMN IsAdmin BOOLEAN NOT NULL DEFAULT FALSE AFTER IsActive;
