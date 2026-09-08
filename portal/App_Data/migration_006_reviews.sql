-- Портал послуг Safina — міграція 006: рейтинги/відгуки (уточнена постановка
-- від 2026-09-08, п.9 — четверта, остання фіча понад ТЗ MVP).
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_006_reviews.sql
-- (з cmd.exe через байтовий редірект `<` — НЕ через PowerShell pipe)

CREATE TABLE IF NOT EXISTS Reviews (
    ReviewId        INT AUTO_INCREMENT PRIMARY KEY,
    ServiceId       INT NOT NULL,
    ConsumerId      INT NOT NULL,
    Rating          TINYINT NOT NULL,
    Comment         VARCHAR(1000) NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Reviews_Service FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE,
    CONSTRAINT FK_Reviews_Consumer FOREIGN KEY (ConsumerId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT CK_Reviews_Rating CHECK (Rating BETWEEN 1 AND 5),
    -- Один відгук від одного користувача на одне оголошення (MVP-спрощення —
    -- без перевірки факту замовлення, бо портал не веде трекінг угод).
    UNIQUE KEY UQ_Reviews_Service_Consumer (ServiceId, ConsumerId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
