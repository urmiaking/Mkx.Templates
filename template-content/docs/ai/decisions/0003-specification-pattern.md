# ADR 0003: Specification Pattern

**Status:** Accepted

Reusable persistence query intent uses specification classes instead of proliferating repository query methods or application-layer LINQ. This keeps filtering/includes/order/paging reusable and testable. Search existing specs before adding query methods.
