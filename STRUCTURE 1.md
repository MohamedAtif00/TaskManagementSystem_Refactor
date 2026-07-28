# Subject Library — Curriculum Hierarchy

An explanation of how the curriculum is organized, from the top level (Season) down to the actual Subject.

---

## The Hierarchy

The structure has 5 levels. Each level contains the one below it:

```
Season (academic season)
  └─ Program
       └─ Term
            └─ Family (a grouping of related subjects)
                 └─ Subject (the actual subject)
```

---

## What each level means

| Level | Represents | Key attributes |
|-------|-----------|----------------|
| **Season** | The academic season / year — the top of the tree. Everything else lives inside a season. | Academic year, start & end dates, archived flag |
| **Program** | A program offered within the season. | Name, code, archived flag |
| **Term** | A term or time window inside a program. | Name, code, start & end dates |
| **Family** | A subject family — a grouping of related subjects. | Name, code |
| **Subject** | The actual subject taught. This is the leaf of the tree. | Code, grade, language, publication status, production target |

---

## Notes

- **Publication starts at the Subject level only** — the higher levels (Season, Program, Term, Family) are organizational; a subject is what gets published.
- A **full path** through the hierarchy is: `Season → Program → Term → Family → Subject`. This complete path is what identifies a subject in context.
- Any level can be **archived** (hidden but retained), and archiving/deleting a level cascades to everything beneath it.
