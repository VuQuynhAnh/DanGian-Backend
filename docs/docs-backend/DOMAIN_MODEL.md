# Domain Model — Dân Gian Backend

> Mô tả chi tiết từng Aggregate/Entity trong domain: vai trò, ý nghĩa từng trường, business rules.
>
> **Quy tắc cập nhật:** Mỗi khi thêm/xóa field, thêm/xóa method, hoặc thay đổi business rule trong code → cập nhật file này ngay trong cùng commit.

---

## Bounded Contexts

| Context | Aggregates | Entities | Value Objects |
|---------|------------|----------|---------------|
| Identity | `User` | — | — |
| Game | `GameSession`, `Room` | `RoomPlayer` | `RoomCode` |
| Mission | `MissionDefinition`, `UserMissionProgress` | `PointTransaction` | — |
| Leaderboard | `Season` | `SeasonRanking` | — |

---

## Identity Context

### `User` — Aggregate Root

**Vai trò:** Đại diện cho một người chơi trong hệ thống. Được tạo tự động khi đăng nhập lần đầu qua Zalo OAuth.

**File:** `src/DanGian.Domain/Identity/User.cs`

| Trường | Kiểu | Ý nghĩa |
|--------|------|---------|
| `Id` | `Guid` | Primary key, tự sinh |
| `ZaloId` | `string(50)` | ID duy nhất từ Zalo OAuth — dùng để nhận diện user giữa các lần login |
| `DisplayName` | `string(100)` | Tên hiển thị, lấy từ Zalo profile, có thể được user cập nhật |
| `AvatarUrl` | `string?` | URL ảnh đại diện từ Zalo, nullable |
| `TotalPoints` | `int` | Tổng điểm tích lũy qua các mission — dùng để tính rank |
| `RankTitle` | `string(50)` | Danh hiệu xếp hạng hiện tại (Thôn → Làng xã → ...) — tính từ `TotalPoints` |
| `IsActive` | `bool` | `true` = tài khoản hoạt động bình thường. Default `true` |
| `LastLoginAt` | `DateTime?` | Thời điểm đăng nhập gần nhất — dùng cho mission daily_login |
| `DeletedAt` | `DateTime?` | Soft delete marker. Khi != null thì user bị vô hiệu hóa |
| `CreatedAt` | `DateTime` | Thời điểm tạo tài khoản lần đầu |
| `UpdatedAt` | `DateTime` | Cập nhật mỗi khi có thay đổi |

**Methods hiện có:**

| Method | Mô tả |
|--------|-------|
| `Create(zaloId, displayName, avatarUrl)` | Factory — tạo user mới khi đăng nhập lần đầu |
| `UpdateProfile(displayName, avatarUrl)` | Đồng bộ tên + avatar từ Zalo mỗi lần login |
| `RecordLogin()` | Cập nhật `LastLoginAt = UtcNow` |

**Business rules:**
- `ZaloId` là duy nhất — không thể có 2 user cùng ZaloId
- `TotalPoints` chỉ tăng, không giảm (trừ khi có penalty feature sau này)
- User bị soft-deleted (`DeletedAt != null`) vẫn còn trong DB, không xóa cứng

---

## Game Context

### `GameSession` — Aggregate Root

**Vai trò:** Đại diện cho một ván đấu cụ thể — từ lúc bắt đầu đến khi kết thúc. Lưu toàn bộ diễn biến và kết quả.

**File:** `src/DanGian.Domain/Game/GameSession.cs`

| Trường | Kiểu | Ý nghĩa |
|--------|------|---------|
| `Id` | `Guid` | Primary key |
| `GameType` | `GameType` (enum) | Loại game: `OAnQuan`, `CoCaro`, ... |
| `Mode` | `GameMode` (enum) | Chế độ chơi: `Solo` (vs AI), `Ranked`, `Room` (bạn bè) |
| `Player1Id` | `Guid` | FK → User. Người tạo phiên, luôn có giá trị |
| `Player2Id` | `Guid?` | FK → User. Null nếu Solo (đối thủ là AI) |
| `WinnerId` | `Guid?` | FK → User. Null khi đang chơi hoặc hòa |
| `IsDraw` | `bool` | `true` nếu ván đấu hòa |
| `Player1Score` | `int` | Điểm số cuối ván của Player1 (số viên ăn được) |
| `Player2Score` | `int` | Điểm số cuối ván của Player2 / AI |
| `Player1Side` | `int` | Phía của Player1: `1` = hàng dưới, `2` = hàng trên. Default `1` |
| `AiDifficulty` | `AiDifficulty?` | Độ khó AI: `Easy`, `Medium`, `Hard`. Null nếu PvP |
| `InitialState` | `string` (JSON) | Trạng thái bàn cờ ban đầu — snapshot để replay |
| `FinalState` | `string?` (JSON) | Trạng thái bàn cờ lúc kết thúc. Null khi đang chơi |
| `Moves` | `string` (JSON array) | Danh sách nước đi theo thứ tự — `"[]"` khi mới tạo |
| `PointsAwarded` | `int` | Điểm mission được cộng sau ván này. `0` khi đang chơi |
| `Status` | `GameStatus` (enum) | `Playing`, `Finished`, `Abandoned` |
| `RoomId` | `Guid?` | FK → Room. Chỉ có giá trị khi Mode = Room |
| `StartedAt` | `DateTime` | Thời điểm bắt đầu ván |
| `EndedAt` | `DateTime?` | Thời điểm kết thúc. Null khi đang chơi |

**Methods hiện có:**

| Method | Mô tả |
|--------|-------|
| `Create(gameType, mode, player1Id, initialState, ...)` | Factory — tạo phiên mới, gán Player1Side = 1 mặc định |

**Methods sẽ thêm khi implement:**

| Method | Sprint | Mô tả |
|--------|--------|-------|
| `RecordMove(movesJson)` | Sprint 2 | Cập nhật danh sách nước đi |
| `Finish(winnerId, isDraw, scores, finalState, points)` | Sprint 2 | Kết thúc ván, cập nhật Status = Finished |
| `Abandon()` | Sprint 2 | Bỏ ván giữa chừng, Status = Abandoned |

**Business rules:**
- `Player1Side` luôn là `1` (hàng dưới) khi tạo, có thể random sau
- Khi `Mode = Solo`, `Player2Id` = null, `AiDifficulty` phải có giá trị
- Khi `Mode = Room`, `RoomId` phải có giá trị
- `Finish()` chỉ được gọi khi `Status = Playing`

---

### `Room` — Aggregate Root

**Vai trò:** Phòng chờ cho chế độ chơi bạn bè. Tồn tại tạm thời — hết hạn sau 2 giờ.

**File:** `src/DanGian.Domain/Game/Room.cs`

| Trường | Kiểu | Ý nghĩa |
|--------|------|---------|
| `Id` | `Guid` | Primary key |
| `RoomCode` | `RoomCode` (VO) | Mã phòng 6 ký tự uppercase alphanumeric — dùng để invite |
| `GameType` | `GameType` (enum) | Loại game sẽ chơi trong phòng |
| `HostId` | `Guid` | FK → User. Người tạo phòng — có quyền start game |
| `Status` | `RoomStatus` (enum) | `Waiting`, `InGame`, `Finished` |
| `MaxPlayers` | `int` | Số người tối đa, default `2` |
| `ExpiresAt` | `DateTime` | Thời điểm phòng hết hạn (tạo + 2 giờ) |

**Methods sẽ thêm khi implement (Sprint 5):**

| Method | Mô tả |
|--------|-------|
| `Create(gameType, hostId)` | Factory — sinh RoomCode, thêm host vào players |
| `Join(userId)` | Thêm người chơi vào phòng |
| `Leave(userId)` | Rời phòng |
| `SetReady(userId, isReady)` | Đánh dấu sẵn sàng |
| `Start()` | Bắt đầu game khi tất cả ready |
| `IsExpired()` | Kiểm tra phòng hết hạn chưa |

**Business rules:**
- `RoomCode` là duy nhất toàn hệ thống tại một thời điểm
- Phòng tự hết hạn sau 2 giờ không hoạt động
- Chỉ `HostId` được gọi `Start()`
- Không thể join phòng khi `Status != Waiting`

---

### `RoomPlayer` — Entity

**Vai trò:** Liên kết User với Room — lưu trạng thái sẵn sàng của từng người trong phòng.

**File:** `src/DanGian.Domain/Game/RoomPlayer.cs`

| Trường | Kiểu | Ý nghĩa |
|--------|------|---------|
| `Id` | `Guid` | Primary key |
| `RoomId` | `Guid` | FK → Room |
| `UserId` | `Guid` | FK → User |
| `JoinedAt` | `DateTime` | Thời điểm vào phòng |
| `IsReady` | `bool` | `true` = người chơi đã bấm "Sẵn sàng" |

---

### `RoomCode` — Value Object

**Vai trò:** Mã phòng 6 ký tự dùng để invite bạn bè. Immutable.

**File:** `src/DanGian.Domain/Game/ValueObjects/RoomCode.cs`

| Thuộc tính | Ý nghĩa |
|------------|---------|
| `Value` | Chuỗi 6 ký tự [A-Z0-9], luôn uppercase |

**Methods sẽ thêm khi implement:**

| Method | Mô tả |
|--------|-------|
| `Generate()` | Sinh ngẫu nhiên 6 ký tự |
| `Create(value)` | Validate + tạo từ chuỗi có sẵn (dùng khi load từ DB) |

---

## Mission Context

### `MissionDefinition` — Aggregate Root

**Vai trò:** Template định nghĩa một loại nhiệm vụ — là seed data, không thay đổi thường xuyên. Ví dụ: "Thắng 3 ván" là 1 definition, áp dụng cho mọi user mỗi ngày.

**File:** `src/DanGian.Domain/Mission/MissionDefinition.cs`

| Trường | Kiểu | Ý nghĩa |
|--------|------|---------|
| `Id` | `Guid` | Primary key |
| `Type` | `string(50)` | Mã duy nhất: `daily_login`, `win_3_games`, `play_with_friend`... |
| `Title` | `string(200)` | Tên hiển thị: "Thắng 3 ván" |
| `Description` | `string?` | Mô tả chi tiết, nullable |
| `RewardPoints` | `int` | Số điểm thưởng khi hoàn thành |
| `Target` | `int` | Ngưỡng hoàn thành. VD: `3` nghĩa là cần làm 3 lần |
| `ResetType` | `ResetType` (enum) | `Daily` / `Weekly` / `Once` — tần suất reset progress |
| `GameType` | `string?` | Giới hạn cho game cụ thể. Null = áp dụng mọi game |
| `IsActive` | `bool` | `false` = nhiệm vụ bị tắt, không hiển thị cho user |

**Methods sẽ thêm khi implement (Sprint 3):**

| Method | Mô tả |
|--------|-------|
| `Create(...)` | Factory — dùng khi seed hoặc admin thêm mission mới |
| `Deactivate()` | Admin tắt mission |

---

### `UserMissionProgress` — Aggregate Root

**Vai trò:** Theo dõi tiến độ của một user trên một mission cụ thể trong một ngày (hoặc tuần). Một bản ghi = 1 user × 1 definition × 1 ngày.

**File:** `src/DanGian.Domain/Mission/UserMissionProgress.cs`

| Trường | Kiểu | Ý nghĩa |
|--------|------|---------|
| `Id` | `Guid` | Primary key |
| `UserId` | `Guid` | FK → User |
| `DefinitionId` | `Guid` | FK → MissionDefinition |
| `Date` | `DateOnly` | Ngày áp dụng (UTC). UNIQUE(UserId, DefinitionId, Date) |
| `Progress` | `int` | Số lần đã hoàn thành hành động. VD: thắng 1/3 ván → Progress = 1 |
| `Status` | `MissionStatus` (enum) | `Pending` → `Completed` → `Claimed` |
| `ClaimedAt` | `DateTime?` | Thời điểm nhận thưởng. Null nếu chưa claim |

**Methods sẽ thêm khi implement (Sprint 3):**

| Method | Mô tả |
|--------|-------|
| `Create(userId, definitionId, target)` | Factory — tạo progress record khi user lần đầu thấy mission |
| `Increment(amount)` | Tăng progress khi user hoàn thành hành động. Tự chuyển sang Completed khi đạt target |
| `Claim(rewardPoints)` | Nhận thưởng — chỉ được gọi khi Status = Completed |

**Business rules:**
- UNIQUE constraint: mỗi (UserId, DefinitionId, Date) chỉ có 1 record
- `Progress` không vượt quá `Target`
- Chỉ claim được khi `Status = Completed`
- Sau khi claim, `Status = Claimed`, không thể claim lại

---

### `PointTransaction` — Entity

**Vai trò:** Audit log bất biến cho mọi thay đổi điểm của user. Không update — chỉ insert. Dùng để debug và reconcile `TotalPoints`.

**File:** `src/DanGian.Domain/Mission/PointTransaction.cs`

| Trường | Kiểu | Ý nghĩa |
|--------|------|---------|
| `Id` | `Guid` | Primary key |
| `UserId` | `Guid` | FK → User |
| `Delta` | `int` | Số điểm thay đổi: dương = cộng, âm = trừ. Không được = 0 |
| `Reason` | `string(100)` | Lý do: `mission_claim`, `game_win`, `penalty`... |
| `ReferenceId` | `Guid?` | FK tùy context: MissionId hoặc SessionId. Nullable |

**Methods sẽ thêm khi implement (Sprint 3):**

| Method | Mô tả |
|--------|-------|
| `Create(userId, delta, reason, referenceId)` | Factory — `delta != 0` là bắt buộc |

---

## Leaderboard Context

### `Season` — Aggregate Root

**Vai trò:** Định nghĩa một mùa thi đấu có thời hạn. Mỗi mùa có bảng xếp hạng riêng snapshot vào cuối mùa.

**File:** `src/DanGian.Domain/Leaderboard/Season.cs`

| Trường | Kiểu | Ý nghĩa |
|--------|------|---------|
| `Id` | `Guid` | Primary key |
| `SeasonNum` | `int` | Số thứ tự mùa: 1, 2, 3... UNIQUE |
| `StartDate` | `DateOnly` | Ngày bắt đầu mùa |
| `EndDate` | `DateOnly` | Ngày kết thúc mùa. Phải > StartDate |
| `IsActive` | `bool` | Chỉ có 1 Season active tại một thời điểm |

**Methods sẽ thêm khi implement (Sprint 4):**

| Method | Mô tả |
|--------|-------|
| `Create(seasonNum, startDate, endDate)` | Factory |
| `Activate()` | Bắt đầu mùa — set IsActive = true |
| `Deactivate()` | Kết thúc mùa |
| `IsOngoing()` | Kiểm tra mùa đang diễn ra |

**Business rules:**
- Chỉ có đúng 1 Season có `IsActive = true` tại một thời điểm
- `EndDate > StartDate`

---

### `SeasonRanking` — Entity

**Vai trò:** Snapshot xếp hạng cuối mùa của từng user. Bất biến sau khi tạo — không update.

**File:** `src/DanGian.Domain/Leaderboard/SeasonRanking.cs`

| Trường | Kiểu | Ý nghĩa |
|--------|------|---------|
| `Id` | `Guid` | Primary key |
| `SeasonId` | `Guid` | FK → Season |
| `UserId` | `Guid` | FK → User |
| `Rank` | `int` | Thứ hạng cuối mùa (1 = cao nhất) |
| `Points` | `int` | Tổng điểm tại thời điểm snapshot |

**Methods sẽ thêm khi implement (Sprint 4):**

| Method | Mô tả |
|--------|-------|
| `Create(seasonId, userId, rank, points)` | Factory — gọi 1 lần khi close season |

---

## Enums

| Enum | Giá trị | Dùng ở |
|------|---------|--------|
| `GameType` | `OAnQuan`, `CoCaro` | GameSession, Room, MissionDefinition |
| `GameMode` | `Solo`, `Ranked`, `Room` | GameSession |
| `GameStatus` | `Playing`, `Finished`, `Abandoned` | GameSession |
| `AiDifficulty` | `Easy`, `Medium`, `Hard` | GameSession |
| `RoomStatus` | `Waiting`, `InGame`, `Finished` | Room |
| `MissionStatus` | `Pending`, `Completed`, `Claimed` | UserMissionProgress |
| `ResetType` | `Daily`, `Weekly`, `Once` | MissionDefinition |

---

## Quy tắc cập nhật document này

| Thay đổi code | Cần cập nhật |
|---------------|-------------|
| Thêm field vào Entity | Thêm dòng vào bảng trường tương ứng |
| Xóa field | Xóa dòng tương ứng |
| Thêm method | Chuyển từ "sẽ thêm" → "hiện có", hoặc thêm mới |
| Xóa method | Xóa hoặc chuyển về "sẽ thêm" |
| Thêm business rule | Thêm vào mục "Business rules" |
| Thêm enum value | Cập nhật bảng Enums |
