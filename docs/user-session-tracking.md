# User Session Tracking

## Problem

The Task Management System needed reliable visibility into **when users are in the system**, **how long they stay**, and **how they leave**.

Before this work:

- Login used JWT + refresh cookies, but **no persisted session history** existed.
- Logout (manual or system) was not recorded with times or reasons.
- Admins could not answer questions such as:
  - When did this user log in / out?
  - How many hours were they active today?
  - Was logout manual, expired, inactivity, or forced by an admin?
- After the access token expired, the UI often still allowed **navigating between pages** because auth was only checked on first load.
- When the system ended a session, the Sessions list could show a logout **without clear reason/time data**.

In short: users could appear logged out in the browser while the backend had no clear, auditable record of the session end — and Owners had no day-level activity view.

---

## Goals

1. Track **login time**, **logout time**, and **duration**.
2. Record **why** the session ended (manual vs system reasons).
3. Support **Owner force-logout**.
4. Show an Owner **Sessions** list and a **day storyline** (active vs inactive hours).
5. Stop expired users from continuing to use the app by **checking auth on navigation and token expiry**.

---

## Solution Overview

We introduced a dedicated **`UserSession`** model (not reusing `RefreshTokens` alone) and wired it into auth, inactivity handling, token expiry, and an Owner UI.

```text
Login  → open UserSession (linked to refresh token)
   │
   ├─ Manual logout          → Manual
   ├─ Access/refresh expiry  → TokenExpired (Logged out by system)
   ├─ 10 min inactivity      → InactivityTimeout
   ├─ Owner force logout     → ForcedByAdmin
   └─ Login again while open → ReplacedByNewLogin

Duration = LogoutAt − LoginAt
Total hours = duration in decimal hours
```

Auth sessions are tracked from **login/logout**, not from SignalR connect/disconnect (tab refresh must not create false sessions).

---

## Data Model

### `UserSession`

| Field | Purpose |
|-------|---------|
| `UserId` | Who was logged in |
| `LoginAt` | UTC login time |
| `LogoutAt` | UTC logout time (`null` while still open) |
| `LogoutReason` | Why the session ended |
| `RefreshToken` | Link to the refresh token created at login |
| `IpAddress` / `UserAgent` | Optional audit context |

### `SessionLogoutReason`

| Value | Meaning (UI label) |
|-------|--------------------|
| `Manual` | Manual logout |
| `TokenExpired` | Logged out by system (session expired) |
| `InactivityTimeout` | Logged out by system (inactivity) |
| `ForcedByAdmin` | Logged out by system (admin) |
| `ReplacedByNewLogin` | Replaced by new login |

---

## Backend Behavior

### Token lifetimes

Centralized in `AuthTokenLifetimes`:

| Token | Lifetime |
|-------|----------|
| Access JWT (interactive session) | **10 minutes** |
| Refresh token | **15 minutes** |

Times are stored/compared in **UTC**.

### Session open / close hooks

| Event | Location | Result |
|-------|----------|--------|
| Login | `AuthService.Login` | Opens `UserSession` |
| Manual logout | `AuthService.Logout` | Closes as `Manual` (or `TokenExpired` if already expired) |
| Session expired (client/server) | `POST /auth/session-expired` | Closes as `TokenExpired` |
| Owner force logout | `POST /auth/force-logout/{userId}` | Invalidates tokens, closes as `ForcedByAdmin`, SignalR `ForceLogout` |
| Inactivity (~10 min disconnected) | `UserConnectionService` | Closes as `InactivityTimeout`, SignalR `ForceLogout` |
| Expiry sweeper / list load | `CloseExpiredSessionsAsync` | Closes open sessions past access/refresh expiry as `TokenExpired` with accurate `LogoutAt` |

### Main APIs (Owner)

| Endpoint | Purpose |
|----------|---------|
| `GET /auth/sessions` | Paged session list (filters: from/to, reason) |
| `GET /auth/sessions/day?userId=&date=` | Day storyline: active/inactive segments + session rows |
| `POST /auth/force-logout/{userId}` | Force end a user’s session |

---

## Frontend Behavior

### Auth gate (expired session fix)

Previously auth was only checked on first app load, so an expired token still allowed route changes.

Now `Auth` + `Auth.ts`:

- Parse JWT `exp` and treat expired tokens as invalid.
- **Abort navigation** if the token is expired.
- **Auto-logout timer** when the JWT expires (even on the same page).
- Re-check when the browser tab becomes visible again.
- On `401` / expiry, call `POST /auth/session-expired` so the server records a system logout.

### Owner UI

1. **Sessions** (`/sessions`)  
   - Columns: User, Login, Logout, Duration, Total Hours, Reason  
   - Click user/row → day detail page  

2. **Day storyline** (`/sessions/{userId}?date=yyyy-MM-dd`)  
   - Active / inactive hours summary  
   - 24-hour bar (green = active, gray = inactive)  
   - Hover popup with exact time under cursor, segment range, duration, reason  
   - Vertical timeline + sessions table for that day  
   - Date picker to change day  

3. **Force Logout**  
   - Owner opens **Sessions** (`/sessions`)  
   - Uses **Active only** filter (optional) to list open sessions  
   - Clicks **Force Logout** on an active session row  
   - Confirms → user is logged out immediately (if connected) and session is closed with admin reason  

---

## How “logged out by system” is decided

A user is considered **logged out by the system** when any of these happen:

1. **Session expired** — access token lifetime (10 min from login) or refresh token expiry is reached; session closed as `TokenExpired`.
2. **Inactivity** — SignalR disconnected long enough (~10 min); session closed as `InactivityTimeout`.
3. **Admin force logout** — Owner uses Force Logout; session closed as `ForcedByAdmin`.

Manual logout (sidebar **Logout**) is recorded separately as `Manual`.

---

## Key Files

| Area | Path |
|------|------|
| Entity | `AutomatedTaskSystem/Models/UserSession.cs` |
| Reason enum | `AutomatedTaskSystem/Models/Enums/SessionLogoutReason.cs` |
| Session service | `AutomatedTaskSystem/Services/SessionTracking/` |
| Auth hooks | `AutomatedTaskSystem/Services/Auth/AuthService.cs` |
| Token lifetimes | `AutomatedTaskSystem/Services/Token/AuthTokenLifetimes.cs` |
| Migration | `AutomatedTaskSystem/Migrations/*_AddUserSessions.cs` |
| Auth client | `AutomatedTaskSystem.UI/client/src/lib/Auth.ts` |
| Auth gate | `AutomatedTaskSystem.UI/client/src/components/auth/index.tsx` |
| Sessions API | `AutomatedTaskSystem.UI/client/src/lib/API/sessions.ts` |
| Sessions list | `AutomatedTaskSystem.UI/client/src/pages/sessions/index.tsx` |
| Day storyline | `AutomatedTaskSystem.UI/client/src/pages/sessions/[userId]/index.tsx` |

---

## Why users were logged out while using the app

Access JWT lasts **10 minutes**. The UI previously **never called** `/auth/refresh-token`, and also **forced logout** when the JWT expired. So after ~10 minutes of use you were kicked to login even though you were active.

**Fix:** silent refresh behind the scenes (~1 minute before expiry). While you keep using the app, refresh renews both access JWT and refresh cookie (15‑minute sliding window). You are only logged out if refresh fails (idle longer than the refresh window, force logout, etc.).

---

## Outcome

Owners can now:

- See who logged in/out, for how long, and why  
- Force-logout a user  
- Inspect a user’s day as an active/inactive storyline  

The system consistently records system-driven logouts (expiry, inactivity, admin) with logout time, duration, and a clear reason — and expired users can no longer keep navigating the app.
