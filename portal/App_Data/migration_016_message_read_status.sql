-- Портал послуг Safina — міграція 016: лічильник непрочитаних повідомлень
-- (наступна фіча понад MVP, обрано автономно циклом /loop, 2026-09-13).
-- Розмова = та сама пара (ServiceId, ConsumerId), що Messages; UserId —
-- один із двох учасників (сам ConsumerId або постачальник оголошення) —
-- кожен веде свою власну позицію "прочитано до" в одній і тій самій
-- розмові незалежно від іншої сторони.
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_016_message_read_status.sql

CREATE TABLE IF NOT EXISTS MessageReadStatus (
    ServiceId           INT NOT NULL,
    ConsumerId           INT NOT NULL,
    UserId               INT NOT NULL,
    LastReadMessageId    INT NOT NULL DEFAULT 0,
    PRIMARY KEY (ServiceId, ConsumerId, UserId),
    CONSTRAINT FK_MessageReadStatus_Service FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE,
    CONSTRAINT FK_MessageReadStatus_User FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
