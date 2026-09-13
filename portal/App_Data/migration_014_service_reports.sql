-- Портал послуг Safina — міграція 014: скарги на оголошення (наступна фіча
-- понад MVP, обрано автономно циклом /loop, 2026-09-13). Будь-хто (включно з
-- анонімом — сторінка оголошення відкрита, п.12) може поскаржитись на
-- оголошення (неправдива інформація/шахрайство/заборонений контент/
-- дублікат/інше); адмін бачить чергу відкритих скарг (AdminReports.aspx) і
-- позначає переглянутою. Не автоматична модерація — лише сигнал адміну,
-- Status оголошення не змінюється сама собою.
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_014_service_reports.sql

CREATE TABLE IF NOT EXISTS ServiceReports (
    ReportId        INT AUTO_INCREMENT PRIMARY KEY,
    ServiceId       INT NOT NULL,
    ReporterEmail   VARCHAR(255) NULL, -- Nothing — скаржився анонім
    Reason          VARCHAR(50) NOT NULL,
    Comment         VARCHAR(1000) NULL,
    Status          ENUM('Open','Reviewed') NOT NULL DEFAULT 'Open',
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ReviewedAt      DATETIME NULL,
    ReviewedBy      INT NULL,
    CONSTRAINT FK_ServiceReports_Service FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE,
    CONSTRAINT FK_ServiceReports_ReviewedBy FOREIGN KEY (ReviewedBy) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
