-- Портал послуг Safina — міграція 018: поле "Специфікація" (файл .xls/.xlsx
-- на оголошенні, з переглядом вмісту таблиці постачальнику і споживачу),
-- реалізовано за прямим запитом користувача, 2026-09-14.
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_018_specification.sql

ALTER TABLE Services
    ADD COLUMN SpecificationFilePath VARCHAR(255) NULL;
