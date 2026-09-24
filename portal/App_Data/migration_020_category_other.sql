-- Портал послуг Safina — міграція 020: категорія «Інше» для послуг, що не
-- підпадають під жодну з наявних категорій (рішення користувача 2026-09-24:
-- поки що лише категорія, без поля «запропонувати свою» — звичайна модерація).
-- Ідемпотентна (той самий прийом, що початкове наповнення в schema.sql).
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_020_category_other.sql

INSERT INTO Categories (Name, Description, ParentId, IsActive)
SELECT * FROM (SELECT
    'Інше' AS Name,
    'Послуги, що не підпадають під жодну з наведених категорій' AS Description,
    NULL AS ParentId, TRUE AS IsActive) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Інше');
