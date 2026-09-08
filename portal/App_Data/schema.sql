-- Портал послуг Safina — схема БД (MySQL 8.x)
-- Відповідає ТЗ_портал_послуг.md, розділ 7.
-- Застосування: mysql -u root -p sumy_portal < schema.sql

SET NAMES utf8mb4;

CREATE TABLE IF NOT EXISTS Users (
    UserId          INT AUTO_INCREMENT PRIMARY KEY,
    Email           VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash    VARCHAR(255) NOT NULL,
    FullName        VARCHAR(255) NOT NULL,
    Phone           VARCHAR(50)  NULL,
    UserType        ENUM('Consumer','Provider') NOT NULL,
    IsLegalEntity   BOOLEAN NOT NULL DEFAULT FALSE,
    CompanyName     VARCHAR(255) NULL,
    EDRPOU          VARCHAR(20)  NULL,
    District        VARCHAR(100) NULL, -- одна з 5 офіційних назв, App_Code/SumyDistricts.vb
    IsActive        BOOLEAN NOT NULL DEFAULT TRUE,
    IsAdmin         BOOLEAN NOT NULL DEFAULT FALSE, -- етап 4, migration_003_admin.sql
    EmailConfirmed  BOOLEAN NOT NULL DEFAULT FALSE, -- етап 2, migration_002_auth.sql
    EmailConfirmationToken VARCHAR(100) NULL,
    PasswordResetToken     VARCHAR(100) NULL,
    PasswordResetExpires   DATETIME NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_Users_EmailConfirmationToken (EmailConfirmationToken),
    INDEX IX_Users_PasswordResetToken (PasswordResetToken)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Categories (
    CategoryId      INT AUTO_INCREMENT PRIMARY KEY,
    Name            VARCHAR(150) NOT NULL,
    Description     VARCHAR(500) NULL, -- розширення понад ТЗ п.7: короткий підпис для картки категорії на головній
    ParentId        INT NULL,
    IsActive        BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentId) REFERENCES Categories(CategoryId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Services (
    ServiceId       INT AUTO_INCREMENT PRIMARY KEY,
    ProviderId      INT NOT NULL,
    CategoryId      INT NOT NULL,
    Title           VARCHAR(255) NOT NULL,
    Description     TEXT NULL,
    Price           DECIMAL(10,2) NULL,
    District        VARCHAR(100) NULL, -- одна з 5 офіційних назв, App_Code/SumyDistricts.vb
    Phone           VARCHAR(50) NULL,
    Status          ENUM('Draft','Pending','Approved','Rejected') NOT NULL DEFAULT 'Draft',
    RejectReason    VARCHAR(500) NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ApprovedAt      DATETIME NULL,
    ApprovedBy      INT NULL,
    CONSTRAINT FK_Services_Provider FOREIGN KEY (ProviderId) REFERENCES Users(UserId),
    CONSTRAINT FK_Services_Category FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT FK_Services_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS ServicePhotos (
    PhotoId         INT AUTO_INCREMENT PRIMARY KEY,
    ServiceId       INT NOT NULL,
    FilePath        VARCHAR(500) NOT NULL,
    CONSTRAINT FK_ServicePhotos_Service FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS ModerationLog (
    LogId           INT AUTO_INCREMENT PRIMARY KEY,
    ServiceId       INT NOT NULL,
    AdminId         INT NOT NULL,
    Action          ENUM('Approved','Rejected') NOT NULL,
    Comment         VARCHAR(500) NULL,
    ActionDate      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ModerationLog_Service FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE,
    CONSTRAINT FK_ModerationLog_Admin FOREIGN KEY (AdminId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Donations (
    DonationId      INT AUTO_INCREMENT PRIMARY KEY,
    OrderId         VARCHAR(64) NOT NULL UNIQUE,
    Amount          DECIMAL(10,2) NOT NULL,
    Currency        VARCHAR(3) NOT NULL DEFAULT 'UAH',
    Status          ENUM('Pending','Success','Failure') NOT NULL DEFAULT 'Pending',
    LiqPayStatus    VARCHAR(50) NULL, -- сирий статус з callback LiqPay (success/failure/reversed/... ) для діагностики
    PaymentId       VARCHAR(64) NULL, -- payment_id від LiqPay після обробки
    DonorName       VARCHAR(255) NULL,
    DonorEmail      VARCHAR(255) NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt       DATETIME NULL,
    INDEX IX_Donations_OrderId (OrderId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Категорії MVP (ТЗ, розділ 3) — початкове наповнення довідника.
INSERT INTO Categories (Name, Description, ParentId, IsActive)
SELECT * FROM (SELECT
    'Побутові послуги' AS Name,
    'Ремонт, клінінг, сантехніка, електрика тощо' AS Description,
    NULL AS ParentId, TRUE AS IsActive) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Побутові послуги');

INSERT INTO Categories (Name, Description, ParentId, IsActive)
SELECT * FROM (SELECT
    'Медицина та здоров''я',
    'Приватні лікарі, клініки, стоматологія',
    NULL, TRUE) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Медицина та здоров''я');

INSERT INTO Categories (Name, Description, ParentId, IsActive)
SELECT * FROM (SELECT
    'Освіта та репетиторство',
    'Курси, репетитори, дитячі гуртки',
    NULL, TRUE) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Освіта та репетиторство');

INSERT INTO Categories (Name, Description, ParentId, IsActive)
SELECT * FROM (SELECT
    'Комунальні / державні послуги',
    'Інформація від ОСББ, комунальних служб, адмінпослуги',
    NULL, TRUE) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Комунальні / державні послуги');
