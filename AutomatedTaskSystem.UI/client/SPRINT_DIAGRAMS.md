# Role-Based Home Page - Visual Diagrams

## User Flow Diagram (Mermaid)

```mermaid
flowchart TD
    Start([User Opens Application]) --> CheckAuth{Is User Authenticated?}
    
    CheckAuth -->|No| LoginPage[Redirect to Login Page]
    LoginPage --> EnterCreds[User Enters Credentials]
    EnterCreds --> PostLogin[POST /auth/login]
    PostLogin --> ReceiveToken[Receive Token + Role]
    ReceiveToken --> StoreAuth[Store in authSlice]
    StoreAuth --> LoadHome[Load Home Page /]
    
    CheckAuth -->|Yes| LoadHome
    
    LoadHome --> GetRole[Get Role from authSlice]
    GetRole --> CheckRole{Check User Role}
    
    CheckRole -->|Role = 4 Owner| LoadPMDash[Load ProjectManagerDashboard]
    LoadPMDash --> CallPMAPI[GET /dashboards/project-manager]
    CallPMAPI --> DisplayPM[Display: Users, Projects, Schemas,<br/>Active Tasks, Charts]
    
    CheckRole -->|Role = 0 Coordinator| LoadPMDash2[Load ProjectManagerDashboard]
    LoadPMDash2 --> CallPMAPI2[GET /dashboards/project-manager]
    CallPMAPI2 --> DisplayPM2[Display: Same as Owner<br/>Note: My Leaves differs]
    
    CheckRole -->|Role = 1 SectionHead| LoadSHDash[Load SectionHeadDashboard]
    LoadSHDash --> CallSHAPI[GET /dashboards/section-head]
    CallSHAPI --> DisplaySH[Display: Tasks metrics,<br/>User Tasks metrics,<br/>Quick links]
    
    CheckRole -->|Role = 2 TeamLeader| LoadTLDash[Load TeamLeaderDashboard]
    LoadTLDash --> CallTLAPI[GET /dashboards/team-leader]
    CallTLAPI --> DisplayTL[Display: Members, Projects,<br/>Tasks charts, Navigation]
    
    CheckRole -->|Role = 3 Member| LoadMemDash[Load MemberDashboard]
    LoadMemDash --> CallMemAPI[GET /dashboards/member]
    CallMemAPI --> DisplayMem[Display: Personal tasks,<br/>My Leaves link]
    
    CheckRole -->|Unknown Role| DisplayError[Display Error / Empty State]
    
    DisplayPM --> End([Dashboard Rendered])
    DisplayPM2 --> End
    DisplaySH --> End
    DisplayTL --> End
    DisplayMem --> End
    DisplayError --> End
    
    style Start fill:#e1f5ff
    style End fill:#c8e6c9
    style CheckAuth fill:#fff9c4
    style CheckRole fill:#fff9c4
    style LoginPage fill:#ffccbc
    style DisplayError fill:#ffcdd2
```

## Sequence Flow Diagram - Owner Login (Mermaid)

```mermaid
sequenceDiagram
    participant U as User
    participant F as Frontend
    participant B as Backend
    participant A as Auth Service
    participant R as Redux Store
    
    U->>F: Login (credentials)
    F->>B: POST /auth/login { code }
    B->>A: Validate credentials
    A-->>B: Token + Role (4)
    B-->>F: Response { token, role: 4 }
    F->>F: Store token in localStorage
    F->>R: Dispatch login { name, role: 4, id, group }
    R-->>R: Store role in authSlice
    
    U->>F: Navigate to Home (/)
    F->>R: Get role from authSlice
    R-->>F: Role: 4 (Owner)
    F->>B: GET /dashboards/project-manager<br/>Authorization: Bearer {token}
    B->>A: Validate token, extract role
    B->>B: Fetch dashboard data<br/>(users, projects, schemas, tasks)
    B-->>F: Response { data: {...}, error: false }
    F->>F: Render ProjectManagerDashboard
    F-->>U: Display Home Page (Dashboard)
```

## Sequence Flow Diagram - Section Head Login (Mermaid)

```mermaid
sequenceDiagram
    participant U as User
    participant F as Frontend
    participant B as Backend
    participant A as Auth Service
    participant R as Redux Store
    
    U->>F: Login (credentials)
    F->>B: POST /auth/login { code }
    B->>A: Validate credentials
    A-->>B: Token + Role (1)
    B-->>F: Response { token, role: 1 }
    F->>F: Store token in localStorage
    F->>R: Dispatch login { name, role: 1, id, group }
    R-->>R: Store role in authSlice
    
    U->>F: Navigate to Home (/)
    F->>R: Get role from authSlice
    R-->>F: Role: 1 (SectionHead)
    F->>B: GET /dashboards/section-head<br/>Authorization: Bearer {token}
    B->>A: Validate token, extract role
    B->>B: Fetch section head dashboard data<br/>(tasks, userTasks, ...)
    B-->>F: Response { data: {...}, error: false }
    F->>F: Render SectionHeadDashboard
    F-->>U: Display Home Page (Section Head Dashboard)
```

## Sequence Flow Diagram - Member Login (Mermaid)

```mermaid
sequenceDiagram
    participant U as User
    participant F as Frontend
    participant B as Backend
    participant A as Auth Service
    participant R as Redux Store
    
    U->>F: Login (credentials)
    F->>B: POST /auth/login { code }
    B->>A: Validate credentials
    A-->>B: Token + Role (3)
    B-->>F: Response { token, role: 3 }
    F->>F: Store token in localStorage
    F->>R: Dispatch login { name, role: 3, id, group }
    R-->>R: Store role in authSlice
    
    U->>F: Navigate to Home (/)
    F->>R: Get role from authSlice
    R-->>F: Role: 3 (Member)
    F->>B: GET /dashboards/member<br/>Authorization: Bearer {token}
    B->>A: Validate token, extract role
    B->>B: Fetch member dashboard data<br/>(personal tasks, ...)
    B-->>F: Response { data: {...}, error: false }
    F->>F: Render MemberDashboard
    F-->>U: Display Home Page (Member Dashboard)
```

## Role-Based Dashboard Selection Flow (Mermaid)

```mermaid
flowchart LR
    Start([Home Page Loads]) --> GetRole[Get Role from Redux]
    GetRole --> RoleSwitch{Switch Role}
    
    RoleSwitch -->|0| PM[ProjectManager<br/>Dashboard]
    RoleSwitch -->|1| SH[SectionHead<br/>Dashboard]
    RoleSwitch -->|2| TL[TeamLeader<br/>Dashboard]
    RoleSwitch -->|3| Mem[Member<br/>Dashboard]
    RoleSwitch -->|4| Owner[Owner<br/>Dashboard]
    
    PM --> PMAPI[GET /dashboards/<br/>project-manager]
    SH --> SHAPI[GET /dashboards/<br/>section-head]
    TL --> TLAPI[GET /dashboards/<br/>team-leader]
    Mem --> MemAPI[GET /dashboards/<br/>member]
    Owner --> PMAPI2[GET /dashboards/<br/>project-manager]
    
    PMAPI --> Render[Render Dashboard Component]
    SHAPI --> Render
    TLAPI --> Render
    MemAPI --> Render
    PMAPI2 --> Render
    
    Render --> End([Display Dashboard])
    
    style Start fill:#e1f5ff
    style End fill:#c8e6c9
    style RoleSwitch fill:#fff9c4
    style PM fill:#bbdefb
    style SH fill:#c5e1a5
    style TL fill:#ffe082
    style Mem fill:#f8bbd0
    style Owner fill:#ce93d8
```

## Component Architecture Diagram (Mermaid)

```mermaid
graph TB
    subgraph "Home Page (index.tsx)"
        HP[Home Page Component]
        HP --> CheckRole{Check authSlice.role}
    end
    
    subgraph "Dashboard Components"
        PMD[ProjectManagerDashboard<br/>Roles: 0, 4]
        SHD[SectionHeadDashboard<br/>Role: 1]
        TLD[TeamLeaderDashboard<br/>Role: 2]
        MD[MemberDashboard<br/>Role: 3]
    end
    
    subgraph "API Layer"
        PMAPI[GET /dashboards/project-manager]
        SHAPI[GET /dashboards/section-head]
        TLAPI[GET /dashboards/team-leader]
        MEMAPI[GET /dashboards/member]
    end
    
    subgraph "State Management"
        AuthSlice[authSlice<br/>role: UserRole]
    end
    
    CheckRole -->|role === 0 or 4| PMD
    CheckRole -->|role === 1| SHD
    CheckRole -->|role === 2| TLD
    CheckRole -->|role === 3| MD
    
    PMD --> PMAPI
    SHD --> SHAPI
    TLD --> TLAPI
    MD --> MEMAPI
    
    AuthSlice --> CheckRole
    
    style HP fill:#e1f5ff
    style PMD fill:#bbdefb
    style SHD fill:#c5e1a5
    style TLD fill:#ffe082
    style MD fill:#f8bbd0
    style AuthSlice fill:#fff9c4
```

## Role Permission Matrix (Mermaid)

```mermaid
graph LR
    subgraph "Roles"
        R0[Coordinator<br/>Role: 0]
        R1[SectionHead<br/>Role: 1]
        R2[TeamLeader<br/>Role: 2]
        R3[Member<br/>Role: 3]
        R4[Owner<br/>Role: 4]
    end
    
    subgraph "Dashboard Access"
        D0[ProjectManager<br/>Dashboard]
        D1[SectionHead<br/>Dashboard]
        D2[TeamLeader<br/>Dashboard]
        D3[Member<br/>Dashboard]
    end
    
    R0 --> D0
    R4 --> D0
    R1 --> D1
    R2 --> D2
    R3 --> D3
    
    style R0 fill:#bbdefb
    style R1 fill:#c5e1a5
    style R2 fill:#ffe082
    style R3 fill:#f8bbd0
    style R4 fill:#ce93d8
    style D0 fill:#e3f2fd
    style D1 fill:#f1f8e9
    style D2 fill:#fffde7
    style D3 fill:#fce4ec
```

---

## How to View These Diagrams

1. **VS Code:** Install the "Markdown Preview Mermaid Support" extension
2. **GitHub:** Diagrams will render automatically in markdown files
3. **Online:** Copy mermaid code to https://mermaid.live/
4. **Documentation Tools:** Most markdown renderers support Mermaid

---

## Text-Based Flow Diagrams

### User Flow (Simplified Text)

```
START
  ↓
User Opens App
  ↓
[Authenticated?]
  ├─ NO → Login → Store Token & Role → Continue
  └─ YES → Continue
  ↓
Load Home Page (/)
  ↓
Get Role from Redux Store
  ↓
[Role Check]
  ├─ 4 (Owner) → ProjectManagerDashboard → GET /dashboards/project-manager
  ├─ 0 (Coordinator) → ProjectManagerDashboard → GET /dashboards/project-manager
  ├─ 1 (SectionHead) → SectionHeadDashboard → GET /dashboards/section-head
  ├─ 2 (TeamLeader) → TeamLeaderDashboard → GET /dashboards/team-leader
  └─ 3 (Member) → MemberDashboard → GET /dashboards/member
  ↓
Render Dashboard Component
  ↓
Display Dashboard
  ↓
END
```

### Sequence Flow (Simplified Text)

```
User → Frontend: Login
Frontend → Backend: POST /auth/login
Backend → Auth: Validate
Auth → Backend: Token + Role
Backend → Frontend: Response { token, role }
Frontend → Redux: Store role
Frontend → Redux: Get role
Redux → Frontend: Role value
Frontend → Backend: GET /dashboards/{role-specific}
Backend → Frontend: Dashboard data
Frontend → User: Display dashboard
```

---

**Note:** These diagrams are complementary to the main sprint document and provide visual representations of the user flows and system interactions for the role-based home page feature.
