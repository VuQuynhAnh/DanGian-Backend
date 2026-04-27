# Database Schema — Backend

> PostgreSQL schema đầy đủ. Chỉ Backend cần biết.
> Managed bởi EF Core Migrations — không sửa DB tay.

---

## Nguyên tắc

- Single database, single DbContext (`ApplicationDbContext`)
- Primary key: `UUID` (`Guid` trong C#, `gen_random_uuid()` trong PostgreSQL)
- Timestamps: `TIMESTAMPTZ NOT NULL DEFAULT NOW()`
- Soft delete: `deleted_at TIMESTAMPTZ NULL`
- Index: đặt tên `IX_{Table}_{Column(s)}` (EF Core convention)
- Migration: `dotnet ef migrations add` — không sửa DB tay

---

## Entities & Tables

### identity.users → `Users`

```sql
CREATE TABLE "Users" (
  "Id"            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  "ZaloId"        VARCHAR(50)  NOT NULL UNIQUE,
  "DisplayName"   VARCHAR(100) NOT NULL,
  "AvatarUrl"     TEXT,
  "TotalPoints"   INT          NOT NULL DEFAULT 0,
  "RankTitle"     VARCHAR(50)  NOT NULL DEFAULT 'Thôn',
  "IsActive"      BOOLEAN      NOT NULL DEFAULT true,
  "CreatedAt"     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  "UpdatedAt"     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  "LastLoginAt"   TIMESTAMPTZ,
  "DeletedAt"     TIMESTAMPTZ
);

CREATE INDEX "IX_Users_ZaloId" ON "Users"("ZaloId");
CREATE INDEX "IX_Users_TotalPoints" ON "Users"("TotalPoints" DESC);
```

**C# Aggregate:** `src/DanGian.Domain/Aggregates/User.cs`

---

### game.sessions → `GameSessions`

```sql
CREATE TABLE "GameSessions" (
  "Id"            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  "GameType"      VARCHAR(50)  NOT NULL,   -- 'o_an_quan', 'co_caro'
  "Mode"          VARCHAR(20)  NOT NULL,   -- 'solo', 'ranked', 'room'
  "Player1Id"     UUID         NOT NULL REFERENCES "Users"("Id"),
  "Player2Id"     UUID         REFERENCES "Users"("Id"),  -- NULL nếu vs AI
  "WinnerId"      UUID,
  "IsDraw"        BOOLEAN      NOT NULL DEFAULT false,
  "Player1Score"  INT          NOT NULL DEFAULT 0,
  "Player2Score"  INT          NOT NULL DEFAULT 0,
  "Player1Side"   SMALLINT     NOT NULL DEFAULT 1,
  "AiDifficulty"  VARCHAR(20),             -- 'easy'|'medium'|'hard', NULL nếu PvP
  "InitialState"  JSONB        NOT NULL,
  "FinalState"    JSONB,
  "Moves"         JSONB        NOT NULL DEFAULT '[]',
  "PointsAwarded" INT          NOT NULL DEFAULT 0,
  "Status"        VARCHAR(20)  NOT NULL DEFAULT 'playing',  -- playing|finished|abandoned
  "RoomId"        UUID         REFERENCES "Rooms"("Id"),
  "StartedAt"     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  "EndedAt"       TIMESTAMPTZ,
  "CreatedAt"     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  "UpdatedAt"     TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX "IX_GameSessions_Player1Id" ON "GameSessions"("Player1Id");
CREATE INDEX "IX_GameSessions_Player2Id" ON "GameSessions"("Player2Id");
CREATE INDEX "IX_GameSessions_Status" ON "GameSessions"("Status");
CREATE INDEX "IX_GameSessions_StartedAt" ON "GameSessions"("StartedAt" DESC);
```

**C# Aggregate:** `src/DanGian.Domain/Aggregates/GameSession.cs`

---

### game.rooms → `Rooms`

```sql
CREATE TABLE "Rooms" (
  "Id"          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  "RoomCode"    VARCHAR(6)   NOT NULL UNIQUE,
  "GameType"    VARCHAR(50)  NOT NULL,
  "HostId"      UUID         NOT NULL REFERENCES "Users"("Id"),
  "Status"      VARCHAR(20)  NOT NULL DEFAULT 'waiting',  -- waiting|playing|finished
  "MaxPlayers"  SMALLINT     NOT NULL DEFAULT 2,
  "CreatedAt"   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  "ExpiresAt"   TIMESTAMPTZ  NOT NULL DEFAULT NOW() + INTERVAL '2 hours'
);

CREATE INDEX "IX_Rooms_RoomCode" ON "Rooms"("RoomCode");
CREATE INDEX "IX_Rooms_HostId" ON "Rooms"("HostId");
```

**C# Aggregate:** `src/DanGian.Domain/Aggregates/Room.cs`

---

### mission.definitions → `MissionDefinitions`

```sql
CREATE TABLE "MissionDefinitions" (
  "Id"            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  "Type"          VARCHAR(50)  NOT NULL UNIQUE,   -- 'daily_login', 'win_games'
  "Title"         VARCHAR(200) NOT NULL,
  "Description"   TEXT,
  "RewardPoints"  INT          NOT NULL,
  "Target"        INT          NOT NULL DEFAULT 1,
  "ResetType"     VARCHAR(20)  NOT NULL DEFAULT 'daily',  -- 'daily'|'weekly'|'once'
  "GameType"      VARCHAR(50),   -- NULL = mọi game
  "IsActive"      BOOLEAN      NOT NULL DEFAULT true,
  "CreatedAt"     TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);
```

**C# Aggregate:** `src/DanGian.Domain/Aggregates/MissionDefinition.cs`

---

### mission.user_progress → `UserMissionProgresses`

```sql
CREATE TABLE "UserMissionProgresses" (
  "Id"            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  "UserId"        UUID         NOT NULL REFERENCES "Users"("Id"),
  "DefinitionId"  UUID         NOT NULL REFERENCES "MissionDefinitions"("Id"),
  "Date"          DATE         NOT NULL DEFAULT CURRENT_DATE,
  "Progress"      INT          NOT NULL DEFAULT 0,
  "Status"        VARCHAR(20)  NOT NULL DEFAULT 'pending',  -- pending|in_progress|completed|claimed
  "ClaimedAt"     TIMESTAMPTZ,
  "CreatedAt"     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  "UpdatedAt"     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  UNIQUE ("UserId", "DefinitionId", "Date")
);

CREATE INDEX "IX_UserMissionProgresses_UserId_Date" ON "UserMissionProgresses"("UserId", "Date");
```

**C# Aggregate:** `src/DanGian.Domain/Aggregates/UserMissionProgress.cs`

---

### leaderboard.seasons → `Seasons`

```sql
CREATE TABLE "Seasons" (
  "Id"          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  "SeasonNum"   INT          NOT NULL UNIQUE,
  "StartDate"   DATE         NOT NULL,
  "EndDate"     DATE         NOT NULL,
  "IsActive"    BOOLEAN      NOT NULL DEFAULT false,
  "CreatedAt"   TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);
```

**C# Aggregate:** `src/DanGian.Domain/Aggregates/Season.cs`

---

## Seed data (Mission definitions)

```sql
INSERT INTO "MissionDefinitions" ("Id", "Type", "Title", "Description", "RewardPoints", "Target", "ResetType", "GameType")
VALUES
  (gen_random_uuid(), 'daily_login',        'Đăng nhập hôm nay',        'Mở app và đăng nhập',         50,  1, 'daily',  NULL),
  (gen_random_uuid(), 'win_3_games',        'Thắng 3 ván',              'Thắng 3 ván bất kỳ',          150, 3, 'daily',  NULL),
  (gen_random_uuid(), 'play_with_friend',   'Chơi cùng bạn Zalo',       'Chơi 1 ván phòng bạn bè',     180, 1, 'daily',  NULL),
  (gen_random_uuid(), 'win_5_streak',       'Thắng 5 ván liên tiếp',    'Thắng 5 ván không thua',      200, 5, 'weekly', NULL),
  (gen_random_uuid(), 'complete_tutorial',  'Hoàn thành hướng dẫn',     'Xem hướng dẫn cách chơi',     50,  1, 'once',   'o_an_quan');
```

---

## Migration commands

```bash
# Từ thư mục root của solution
dotnet ef migrations add <MigrationName> \
  --project src/DanGian.Infrastructure \
  --startup-project src/DanGian.Api

# Apply migration
dotnet ef database update \
  --project src/DanGian.Infrastructure \
  --startup-project src/DanGian.Api

# Xem SQL script (không apply)
dotnet ef migrations script \
  --project src/DanGian.Infrastructure \
  --startup-project src/DanGian.Api

# Revert về migration trước
dotnet ef database update <PreviousMigrationName> \
  --project src/DanGian.Infrastructure \
  --startup-project src/DanGian.Api
```

## Quy tắc migration

- Không bao giờ sửa migration đã commit
- Mỗi thay đổi schema = 1 migration mới với tên rõ ràng (VD: `AddUserLastLoginAt`)
- Luôn có method `Down()` để rollback
- Chạy migration qua CI/CD — không apply tay lên production
