# Database Schema — Backend

> PostgreSQL schema đầy đủ. Chỉ Backend cần biết.

---

## Nguyên tắc

- Mỗi service có database schema riêng (tách bằng PostgreSQL schema/namespace)
- Primary key: `UUID` dùng `gen_random_uuid()`
- Timestamps: `TIMESTAMPTZ NOT NULL DEFAULT NOW()`
- Soft delete: `deleted_at TIMESTAMPTZ NULL`
- Index: đặt tên `idx_{table}_{column(s)}`
- Migration: dùng EF Core Migrations, không sửa DB tay

---

## Schema: identity

```sql
CREATE SCHEMA IF NOT EXISTS identity;

-- Users
CREATE TABLE identity.users (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  zalo_id         VARCHAR(50)  NOT NULL UNIQUE,
  display_name    VARCHAR(100) NOT NULL,
  avatar_url      TEXT,
  total_points    INT          NOT NULL DEFAULT 0,
  rank_title      VARCHAR(50)  NOT NULL DEFAULT 'Thôn',
  is_active       BOOLEAN      NOT NULL DEFAULT true,
  created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  updated_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  last_login_at   TIMESTAMPTZ,
  deleted_at      TIMESTAMPTZ
);

CREATE INDEX idx_users_zalo_id ON identity.users(zalo_id);
CREATE INDEX idx_users_total_points ON identity.users(total_points DESC);
CREATE INDEX idx_users_rank_title ON identity.users(rank_title);

-- Auto-update updated_at
CREATE OR REPLACE FUNCTION update_updated_at()
RETURNS TRIGGER AS $$
BEGIN NEW.updated_at = NOW(); RETURN NEW; END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_users_updated_at
  BEFORE UPDATE ON identity.users
  FOR EACH ROW EXECUTE FUNCTION update_updated_at();
```

---

## Schema: game

```sql
CREATE SCHEMA IF NOT EXISTS game;

-- Game sessions
CREATE TABLE game.sessions (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  game_type       VARCHAR(50)  NOT NULL,  -- 'o_an_quan', 'co_caro'...
  mode            VARCHAR(20)  NOT NULL,  -- 'solo', 'ranked', 'room'
  player1_id      UUID         NOT NULL,  -- FK → identity.users.id
  player2_id      UUID,                   -- NULL nếu vs AI
  winner_id       UUID,                   -- NULL nếu chưa kết thúc / hòa
  is_draw         BOOLEAN      NOT NULL DEFAULT false,
  player1_score   INT          NOT NULL DEFAULT 0,
  player2_score   INT          NOT NULL DEFAULT 0,
  player1_side    SMALLINT     NOT NULL DEFAULT 1, -- 1 = hàng dưới
  ai_difficulty   VARCHAR(20),            -- 'easy'|'medium'|'hard', NULL nếu PvP
  initial_state   JSONB        NOT NULL,
  final_state     JSONB,
  moves           JSONB        NOT NULL DEFAULT '[]',
  points_awarded  INT          NOT NULL DEFAULT 0,
  status          VARCHAR(20)  NOT NULL DEFAULT 'playing', -- playing|finished|abandoned
  room_id         UUID,                   -- FK → game.rooms.id nếu mode=room
  started_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  ended_at        TIMESTAMPTZ,
  created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  updated_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_sessions_player1_id ON game.sessions(player1_id);
CREATE INDEX idx_sessions_player2_id ON game.sessions(player2_id);
CREATE INDEX idx_sessions_game_type ON game.sessions(game_type);
CREATE INDEX idx_sessions_mode ON game.sessions(mode);
CREATE INDEX idx_sessions_status ON game.sessions(status);
CREATE INDEX idx_sessions_started_at ON game.sessions(started_at DESC);

-- Rooms (phòng bạn bè)
CREATE TABLE game.rooms (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  room_code   VARCHAR(6)   NOT NULL UNIQUE,
  game_type   VARCHAR(50)  NOT NULL,
  host_id     UUID         NOT NULL,
  status      VARCHAR(20)  NOT NULL DEFAULT 'waiting',
  max_players SMALLINT     NOT NULL DEFAULT 2,
  created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  expires_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW() + INTERVAL '2 hours'
);

CREATE INDEX idx_rooms_room_code ON game.rooms(room_code);
CREATE INDEX idx_rooms_host_id ON game.rooms(host_id);
CREATE INDEX idx_rooms_status ON game.rooms(status);

-- Room players
CREATE TABLE game.room_players (
  room_id     UUID        NOT NULL,
  user_id     UUID        NOT NULL,
  joined_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  is_ready    BOOLEAN     NOT NULL DEFAULT false,
  PRIMARY KEY (room_id, user_id)
);
```

---

## Schema: mission

```sql
CREATE SCHEMA IF NOT EXISTS mission;

-- Mission definitions (seed data)
CREATE TABLE mission.definitions (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  type            VARCHAR(50)  NOT NULL UNIQUE, -- 'daily_login', 'win_games'...
  title           VARCHAR(200) NOT NULL,
  description     TEXT,
  reward_points   INT          NOT NULL,
  target          INT          NOT NULL DEFAULT 1,
  reset_type      VARCHAR(20)  NOT NULL DEFAULT 'daily', -- 'daily'|'weekly'|'once'
  game_type       VARCHAR(50),  -- NULL = mọi game
  is_active       BOOLEAN      NOT NULL DEFAULT true,
  created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- User mission progress (reset hàng ngày)
CREATE TABLE mission.user_progress (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id         UUID         NOT NULL,
  definition_id   UUID         NOT NULL REFERENCES mission.definitions(id),
  date            DATE         NOT NULL DEFAULT CURRENT_DATE,
  progress        INT          NOT NULL DEFAULT 0,
  status          VARCHAR(20)  NOT NULL DEFAULT 'pending',
  claimed_at      TIMESTAMPTZ,
  created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  updated_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  UNIQUE (user_id, definition_id, date)
);

CREATE INDEX idx_user_progress_user_date ON mission.user_progress(user_id, date);
CREATE INDEX idx_user_progress_status ON mission.user_progress(status);

-- Point transactions (audit log)
CREATE TABLE mission.point_transactions (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id         UUID         NOT NULL,
  delta           INT          NOT NULL,  -- dương = cộng, âm = trừ
  reason          VARCHAR(100) NOT NULL,  -- 'mission_claim', 'game_win'...
  reference_id    UUID,                   -- mission_id hoặc session_id
  created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_point_tx_user_id ON mission.point_transactions(user_id);
CREATE INDEX idx_point_tx_created_at ON mission.point_transactions(created_at DESC);
```

---

## Schema: leaderboard

```sql
CREATE SCHEMA IF NOT EXISTS leaderboard;

-- Season definitions
CREATE TABLE leaderboard.seasons (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  season_num  INT          NOT NULL UNIQUE,
  start_date  DATE         NOT NULL,
  end_date    DATE         NOT NULL,
  is_active   BOOLEAN      NOT NULL DEFAULT false,
  created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Season rankings snapshot (cuối mùa)
CREATE TABLE leaderboard.season_rankings (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  season_id   UUID         NOT NULL REFERENCES leaderboard.seasons(id),
  user_id     UUID         NOT NULL,
  rank        INT          NOT NULL,
  points      INT          NOT NULL,
  created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  UNIQUE (season_id, user_id)
);

CREATE INDEX idx_season_rankings_season_rank ON leaderboard.season_rankings(season_id, rank);
```

---

## Seed data (Mission definitions)

```sql
INSERT INTO mission.definitions (type, title, description, reward_points, target, reset_type, game_type)
VALUES
  ('daily_login',   'Đăng nhập hôm nay',         'Mở app và đăng nhập',          50,  1, 'daily',  NULL),
  ('win_3_games',   'Thắng 3 ván',                'Thắng 3 ván bất kỳ',           150, 3, 'daily',  NULL),
  ('play_with_friend', 'Chơi cùng bạn Zalo',      'Chơi 1 ván phòng bạn bè',      180, 1, 'daily',  NULL),
  ('win_5_streak',  'Thắng 5 ván liên tiếp',      'Thắng 5 ván không thua',       200, 5, 'weekly', NULL),
  ('complete_tutorial', 'Hoàn thành hướng dẫn',   'Xem hướng dẫn cách chơi',      50,  1, 'once',   'o_an_quan');
```

---

## Migration strategy

```bash
# Tạo migration mới (chạy trong thư mục service)
dotnet ef migrations add InitialCreate --project src/ServiceName

# Apply migration
dotnet ef database update

# Revert migration
dotnet ef database update PreviousMigrationName

# Xem SQL (không apply)
dotnet ef migrations script
```

**Quy tắc migration:**
- Không bao giờ sửa migration đã commit
- Mỗi thay đổi schema = 1 migration mới
- Migration phải chạy được idempotent (chạy nhiều lần không lỗi)
- Có migration `Down()` rõ ràng để rollback
