# API Contract — Dân Gian Platform

> **Nguồn sự thật duy nhất giữa Frontend và Backend.**
> Backend implement theo file này. Frontend gọi theo file này.
> Mọi thay đổi API phải cập nhật file này trước, sau đó mới code.

---

## Quy ước chung

### Base URL
```
Production:  https://api.dangian.app/api/v1
Development: http://localhost:5000/api/v1
```

### Authentication
Tất cả endpoint (trừ `/auth/*`) yêu cầu header:
```
Authorization: Bearer <access_token>
```

### Response chuẩn

**Thành công:**
```json
{
  "success": true,
  "data": { },
  "meta": {
    "timestamp": "2026-04-27T09:00:00Z"
  }
}
```

**Lỗi:**
```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Mô tả lỗi cho developer",
    "details": { }
  }
}
```

### Error codes chuẩn

| Code | HTTP Status | Ý nghĩa |
|------|-------------|---------|
| `UNAUTHORIZED` | 401 | Token thiếu hoặc không hợp lệ |
| `FORBIDDEN` | 403 | Không có quyền |
| `NOT_FOUND` | 404 | Resource không tồn tại |
| `VALIDATION_ERROR` | 422 | Dữ liệu đầu vào không hợp lệ |
| `RATE_LIMITED` | 429 | Quá nhiều request |
| `INTERNAL_ERROR` | 500 | Lỗi server |
| `GAME_NOT_FOUND` | 404 | Ván đấu không tồn tại |
| `INVALID_MOVE` | 422 | Nước đi không hợp lệ |
| `ROOM_FULL` | 409 | Phòng đã đủ người |
| `ROOM_NOT_FOUND` | 404 | Mã phòng không tồn tại |
| `MISSION_ALREADY_CLAIMED` | 409 | Nhiệm vụ đã nhận thưởng |

---

## Auth Service

### POST `/auth/zalo`
Đổi Zalo auth code lấy JWT.

**Request:**
```json
{
  "code": "string",        // auth code từ Zalo JS SDK
  "codeVerifier": "string" // PKCE code verifier
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "accessToken": "string",   // JWT, hết hạn sau 15 phút
    "refreshToken": "string",  // hết hạn sau 7 ngày
    "expiresIn": 900,          // seconds
    "user": {
      "id": "uuid",
      "displayName": "string",
      "avatarUrl": "string",
      "totalPoints": 0,
      "rankTitle": "Thôn"
    }
  }
}
```

**Errors:** `VALIDATION_ERROR`, `INTERNAL_ERROR`

---

### POST `/auth/refresh`
Làm mới access token.

**Request:**
```json
{
  "refreshToken": "string"
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "accessToken": "string",
    "expiresIn": 900
  }
}
```

**Errors:** `UNAUTHORIZED` (token hết hạn hoặc không hợp lệ)

---

### POST `/auth/logout`
Vô hiệu hóa refresh token.

**Request:** _(body rỗng, dùng Authorization header)_

**Response 200:**
```json
{ "success": true, "data": null }
```

---

## User Service

### GET `/users/me`
Lấy thông tin user hiện tại.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "displayName": "string",
    "avatarUrl": "string",
    "totalPoints": 1240,
    "rankTitle": "Làng xã",
    "stats": {
      "totalGames": 42,
      "wins": 28,
      "losses": 14,
      "winRate": 0.667
    },
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

---

### PATCH `/users/me`
Cập nhật thông tin user (chỉ các field cho phép).

**Request:**
```json
{
  "displayName": "string"  // optional, max 50 ký tự
}
```

**Response 200:** trả về user object như GET `/users/me`

---

## Game Service

### GET `/games`
Danh sách game khả dụng trên nền tảng.

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "type": "o_an_quan",
      "name": "Ô Ăn Quan",
      "description": "Trò chơi dân gian chiến thuật",
      "status": "available",   // available | coming_soon
      "modes": ["solo", "ranked", "room"],
      "minPlayers": 2,
      "maxPlayers": 2,
      "thumbnailUrl": "string"
    },
    {
      "type": "co_caro",
      "name": "Cờ Caro",
      "status": "coming_soon",
      "modes": ["solo", "ranked", "room"],
      "minPlayers": 2,
      "maxPlayers": 2,
      "thumbnailUrl": "string"
    }
  ]
}
```

---

### POST `/games/{gameType}/solo`
Bắt đầu ván solo chơi với AI.

**Path params:** `gameType` = `o_an_quan` | `co_caro`

**Request:**
```json
{
  "difficulty": "easy" // easy | medium | hard
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "sessionId": "uuid",
    "gameType": "o_an_quan",
    "mode": "solo",
    "initialState": { },   // GameState object — xem GAME_RULES.md
    "playerSide": 1,       // 1 = hàng dưới, 2 = hàng trên
    "aiDifficulty": "easy",
    "createdAt": "2026-04-27T09:00:00Z"
  }
}
```

---

### POST `/games/{gameType}/ranked`
Vào hàng đợi matchmaking xếp hạng.

**Request:** _(body rỗng)_

**Response 200:**
```json
{
  "success": true,
  "data": {
    "queueId": "uuid",
    "estimatedWaitSeconds": 30,
    "status": "queued"  // queued | matched
  }
}
```

> Khi match thành công, server gửi Socket.IO event `game:start`.

---

### DELETE `/games/ranked/queue`
Rời hàng đợi matchmaking.

**Response 200:**
```json
{ "success": true, "data": null }
```

---

### POST `/games/{gameType}/rooms`
Tạo phòng chơi bạn bè.

**Request:** _(body rỗng)_

**Response 201:**
```json
{
  "success": true,
  "data": {
    "roomId": "uuid",
    "roomCode": "ABC123",   // 6 ký tự, dùng để invite
    "gameType": "o_an_quan",
    "hostId": "uuid",
    "players": [
      {
        "id": "uuid",
        "displayName": "string",
        "avatarUrl": "string",
        "isHost": true,
        "isReady": false
      }
    ],
    "status": "waiting",    // waiting | playing | finished
    "maxPlayers": 2,
    "createdAt": "string"
  }
}
```

---

### POST `/games/{gameType}/rooms/{roomCode}/join`
Tham gia phòng bằng mã.

**Request:** _(body rỗng)_

**Response 200:** trả về Room object như POST `/rooms`

**Errors:** `ROOM_NOT_FOUND`, `ROOM_FULL`

---

### GET `/games/sessions/{sessionId}`
Lấy trạng thái ván đấu hiện tại (dùng khi reconnect).

**Response 200:**
```json
{
  "success": true,
  "data": {
    "sessionId": "uuid",
    "gameType": "o_an_quan",
    "mode": "solo",
    "playerSide": 1,
    "currentState": { },   // GameState object
    "status": "playing",   // playing | finished
    "result": null         // null nếu đang chơi
  }
}
```

---

### GET `/games/history`
Lịch sử ván đấu của user.

**Query params:**
- `page` (default: 1)
- `limit` (default: 20, max: 50)
- `gameType` (optional)

**Response 200:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "sessionId": "uuid",
        "gameType": "o_an_quan",
        "mode": "solo",
        "result": "win",       // win | loss | draw
        "myScore": 35,
        "opponentScore": 35,
        "pointsEarned": 50,
        "duration": 180,       // seconds
        "playedAt": "string"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "total": 42,
      "totalPages": 3
    }
  }
}
```

---

## Mission Service

### GET `/missions/daily`
Danh sách nhiệm vụ hôm nay.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "date": "2026-04-27",
    "totalPoints": 380,
    "earnedPoints": 150,
    "missions": [
      {
        "id": "uuid",
        "type": "daily_login",
        "title": "Đăng nhập hôm nay",
        "description": "Đăng nhập vào game",
        "reward": 50,
        "progress": 1,
        "target": 1,
        "status": "completed",  // pending | in_progress | completed | claimed
        "claimedAt": "string"
      },
      {
        "id": "uuid",
        "type": "win_games",
        "title": "Thắng 3 ván Ô Ăn Quan",
        "description": "Thắng 3 ván bất kỳ",
        "reward": 150,
        "progress": 1,
        "target": 3,
        "status": "in_progress",
        "claimedAt": null
      }
    ]
  }
}
```

---

### POST `/missions/{missionId}/claim`
Nhận thưởng nhiệm vụ đã hoàn thành.

**Request:** _(body rỗng)_

**Response 200:**
```json
{
  "success": true,
  "data": {
    "missionId": "uuid",
    "reward": 150,
    "newTotalPoints": 1390,
    "newRankTitle": "Làng xã"   // null nếu không lên hạng
  }
}
```

**Errors:** `MISSION_ALREADY_CLAIMED`, `NOT_FOUND`

---

## Leaderboard Service

### GET `/leaderboard/weekly`
Bảng xếp hạng tuần hiện tại.

**Query params:**
- `limit` (default: 20, max: 100)

**Response 200:**
```json
{
  "success": true,
  "data": {
    "weekStart": "2026-04-21",
    "weekEnd": "2026-04-27",
    "entries": [
      {
        "rank": 1,
        "userId": "uuid",
        "displayName": "string",
        "avatarUrl": "string",
        "points": 4820,
        "isCurrentUser": false
      }
    ],
    "currentUserEntry": {
      "rank": 5,
      "points": 1240,
      "isCurrentUser": true
    }
  }
}
```

---

### GET `/leaderboard/season`
Bảng xếp hạng mùa hiện tại.

**Response 200:** cấu trúc tương tự `/leaderboard/weekly`, thêm:
```json
{
  "data": {
    "season": 3,
    "seasonEnd": "2026-06-30",
    "entries": [ ]
  }
}
```

---

## Socket.IO — Realtime Events

### Kết nối
```
URL: wss://api.dangian.app
Path: /socket.io
Auth: { token: "<access_token>" }  // trong handshake query
```

### Namespace: `/game`

---

### Client → Server events

#### `room:join`
```typescript
socket.emit('room:join', {
  roomId: string
})
```

#### `room:ready`
```typescript
socket.emit('room:ready', {
  roomId: string
})
```

#### `room:leave`
```typescript
socket.emit('room:leave', {
  roomId: string
})
```

#### `game:move`
```typescript
socket.emit('game:move', {
  sessionId: string,
  move: {
    cellIndex: number,           // 0-4 (hàng của mình)
    direction: 'clockwise' | 'counterclockwise'
  }
}, (response: MoveResponse) => {
  // callback xác nhận từ server
})
```

#### `ranked:queue`
```typescript
socket.emit('ranked:queue', {
  gameType: string
})
```

---

### Server → Client events

#### `room:updated`
```typescript
socket.on('room:updated', (data: {
  roomId: string,
  players: Player[],
  status: 'waiting' | 'playing' | 'finished'
}) => { })
```

#### `game:start`
```typescript
socket.on('game:start', (data: {
  sessionId: string,
  gameType: string,
  playerSide: 1 | 2,
  opponent: {
    id: string,
    displayName: string,
    avatarUrl: string
  },
  initialState: GameState
}) => { })
```

#### `game:state`
```typescript
socket.on('game:state', (data: {
  sessionId: string,
  state: GameState,
  lastMove: MoveResult
}) => { })
```

#### `game:end`
```typescript
socket.on('game:end', (data: {
  sessionId: string,
  result: 'win' | 'loss' | 'draw',
  myScore: number,
  opponentScore: number,
  pointsEarned: number,
  missionUpdates: MissionUpdate[]
}) => { })
```

#### `ranked:matched`
```typescript
socket.on('ranked:matched', (data: {
  sessionId: string,
  opponent: Player
}) => { })
```

#### `error`
```typescript
socket.on('error', (data: {
  code: string,
  message: string
}) => { })
```

---

## GameState Schema

> Dùng chung giữa BE và FE. Xem chi tiết luật trong `GAME_RULES.md`.

```typescript
interface GameState {
  cells: number[]        // 10 phần tử: index 0-4 hàng player1, 5-9 hàng player2
  quantLeft: number      // viên trong ô quan trái
  quantRight: number     // viên trong ô quan phải
  player1Score: number   // viên player1 đã ăn
  player2Score: number   // viên player2 đã ăn
  currentPlayer: 1 | 2
  status: 'playing' | 'finished'
  winner: 1 | 2 | 'draw' | null
  lastMove: MoveResult | null
  turnNumber: number
}

interface MoveResult {
  playerId: string
  cellIndex: number
  direction: 'clockwise' | 'counterclockwise'
  capturedCells: number[]
  capturedCount: number
  chainCaptures: number
}

interface MissionUpdate {
  missionId: string
  type: string
  progress: number
  target: number
  completed: boolean
  pointsEarned: number
}

interface Player {
  id: string
  displayName: string
  avatarUrl: string
  isHost?: boolean
  isReady?: boolean
}
```
