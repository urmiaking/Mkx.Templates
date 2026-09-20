# ADR 0005: Attribute-Based DI Scanning

**Status:** Accepted

Concrete services use SDK lifetime attributes such as `[ScopedService]`, `[TransientService]`, and `[SingletonService]`. This removes repetitive registration and keeps lifetime intent near implementation. Do not manually register an auto-scanned service again unless deliberately overriding behavior.
