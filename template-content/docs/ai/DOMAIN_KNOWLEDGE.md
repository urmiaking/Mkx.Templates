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

- **Administrators**: Built-in administrator role. It receives registered policy claims from the startup seeder.
- **Users**: Built-in standard user role. Its policy claims can be assigned in the role claim editor.

### Template identity and policy editor

The template's actual built-in Identity roles are `Administrators` and `Users` (`BuiltinRoles.Roles`). The administrator role is seeded with claims from registered policy providers when the host starts. The user management policies are `AppPolicies.Users.View`, `Manage`, and `ManageClaims`; the initial tree also contains `AppPolicies.Tests.View`. This is a template policy catalog, so applications should add their own policies to `AppPolicyProvider` as they add features.

The user claim editor changes direct claims on one user. The role claim editor changes claims on a built-in role; role membership is managed separately. The current role seeder reapplies provider policy claims to `Administrators` on startup, so removing one of those claims from that role in the editor is not persistent across a restart. Identity claims outside the registered policy catalog are not editable through this tree.
