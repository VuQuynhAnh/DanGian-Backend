# Game Rules — Ô Ăn Quan

> Tài liệu này là input chính cho việc implement game logic.  
> Mọi edge case đều phải được test theo đặc tả ở đây.

---

## Cấu trúc bàn chơi

```
┌──────┬─────┬─────┬─────┬─────┬─────┬──────┐
│      │  5  │  4  │  3  │  2  │  1  │      │
│ QUAN │ [A5]│ [A4]│ [A3]│ [A2]│ [A1]│ QUAN │
│  L   │     │     │     │     │     │  R   │
│      │ [B1]│ [B2]│ [B3]│ [B4]│ [B5]│      │
│      │  1  │  2  │  3  │  4  │  5  │      │
└──────┴─────┴─────┴─────┴─────┴─────┴──────┘

Player 2 (AI / Opponent): hàng A — chơi từ phải sang trái (A1→A5)
Player 1 (Bạn): hàng B — chơi từ trái sang phải (B1→B5)
QUAN L = Ô quan trái (lớn)
QUAN R = Ô quan phải (lớn)
```

**Số viên đá ban đầu:**
- Mỗi ô thường (10 ô): 5 viên
- Mỗi ô quan (2 ô): 10 viên
- Tổng: 10 × 5 + 2 × 10 = 70 viên

---

## Luật chơi cơ bản

### 1. Bắt đầu lượt
Player chọn 1 trong các ô **không rỗng** ở hàng của mình.

### 2. Rải đá
- Lấy **toàn bộ** số viên trong ô đã chọn
- Rải lần lượt, **mỗi ô 1 viên**, theo chiều đã chọn (thuận hoặc nghịch chiều kim đồng hồ)
- Bắt đầu từ ô **kế tiếp** (không rải vào ô vừa chọn)
- Rải qua **tất cả các ô** kể cả ô quan và ô của đối thủ

### 3. Tiếp tục hoặc ăn

**Trường hợp A — Ô cuối có viên đá:**
- Lấy toàn bộ viên ở ô cuối + tiếp tục rải

**Trường hợp B — Ô cuối rỗng, ô kế tiếp có viên (ăn):**
- Nếu ô cuối **rỗng** VÀ ô **kế tiếp** có viên → ăn hết viên ở ô kế tiếp vào kho của mình
- Nếu ô kế tiếp đó cũng rỗng nhưng ô tiếp theo nữa có viên → tiếp tục ăn
- Cứ như vậy cho đến khi gặp ô có viên hoặc hết bàn

**Trường hợp C — Kết thúc lượt:**
- Ô cuối rỗng VÀ ô kế tiếp cũng rỗng → **hết lượt**
- Hoặc ô cuối là ô quan có viên → **không ăn, hết lượt**

### 4. Ăn quan (đặc biệt)
- Nếu ô cuối rỗng và ô kế tiếp là **ô quan** có viên → ăn hết viên trong ô quan đó
- Ô quan ăn được sẽ **không còn viên** (ô quan trở thành rỗng)

---

## Điều kiện kết thúc ván

### Kết thúc thông thường
Ván kết thúc khi **một bên hết viên** trong 5 ô của mình.

### Xử lý khi hết viên
Người chơi còn viên sẽ **ăn hết** số viên còn lại trên bàn (kể cả ô đối thủ, nhưng không tính ô quan của đối thủ chưa được ăn).

**Quy tắc ô quan chưa ăn:**
- Ô quan nào **chưa bị ăn** khi ván kết thúc → viên trong đó không thuộc về ai
- Giữ lại cho ván sau hoặc tính theo thỏa thuận địa phương

> **Quyết định implement:** Dự án này sẽ dùng luật: ô quan chưa ăn → viên trả về pool, **không** tính vào điểm của ai.

---

## Tính điểm

```
Điểm mỗi ô thường = 1 điểm/viên
Điểm ô quan = 10 điểm/viên (hoặc tính theo số viên × 1)
```

> **Quyết định implement:** Dự án dùng cách tính đơn giản — **1 viên = 1 điểm** (kể cả viên trong ô quan). Ô quan ban đầu có 10 viên = 10 điểm nếu ăn được.

**Ai thắng:** người có **tổng điểm cao hơn** khi ván kết thúc.

**Hòa:** tổng điểm bằng nhau.

---

## Edge Cases quan trọng

### Edge Case 1: Rải đến hết bàn
Khi đang rải, đến cuối bàn → **quay vòng** tiếp tục từ đầu bàn (kể cả ô vừa chọn ban đầu nếu có viên rải qua).

### Edge Case 2: Chỉ còn 1 viên
Chọn ô có 1 viên → rải vào ô kế tiếp → xử lý bình thường theo trường hợp A/B/C.

### Edge Case 3: Ô cuối là ô quan có viên
→ Không ăn (ô quan chặn chuỗi ăn), kết thúc lượt.

### Edge Case 4: Ăn chuỗi (chain capture)
```
Ví dụ: ô cuối rỗng → ô kế = rỗng → ô kế nữa có viên → ăn
Ô kế đó ăn xong → ô tiếp theo lại rỗng → ô kế đó có viên → ăn tiếp
```
Chuỗi ăn chỉ dừng khi gặp: 2 ô rỗng liên tiếp HOẶC ô quan có viên HOẶC hết bàn.

### Edge Case 5: Bàn trống về phía người chơi
Người chơi không còn viên ở hàng của mình → phải **bỏ lượt** hoặc ván kết thúc (xem điều kiện kết thúc).

---

## State của game (dùng cho implementation)

```typescript
interface GameState {
  // Bàn chơi: index 0-4 = hàng player1, index 5-9 = hàng player2
  // Đọc theo chiều: player1 B1,B2,B3,B4,B5 | player2 A5,A4,A3,A2,A1
  cells: number[];        // 10 phần tử, số viên mỗi ô
  quantLeft: number;      // Viên trong ô quan trái
  quantRight: number;     // Viên trong ô quan phải
  player1Score: number;   // Viên player 1 đã ăn được
  player2Score: number;   // Viên player 2 đã ăn được
  currentPlayer: 1 | 2;
  direction: 'clockwise' | 'counterclockwise';
  status: 'playing' | 'finished';
  winner: 1 | 2 | 'draw' | null;
  lastMove: MoveResult | null;
}

interface MoveResult {
  selectedCell: number;
  direction: 'clockwise' | 'counterclockwise';
  capturedCells: number[];  // Index các ô đã ăn
  capturedCount: number;    // Tổng số viên ăn được
  chainCaptures: number;    // Số lần ăn chuỗi
  gameEnded: boolean;
}
```

---

## AI Strategy (Solo Mode)

### Level 1 — Tập sự (Random)
Chọn ngẫu nhiên ô hợp lệ.

### Level 2 — Làng xã (Greedy)
Chọn nước đi ăn được nhiều viên nhất (look-ahead 1 lượt).

### Level 3 — Quan lớn (Minimax)
Dùng thuật toán Minimax depth-3 với alpha-beta pruning.

**Heuristic function:**
```
score = player_score - opponent_score 
      + (cells_with_stones * 0.5)     // Linh hoạt
      + (quan_cells_near * 2)          // Gần ô quan có giá trị cao
```

---

## Test Cases bắt buộc

```
TC001: Rải bình thường — không ăn
TC002: Rải đến ô cuối rỗng → ăn 1 ô
TC003: Rải → ăn chuỗi 2 ô liên tiếp  
TC004: Rải → ăn ô quan
TC005: Ô cuối rỗng → ô kế là ô quan có viên → không ăn
TC006: Rải đến hết bàn → quay vòng
TC007: Hàng người chơi hết viên → kết thúc ván
TC008: Tính điểm cuối ván đúng
TC009: Oan kết thúc → ô quan chưa ăn không tính điểm
TC010: Hòa (điểm bằng nhau)
TC011: AI level 1 chọn ô hợp lệ
TC012: Không thể chọn ô rỗng
```
