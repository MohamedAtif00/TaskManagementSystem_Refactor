# Sprint: Role-Based Home Page Dashboard

## Sprint Overview

**Sprint Goal:**  
Enable role-based home page dashboard where each authenticated user sees dashboard content, widgets, and navigation links relevant to their assigned role.

**Sprint Duration:** 1-2 weeks  
**Sprint Type:** Feature Development

---

## User Story

**As an authenticated user,**  
**I want the home page dashboard to display content and widgets based on my assigned role**  
**So that I only see information and quick actions relevant to my responsibilities and permissions in the system.**

---

## Role Matrix for Home Page

| Role | Role ID | Dashboard Components | Quick Actions Available |
|------|---------|---------------------|------------------------|
| **Owner** | 4 | Full dashboard with all metrics, charts, and reports | All navigation links visible |
| **Coordinator** (ProjectManager) | 0 | Same as Owner dashboard | Same as Owner (except "My Leaves" behavior differs) |
| **Section Head** | 1 | Tasks-focused dashboard, User Tasks metrics, Advanced Reports | Tasks, User Tasks, Advanced Report, My Leaves |
| **Team Leader** | 2 | Team-focused dashboard with member metrics, project tasks | Tasks, User Tasks, Advanced Report, My Leaves |
| **Member** | 3 | Personal tasks dashboard, minimal metrics | Tasks, My Leaves only |

---

## Acceptance Criteria

### ✅ Owner (Role 4)
- [ ] Sees full Project Manager dashboard with:
  - Users count
  - Projects count
  - Schemas count
  - Active Tasks count
  - Learning Objective Statuses pie chart
  - User count in Group bar chart
  - Learning Objective statuses in Project stacked bar chart
- [ ] Can access all quick navigation links from home page
- [ ] Dashboard loads correctly after login

### ✅ Coordinator (Role 0 - ProjectManager)
- [ ] Sees same dashboard as Owner
- [ ] All metrics and charts visible
- [ ] "My Leaves" behavior differs from Owner (backend logic)
- [ ] Dashboard loads correctly after login

### ✅ Section Head (Role 1)
- [ ] Sees role-appropriate dashboard (needs new dashboard component or modified existing)
- [ ] Can see:
  - Tasks metrics
  - User Tasks metrics
  - Advanced Report access
  - My Leaves
- [ ] Cannot see:
  - Full reports dashboard
  - Admin-level metrics
- [ ] Dashboard loads correctly after login

### ✅ Team Leader (Role 2)
- [ ] Sees Team Leader dashboard with:
  - Members count
  - Active Projects count
  - Projects with Tasks count
  - Active Tasks count
  - Tasks Per Project bar chart
  - Tasks Per User bar chart
  - Projects Navigation links
- [ ] Can access Tasks, User Tasks, Advanced Report, My Leaves
- [ ] Dashboard loads correctly after login

### ✅ Member (Role 3)
- [ ] Sees simplified dashboard (needs new dashboard component)
- [ ] Can see:
  - Personal tasks metrics
  - My Leaves access
- [ ] Cannot see:
  - User Tasks
  - Reports
  - Advanced Reports
  - Team/Project management metrics
- [ ] Dashboard loads correctly after login

---

## Technical Requirements

### Backend APIs Required

1. **POST /auth/about-me**
   - Returns user profile including role
   - Used to determine which dashboard to load

2. **GET /dashboards/project-manager**
   - Returns `ProjectManagerDashboard` data
   - For Owner (4) and Coordinator (0)

3. **GET /dashboards/team-leader**
   - Returns `TeamLeaderDashboard` data
   - For Team Leader (2)

4. **GET /dashboards/section-head** (NEW - to be created)
   - Returns `SectionHeadDashboard` data
   - For Section Head (1)

5. **GET /dashboards/member** (NEW - to be created)
   - Returns `MemberDashboard` data
   - For Member (3)

### Frontend Components

1. **Home Page (`pages/index.tsx`)**
   - Role-based conditional rendering
   - Loads appropriate dashboard component based on role

2. **Dashboard Components:**
   - `ProjectManagerDashboard` (existing) - Roles 0, 4
   - `TeamLeaderDashboard` (existing) - Role 2
   - `SectionHeadDashboard` (NEW) - Role 1
   - `MemberDashboard` (NEW) - Role 3

3. **Auth State Management:**
   - `authSlice` stores role from login
   - Role loaded from `/auth/about-me` on app initialization

---

## Sprint Backlog

### Backend Tasks

- [ ] **B1:** Create `GET /dashboards/section-head` endpoint
  - Returns section head specific metrics
  - Includes: tasks count, user tasks metrics, advanced report access indicators
  - **Estimate:** 3 points

- [ ] **B2:** Create `GET /dashboards/member` endpoint
  - Returns member-specific dashboard data
  - Includes: personal tasks count, my leaves summary
  - **Estimate:** 2 points

- [ ] **B3:** Verify `/auth/about-me` returns role correctly
  - Ensure role is included in response
  - **Estimate:** 1 point

### Frontend Tasks

- [ ] **F1:** Update `pages/index.tsx` with complete role-based rendering
  - Map all roles (0, 1, 2, 3, 4) to correct dashboard components
  - Handle loading states
  - Handle error states
  - **Estimate:** 3 points

- [ ] **F2:** Create `SectionHeadDashboard` component
  - Design dashboard layout for Section Head role
  - Include tasks metrics, user tasks metrics
  - Add quick navigation to Advanced Report
  - **Estimate:** 5 points

- [ ] **F3:** Create `MemberDashboard` component
  - Design simplified dashboard for Member role
  - Include personal tasks summary
  - Add quick link to My Leaves
  - **Estimate:** 4 points

- [ ] **F4:** Create API methods for new dashboard endpoints
  - Add `GET_SECTION_HEAD_DB()` to `lib/API/dashboard.ts`
  - Add `GET_MEMBER_DB()` to `lib/API/dashboard.ts`
  - Add TypeScript interfaces for new dashboard types
  - **Estimate:** 2 points

- [ ] **F5:** Add loading and error handling to all dashboard components
  - Consistent loading spinner
  - Error message display
  - **Estimate:** 2 points

- [ ] **F6:** Test role-based dashboard rendering
  - Test each role (0, 1, 2, 3, 4)
  - Verify correct dashboard loads
  - Verify no unauthorized content visible
  - **Estimate:** 3 points

### Testing Tasks

- [ ] **T1:** Manual testing for each role
  - Login as each role
  - Verify correct dashboard displays
  - Verify no errors in console
  - **Estimate:** 3 points

- [ ] **T2:** Integration testing
  - Test dashboard API calls
  - Test role loading from auth
  - **Estimate:** 2 points

---

## Definition of Done

- [ ] All 5 roles (Owner, Coordinator, Section Head, Team Leader, Member) have appropriate dashboards
- [ ] Home page correctly renders dashboard based on user role
- [ ] No user sees dashboard content outside their role permissions
- [ ] All dashboard components have loading states
- [ ] All dashboard components have error handling
- [ ] Backend APIs return correct data for each role
- [ ] No console errors when loading dashboards
- [ ] Dashboard loads correctly immediately after login
- [ ] Code is reviewed and merged

---

## User Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    USER FLOW: ROLE-BASED HOME PAGE              │
└─────────────────────────────────────────────────────────────────┘

START
  │
  ├─► User opens application
  │
  ├─► [Not Authenticated?]
  │   │
  │   ├─► YES ──► Redirect to Login Page
  │   │            │
  │   │            └─► User enters credentials
  │   │                │
  │   │                └─► POST /auth/login
  │   │                    │
  │   │                    └─► Receive token + role
  │   │                        │
  │   │                        └─► Store in authSlice
  │   │
  │   └─► NO ──► Continue
  │
  ├─► Load Home Page (/)
  │
  ├─► Check authSlice.role
  │
  ├─► [Role = 4 (Owner)?]
  │   │
  │   ├─► YES ──► Load ProjectManagerDashboard
  │   │            │
  │   │            └─► GET /dashboards/project-manager
  │   │                │
  │   │                └─► Display: Users, Projects, Schemas, Active Tasks
  │   │                    Charts: Learning Objectives, Groups, Projects
  │   │
  │   └─► NO ──► Continue
  │
  ├─► [Role = 0 (Coordinator/ProjectManager)?]
  │   │
  │   ├─► YES ──► Load ProjectManagerDashboard
  │   │            │
  │   │            └─► GET /dashboards/project-manager
  │   │                │
  │   │                └─► Display: Same as Owner
  │   │                    Note: "My Leaves" behavior differs
  │   │
  │   └─► NO ──► Continue
  │
  ├─► [Role = 1 (Section Head)?]
  │   │
  │   ├─► YES ──► Load SectionHeadDashboard
  │   │            │
  │   │            └─► GET /dashboards/section-head
  │   │                │
  │   │                └─► Display: Tasks metrics, User Tasks metrics
  │   │                    Quick links: Tasks, User Tasks, Advanced Report, My Leaves
  │   │
  │   └─► NO ──► Continue
  │
  ├─► [Role = 2 (Team Leader)?]
  │   │
  │   ├─► YES ──► Load TeamLeaderDashboard
  │   │            │
  │   │            └─► GET /dashboards/team-leader
  │   │                │
  │   │                └─► Display: Members, Projects, Active Tasks
  │   │                    Charts: Tasks Per Project, Tasks Per User
  │   │                    Navigation: Project links
  │   │
  │   └─► NO ──► Continue
  │
  ├─► [Role = 3 (Member)?]
  │   │
  │   ├─► YES ──► Load MemberDashboard
  │   │            │
  │   │            └─► GET /dashboards/member
  │   │                │
  │   │                └─► Display: Personal tasks summary
  │   │                    Quick links: Tasks, My Leaves
  │   │
  │   └─► NO ──► Display Error / Empty State
  │
  └─► END (Dashboard Rendered)
```

---

## Sequence Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│          SEQUENCE FLOW: USER LOGS IN AND VIEWS ROLE-BASED HOME PAGE        │
└─────────────────────────────────────────────────────────────────────────────┘

User          Frontend          Backend          Auth Service      Redux Store
 │                │                 │                  │                 │
 │                │                 │                  │                 │
 │───Login───────►│                 │                  │                 │
 │  (credentials) │                 │                  │                 │
 │                │                 │                  │                 │
 │                │──POST /auth/login─────────────────►│                 │
 │                │  { code: "..." }                   │                 │
 │                │                 │                  │                 │
 │                │                 │──Validate───────►│                 │
 │                │                 │  credentials      │                 │
 │                │                 │                  │                 │
 │                │                 │◄───Token + Role───│                 │
 │                │                 │  { token, role }  │                 │
 │                │                 │                  │                 │
 │                │◄───Response─────│                  │                 │
 │                │  { token, role }│                  │                 │
 │                │                 │                  │                 │
 │                │──Store Token────┼───────────────────────────────────►│
 │                │  in localStorage│                  │                 │
 │                │                 │                  │                 │
 │                │──Dispatch login─────────────────────────────────────►│
 │                │  { name, role, id, group }         │                 │
 │                │                 │                  │                 │
 │                │                 │                  │                 │◄───Store Role
 │                │                 │                  │                 │
 │                │                 │                  │                 │
 │───Navigate─────►│                 │                  │                 │
 │  to Home (/)    │                 │                  │                 │
 │                │                 │                  │                 │
 │                │──Get Role from Redux Store─────────────────────────►│
 │                │                 │                  │                 │
 │                │◄───Role: 4 (Owner)───────────────────────────────────│
 │                │                 │                  │                 │
 │                │                 │                  │                 │
 │                │──GET /dashboards/project-manager───►│                 │
 │                │  Authorization: Bearer {token}      │                 │
 │                │                 │                  │                 │
 │                │                 │──Validate Token───│                 │
 │                │                 │  Extract Role     │                 │
 │                │                 │                  │                 │
 │                │                 │──Fetch Dashboard Data───────────────│
 │                │                 │  (users, projects, schemas, tasks)  │
 │                │                 │                  │                 │
 │                │                 │◄───Dashboard Data───────────────────│
 │                │                 │  { numberOfUsers, numberOfProject,   │
 │                │                 │    numberOfSchemas, ... }            │
 │                │                 │                  │                 │
 │                │◄───Response─────│                  │                 │
 │                │  { data: {...}, │                  │                 │
 │                │    error: false }│                  │                 │
 │                │                 │                  │                 │
 │                │──Render ProjectManagerDashboard───►│                 │
 │                │  Display: Cards, Charts, Metrics   │                 │
 │                │                 │                  │                 │
 │◄───Home Page───│                 │                  │                 │
 │  (Dashboard)   │                 │                  │                 │
 │                │                 │                  │                 │
 │                │                 │                  │                 │
```

---

## Alternative Sequence Flow: Section Head Role

```
User          Frontend          Backend          Auth Service      Redux Store
 │                │                 │                  │                 │
 │───Navigate─────►│                 │                  │                 │
 │  to Home (/)    │                 │                  │                 │
 │                │                 │                  │                 │
 │                │──Get Role from Redux Store─────────────────────────►│
 │                │                 │                  │                 │
 │                │◄───Role: 1 (SectionHead)─────────────────────────────│
 │                │                 │                  │                 │
 │                │                 │                  │                 │
 │                │──GET /dashboards/section-head─────►│                 │
 │                │  Authorization: Bearer {token}      │                 │
 │                │                 │                  │                 │
 │                │                 │──Validate Token───│                 │
 │                │                 │  Extract Role     │                 │
 │                │                 │                  │                 │
 │                │                 │──Fetch Section Head Data────────────│
 │                │                 │  (tasks, userTasks, ...)             │
 │                │                 │                  │                 │
 │                │                 │◄───Dashboard Data───────────────────│
 │                │                 │  { tasksCount, userTasksCount, ... }│
 │                │                 │                  │                 │
 │                │◄───Response─────│                  │                 │
 │                │  { data: {...}, │                  │                 │
 │                │    error: false }│                  │                 │
 │                │                 │                  │                 │
 │                │──Render SectionHeadDashboard───────►│                 │
 │                │  Display: Tasks metrics, User Tasks│                 │
 │                │  Quick links: Tasks, User Tasks,   │                 │
 │                │  Advanced Report, My Leaves         │                 │
 │                │                 │                  │                 │
 │◄───Home Page───│                 │                  │                 │
 │  (Dashboard)   │                 │                  │                 │
```

---

## Implementation Notes

### Role Mapping Reference

```typescript
// Role IDs from backend enum UserRoleEnum
enum UserRole {
  ProjectManager = 0,  // Coordinator
  SectionHead = 1,
  TeamLeader = 2,
  Member = 3,
  Owner = 4
}
```

### Current Implementation Status

- ✅ Owner (4) - Has dashboard
- ✅ Coordinator (0) - Has dashboard (same as Owner)
- ✅ Team Leader (2) - Has dashboard
- ❌ Section Head (1) - Needs dashboard component + API
- ❌ Member (3) - Needs dashboard component + API

### Next Steps

1. Create backend endpoints for Section Head and Member dashboards
2. Create frontend dashboard components for Section Head and Member
3. Update home page to handle all roles correctly
4. Test each role to ensure correct dashboard displays
5. Add error handling and loading states

---

## Risk Assessment

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Backend API not ready | High | Low | Create mock data for frontend development |
| Role mapping incorrect | High | Medium | Verify role enum values match frontend expectations |
| Dashboard performance issues | Medium | Low | Implement loading states and optimize API calls |
| Missing dashboard data | Medium | Medium | Add fallback UI for missing data |

---

## Dependencies

- Backend: `/auth/about-me` endpoint must return role
- Backend: Dashboard endpoints must be available
- Frontend: Redux store must have authSlice with role
- Frontend: Dashboard components must be created/updated

---

## Success Metrics

- ✅ 100% of users see appropriate dashboard for their role
- ✅ Zero unauthorized dashboard content visible
- ✅ Dashboard loads within 2 seconds
- ✅ Zero console errors related to dashboard loading
- ✅ All 5 roles have functional dashboards

---

**Sprint Created:** [Current Date]  
**Sprint Owner:** Development Team  
**Status:** Ready for Development
