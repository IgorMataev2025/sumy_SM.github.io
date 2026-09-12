-- Портал послуг Safina — міграція 009: геолокація оголошень + галерея на акаунті постачальника
-- (постановка робочої тестової версії, 2026-09-12).
-- Latitude/Longitude — клік на карті в ServiceEdit.aspx, лише інформативна мітка на
-- ServiceDetails.aspx (не замінює District — район лишається без змін, координати доповнюють).
-- ProviderGalleryPhotos — окремо від ServicePhotos: галерея на Profile.aspx, публічна
-- (Profile.aspx?providerId=X, доступна й неавторизованим), не прив'язана до конкретного
-- оголошення, до 5 фото ≤3МБ (обмеження на рівні застосунку, як і ServicePhotos).
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_009_geolocation_gallery.sql

ALTER TABLE Services
    ADD COLUMN Latitude DECIMAL(9,6) NULL,
    ADD COLUMN Longitude DECIMAL(9,6) NULL;

CREATE TABLE IF NOT EXISTS ProviderGalleryPhotos (
    PhotoId         INT AUTO_INCREMENT PRIMARY KEY,
    ProviderId      INT NOT NULL,
    FilePath        VARCHAR(500) NOT NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ProviderGalleryPhotos_Provider FOREIGN KEY (ProviderId) REFERENCES Users(UserId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
