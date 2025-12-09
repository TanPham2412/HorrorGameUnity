# NoEscapeNoReturn

Một dự án game kinh dị góc nhìn thứ nhất được xây dựng bằng **Unity 2022.3.62f2 (LTS)**. Sử dụng những asset miễn phí. Người chơi khám phá một trường học bỏ hoang, thu thập vật dụng quan trọng, tương tác với thiết bị âm thanh và né tránh các thực thể như Ghost hoặc Principal Boss.

---

## Mục lục
1. [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
2. [Cấu trúc dự án](#cấu-trúc-dự-án)
3. [Các hệ thống chính](#các-hệ-thống-chính)
4. [Cách chạy dự án](#cách-chạy-dự-án)
5. [Quy ước & script đáng chú ý](#quy-ước--script-đáng-chú-ý)
6. [Đóng góp & giấy phép](#đóng-góp--giấy-phép)

---

## Yêu cầu hệ thống
- **Unity**: 2022.3.62f2 (xem `ProjectSettings/ProjectVersion.txt`)
- **Build target mặc định**: Windows (có thể chuyển sang nền tảng khác trong Build Settings)
- **IDE gợi ý**: Rider hoặc Visual Studio với Unity workload
- **Âm thanh**: Các AudioSource như `Run2`, `GhostCry`, `GhostScream`, `ActiveSuccess/Fail` phải được gán clip tương ứng trong scene

---

## Cấu trúc dự án
```
FirstUnityGame/
├─ Assets/
│  ├─ Scripts/
│  │  ├─ Ghost/               # GhostBoss, logic phát hiện/nghe tiếng khóc
│  │  ├─ Principal/           # PrincipalBoss, rượt đuổi + jumpscare
│  │  ├─ Sequences/           # MusicTrigger, RestroomMusicTrigger, cutscene logic
│  │  ├─ Door/                # DoorKey & UI tương tác cửa
│  │  ├─ …                    # Các script gameplay khác (MagicActive, PlayCassette…)
│  ├─ Audio/FX/               # GhostCry, GhostScream, Run2, ActiveSuccess/Fail…
│  └─ Scenes/                 # Scene gameplay chính (hãy mở bằng Unity)
├─ Packages/                  # Unity package manifest & lock file
├─ ProjectSettings/           # Thiết lập build, input, quality…
├─ README.md                  # Tài liệu này
└─ UserSettings/              # Thiết lập cá nhân (không cần commit)
```

---

## Các hệ thống chính
### 1. AI Bosses
- **GhostBoss**: Tuần tra bằng NavMeshAgent, dùng `visionRange` + `visionAngle`. Có hai AudioSource:
  - `ghostCrySource`: phát lặp khi người chơi ở trong `cryRange` mà chưa bị đuổi.
  - `ghostScreamSource`: bật khi ghost thấy người chơi và chase, tắt khi mất dấu.
- **PrincipalBoss**: Có animation controller, chase speed riêng và âm thanh `chaseAudioSource`. Liên kết với `MusicTrigger` để bị vô hiệu hóa khi người chơi bước vào phòng máy.

### 2. Music & Ambient Triggers
- `MusicTrigger`: Delay bật nhạc phòng máy, đóng cửa sau `doorCloseDelaySeconds`, phát tiếng gõ + thoại “Mình phải nhanh lên…” và tự disable Principal.
- `RestroomMusicTrigger`: Vùng âm thanh riêng cho nhà vệ sinh, có thể tự vô hiệu nếu story flag đã hoàn thành.

### 3. Cutscene & Narrative
- `MagicActive`: Kiểm tra các slot vật phẩm, phát monologue thành công/thất bại, phát video (`VideoPlaybackConfig`) và audio kích hoạt/success/failure.
- `ImportantItemsCutscene`: Khi thu thập đủ item sẽ chạy video, khóa camera/UI, phát monologue và ẩn “ghost” inspector sau cutscene.

### 4. Cassette & Audio Logs
- `PlayCassette`: Phát video VHS toàn màn hình, queue monologue, khóa/bật flashlight sau khi xem và chơi audio post-viewing tùy tape.

### 5. Doors & Items
- `DoorKey`: Xử lý UI hành động (ActionDisplay, LockedText), kiểm tra key guard/office, hỗ trợ `ForceCloseAndDisableInteraction` (dùng cho phòng máy).
- Hệ thống inventory (`GlobalInventory`, `SlotImportant`, `PickUpItem`) phối hợp với các puzzle.

---

## Cách chạy dự án
1. **Clone repo** và mở bằng Unity Hub → Unity 2022.3.62f2.
2. Chờ import asset, kiểm tra console lỗi.
3. Mở scene mong muốn (ví dụ scene chính trong `Assets/Scenes/`).
4. Nhấn **Play** trong editor.
5. Build:
   - `File → Build Settings…`
   - Add scene vào “Scenes In Build”.
   - Chọn platform và nhấn **Build**.

### Điều khiển mặc định
| Hành động           | Input (mặc định) |
|---------------------|------------------|
| Di chuyển           | WASD / Joystick  |
| Nhìn quanh          | Chuột            |
| Tương tác (“Action”)| `E`              |

---

## Quy ước & script đáng chú ý
| Script                          | Ghi chú |
|---------------------------------|---------|
| `GhostBoss`, `PrincipalBoss`    | AI tuần tra, chase audio, jumpscare |
| `MusicTrigger`, `RestroomMusicTrigger` | Điều khiển ambient music, door, monologue |
| `MagicActive`                   | Puzzle đặt vật phẩm + video success/fail |
| `PlayCassette`                  | Phát video VHS, khóa flashlight, monologue |
| `ImportantItemsCutscene`       | Cutscene khi đủ item, ẩn ghost object |
| `DoorKey`                       | UI tương tác cửa, ForceClose |

> **Lưu ý**: Project dùng Input Manager cũ. Nếu chuyển sang New Input System cần map lại action tương ứng.
Chúc bạn có trải nghiệm đáng sợ và thú vị khi làm việc với **NoEscapeNoReturn**!
