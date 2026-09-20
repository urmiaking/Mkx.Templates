# ADR 0002: Shared Service Contracts

**Status:** Accepted

Service interfaces/DTOs live in Shared. Server implementations live in Application; WASM implementations live in Client and call Server APIs. UI therefore depends on stable contracts while execution differs between server/browser. Pages should not bypass this model with raw HttpClient calls.
