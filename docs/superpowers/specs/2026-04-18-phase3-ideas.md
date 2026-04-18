# My Places — Phase 3: Discovery & Social+

**Date:** 2026-04-18  
**Status:** Backlog ideas — chưa design, implement sau Phase 2  
**Phase:** 3

---

## Phase 2 Done Criteria (trigger Phase 3)

- [ ] Collections feature live và đang dùng thực tế
- [ ] CI/CD pipeline hoạt động ổn định
- [ ] App có ít nhất vài người dùng thực tế (không phải chỉ mình)

---

## 1. Discovery / Explore by City

**Problem:** Người dùng mới không có following → feed trống. Cần cách khám phá nội dung public.

**Idea:**
- Trang Explore: filter theo City → hiển thị Places/Trips public từ tất cả users
- Có thể filter thêm theo category (ăn uống, cà phê, tham quan)
- Ranking: newest | most-copied (SourcePlaceId count)

**Open questions:**
- Category/tag system có cần không? (hiện MVP không có tag)
- Có cần moderation không nếu content public?

---

## 2. Notifications

**Problem:** Không biết khi following thêm Place/Trip mới, hoặc khi ai đó copy Place của mình.

**Idea:**
- In-app notifications (bell icon)
- Notification types:
  - `[username] added a new place in [city]`
  - `[username] copied your place [place name]`
  - `[username] started following you`
  - `[username] shared a trip to [city]`
- Read/unread state
- Không cần push notification (browser) cho MVP Phase 3 — chỉ cần polling hoặc SignalR

**Open questions:**
- SignalR real-time hay polling đơn giản?
- Email digest notification? (weekly "your following added X places")

---

## 3. Mobile PWA — Offline Support

**Problem:** Khi du lịch hay có mạng yếu, app không dùng được offline.

**Idea:**
- Blazor WASM PWA manifest (install to homescreen)
- Service Worker cache: cache Places/Trips đã xem
- Offline: có thể xem, không thể tạo mới (rõ ràng thông báo)
- Sync khi có mạng trở lại

**Note:** Blazor WASM hỗ trợ PWA built-in từ .NET 6+. Chỉ cần enable và config Service Worker.

---

## 4. Passkey Authentication (WebAuthn)

**Problem:** Password phức tạp, phải nhớ. Google OAuth phụ thuộc bên thứ ba.

**Idea:**
- Đăng nhập bằng biometric (Touch ID / Face ID) hoặc hardware key
- .NET 10 có built-in WebAuthn support
- Passkey thay thế hoặc bổ sung cho Email/Password

**Open questions:**
- Có cần không nếu Google OAuth đã đủ?
- Device management: nếu mất điện thoại thì fallback thế nào?

---

## 5. Categories / Tags

**Problem:** Hiện tại Place chỉ có Name + Note. Khó filter theo loại (quán ăn, cà phê, bar, tham quan).

**Idea:**
- Predefined categories: 🍜 Ăn uống | ☕ Cà phê | 🍺 Bar | 🏛️ Tham quan | 🛍️ Mua sắm | 🏨 Lưu trú | Khác
- Filter places by category
- Category hiển thị trên card

**Note:** Có thể thêm vào Phase 2 nếu thấy cần sớm. Đơn giản — chỉ là thêm `Category` enum column.

---

## GitHub Milestone

Tạo Milestone `Phase 3: Social+` trên GitHub. Issues sẽ tạo khi bắt đầu Phase 3.
