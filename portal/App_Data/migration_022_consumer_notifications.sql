-- Портал послуг Safina — міграція 022: налаштування листів для споживача
-- (2026-09-25, роль Споживач, п.5 і п.8 аудиту).
-- 1) Персональний дайджест: споживач обирає категорії (порожньо = усі) і район
--    (NULL = усі), або вимикає дайджест зовсім (DigestEnabled).
-- 2) Сповіщення про «Обране»: знімок ціни/статусу на момент додавання або
--    останнього листа (LastKnownPrice/LastKnownStatus); щоденна перевірка шле лист,
--    коли ціна змінилась або оголошення зняли з публікації / повернули.
--    FavoriteAlertsEnabled — окремий вимикач цих листів.
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_022_consumer_notifications.sql

CREATE TABLE IF NOT EXISTS UserDigestCategories (
    UserId      INT NOT NULL,
    CategoryId  INT NOT NULL,
    PRIMARY KEY (UserId, CategoryId),
    CONSTRAINT FK_UserDigestCategories_User FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserDigestCategories_Category FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

ALTER TABLE Users
    ADD COLUMN DigestEnabled TINYINT(1) NOT NULL DEFAULT 1,
    ADD COLUMN DigestDistrict VARCHAR(100) NULL,
    ADD COLUMN FavoriteAlertsEnabled TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE Favorites
    ADD COLUMN LastKnownPrice DECIMAL(10,2) NULL,
    ADD COLUMN LastKnownStatus VARCHAR(20) NULL;

-- Наявне «Обране» — знімок поточного стану, щоб перша перевірка не розіслала
-- листи про «зміни», яких насправді не було.
UPDATE Favorites f JOIN Services s ON s.ServiceId = f.ServiceId
SET f.LastKnownPrice = s.Price, f.LastKnownStatus = s.Status;
