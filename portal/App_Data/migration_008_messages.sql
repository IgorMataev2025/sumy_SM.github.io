-- Портал послуг Safina — міграція 008: обмін повідомленнями Постачальник↔Споживач
-- (концептуальне рішення 2026-09-09, п.10 — динамічний обмін інформацією).
-- Розмова = пара (ServiceId, ConsumerId); постачальник визначається через
-- Services.ProviderId, окремо не зберігається. SenderId — хто саме написав
-- це конкретне повідомлення (споживач або постачальник цього оголошення).
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_008_messages.sql
-- (з cmd.exe через байтовий редірект `<` — НЕ через PowerShell pipe)

CREATE TABLE IF NOT EXISTS Messages (
    MessageId       INT AUTO_INCREMENT PRIMARY KEY,
    ServiceId       INT NOT NULL,
    ConsumerId      INT NOT NULL,
    SenderId        INT NOT NULL,
    Body            VARCHAR(2000) NOT NULL,
    SentAt          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Messages_Service FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE,
    CONSTRAINT FK_Messages_Consumer FOREIGN KEY (ConsumerId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Messages_Sender FOREIGN KEY (SenderId) REFERENCES Users(UserId) ON DELETE CASCADE,
    INDEX IX_Messages_Thread (ServiceId, ConsumerId, SentAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
