# Domain Knowledge Guide

*Note for Developers: Update this document with specific terminology, invariants, state machine logic, policies, and business rules of your application so that AI agents can verify requirements and write business logic matching real-world conditions.*

---

## 1. Glossary & Ubiquitous Language

Define terms used in code classes, properties, and database tables to ensure alignment across all layers.

| Term | Definition | Code Symbol (if applicable) |
| :--- | :--- | :--- |
| **ExampleTerm** | Brief description of what this term means in business context. | `ExampleClass` |

---

## 2. Core Business Invariants & Constraints

List core rules that must remain true at all times in the system. Domain models should validate these constraints before persisting state changes.

- **Invariant A**: E.g., "An invoice balance cannot become negative."
- **Invariant B**: E.g., "A customer account cannot be deleted if there are open transactions."

---

## 3. Core Workflows & State Machines

Draw diagrams or describe state transitions for business objects (e.g. Orders, Shipments, Invoices):

```
[ Draft ] ──( Submit )──> [ Pending Approval ] ──( Approve )──> [ Active ]
```

- **Draft State**: Editable, not visible to system operations.
- **Pending State**: Read-only, waiting for approval.
- **Active State**: Validated and operational.

---

## 4. User Roles & Security Policies

List the actors in the system, their access scope, and authorization rules:

- **Admin**: Full system access, can configure global settings.
- **Operator**: Can create and edit business records but cannot delete them.
- **Viewer**: Read-only access to specific dashboards.
