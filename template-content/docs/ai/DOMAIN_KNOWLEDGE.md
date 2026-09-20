# Domain Knowledge Guide

This file is the durable business-context workspace for the application generated from this template. Unlike architecture documents, it should evolve with the product domain.

Agents: update this file when the user establishes a stable business term, invariant, workflow, state transition, calculation rule, or domain decision. Do not add guesses, transient requirements, secrets, or implementation-only details.

## 1. Glossary & Ubiquitous Language

Document domain terms in the language used by stakeholders.

| Term | Meaning | Notes |
|---|---|---|
| Example | Replace with a real domain term | Delete template rows once real domain knowledge exists |

## 2. Core Business Invariants & Constraints

Record rules that must always remain true regardless of UI/API implementation.

Example format:
- **Invoice posting:** A posted invoice cannot be edited directly; corrections use the domain's reversal/correction workflow.
- **Uniqueness:** Describe identifiers that must be unique and whether the database also enforces them.

## 3. Core Workflows & State Machines

Describe meaningful lifecycle transitions explicitly.

```text
Draft -> Submitted -> Approved
   \-> Cancelled
```

For each workflow, document:
- allowed transitions;
- who/what can trigger them;
- validation/invariants;
- important side effects.

## 4. Calculations & Financial/Domain Formulas

Record formulas with units, rounding rules, currency/unit assumptions, and examples. Do not leave critical calculation semantics only in code.

## 5. User Roles & Security Policies

Document business meaning of roles and capabilities. Technical policy registration belongs in the authorization documentation; this section explains what the permissions mean to the business.

### Template Identity and Policy Editor
The template ships with administrator/user identity infrastructure and a policy-tree editor. Application-specific policies should be added as the generated product's authorization requirements become known.

## 6. Integrations & External Systems

For each durable integration document:
- business purpose;
- source of truth;
- identifiers exchanged;
- sync direction;
- failure/retry expectations;
- ownership of data.

Never store credentials or secrets here.

## 7. Business Decisions

Keep short dated entries for decisions that future agents need to understand.

### YYYY-MM-DD — Decision title
- **Decision:** What was decided.
- **Reason:** Why.
- **Consequences:** What future implementation must preserve.
- **Supersedes:** Link/identify an older decision if applicable.

Cross-cutting technical architecture decisions belong in `docs/ai/decisions/` instead.
