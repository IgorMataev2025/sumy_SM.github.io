-- Портал послуг Safina — міграція 011: відповідь постачальника на відгук (п.19,
-- наступна фіча понад MVP після статистики постачальника, постановка робочої
-- тестової версії, 2026-09-12). Одна відповідь на відгук (без гілок/треду) —
-- зберігається просто на самому рядку Reviews, а не в окремій таблиці.
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_011_review_reply.sql

ALTER TABLE Reviews
    ADD COLUMN ProviderReply VARCHAR(1000) NULL,
    ADD COLUMN ProviderReplyAt DATETIME NULL;
