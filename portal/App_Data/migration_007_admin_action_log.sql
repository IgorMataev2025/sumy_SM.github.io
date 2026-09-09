-- Портал послуг Safina — міграція 007: журнал дій адміністратора (концептуальне
-- рішення 2026-09-09, п.10 — доповнення до організації адмінки). ModerationLog
-- (схема з коробки) уже пише Approve/Reject; ця таблиця покриває решту дій
-- (блокування користувача, зміни категорій), щоб AdminLog.aspx показував
-- єдину стрічку.
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_007_admin_action_log.sql
-- (з cmd.exe через байтовий редірект `<` — НЕ через PowerShell pipe)

CREATE TABLE IF NOT EXISTS AdminActionLog (
    LogId               INT AUTO_INCREMENT PRIMARY KEY,
    AdminId             INT NOT NULL,
    Action              VARCHAR(50) NOT NULL,
    TargetDescription   VARCHAR(255) NOT NULL,
    ActionDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_AdminActionLog_Admin FOREIGN KEY (AdminId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
