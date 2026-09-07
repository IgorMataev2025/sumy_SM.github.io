-- Портал послуг Safina — міграція 004: донат-модуль через LiqPay (уточнена
-- постановка від 2026-09-07, п.1 — "переказ коштів на підтримку проєкту").
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_004_donations.sql
-- (з cmd.exe через байтовий редірект `<` — НЕ через PowerShell pipe)

CREATE TABLE IF NOT EXISTS Donations (
    DonationId      INT AUTO_INCREMENT PRIMARY KEY,
    OrderId         VARCHAR(64) NOT NULL UNIQUE,
    Amount          DECIMAL(10,2) NOT NULL,
    Currency        VARCHAR(3) NOT NULL DEFAULT 'UAH',
    Status          ENUM('Pending','Success','Failure') NOT NULL DEFAULT 'Pending',
    LiqPayStatus    VARCHAR(50) NULL,
    PaymentId       VARCHAR(64) NULL,
    DonorName       VARCHAR(255) NULL,
    DonorEmail      VARCHAR(255) NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt       DATETIME NULL,
    INDEX IX_Donations_OrderId (OrderId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
