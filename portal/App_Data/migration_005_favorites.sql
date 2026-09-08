-- Портал послуг Safina — міграція 005: вподобані оголошення (уточнена
-- постановка від 2026-09-08, п.8 — третя фіча понад ТЗ MVP).
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_005_favorites.sql
-- (з cmd.exe через байтовий редірект `<` — НЕ через PowerShell pipe)

CREATE TABLE IF NOT EXISTS Favorites (
    UserId          INT NOT NULL,
    ServiceId       INT NOT NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, ServiceId),
    CONSTRAINT FK_Favorites_User FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Favorites_Service FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
