-- Портал послуг Safina — міграція 015: сигналізація для відеодзвінків
-- Постачальник↔Споживач (наступна фіча понад MVP, обрано користувачем,
-- 2026-09-13). WebRTC P2P (браузер↔браузер, без відео-сервера) — ця
-- таблиця лише передає SDP offer/answer і ICE-кандидати між двома
-- сторонами через AJAX-polling (CallSignal.ashx), той самий принцип, що
-- вже жива переписка (MessagesPoll.ashx, migration 7ade45e). Сама
-- аудіо/відео-доріжка через цей сервер НЕ проходить.
-- Застосування: mysql --default-character-set=utf8mb4 -u root -p sumy_portal < migration_015_video_call_signals.sql

CREATE TABLE IF NOT EXISTS VideoCallSignals (
    SignalId    INT AUTO_INCREMENT PRIMARY KEY,
    ServiceId   INT NOT NULL,
    ConsumerId  INT NOT NULL,
    SenderId    INT NOT NULL,
    SignalType  ENUM('offer','answer','ice-candidate','hangup') NOT NULL,
    Payload     TEXT NOT NULL, -- SDP (offer/answer) чи ICE-кандидат, як є, у вигляді рядка
    CreatedAt   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_VideoCallSignals_Service FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE,
    CONSTRAINT FK_VideoCallSignals_Consumer FOREIGN KEY (ConsumerId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_VideoCallSignals_Sender FOREIGN KEY (SenderId) REFERENCES Users(UserId) ON DELETE CASCADE,
    INDEX IX_VideoCallSignals_Thread (ServiceId, ConsumerId, SignalId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
