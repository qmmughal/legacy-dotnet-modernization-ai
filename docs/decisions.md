# Decision log

| ID | Date | Decision | Why | Approved by |
|---|---|---|---|---|
| D-001 | 2026-11-02 | Target .NET 10 (LTS) for all new projects | Patch-only line until Nov 2028; .NET 8/9 reached end of support 10 Nov 2026 | Architecture board |
| D-002 | 2026-11-02 | Strangler fig with a YARP gateway in front of IIS | Feature-by-feature cut-over with instant rollback by route | Architecture board |
| D-003 | 2026-11-03 | Characterization tests before any port | Pricing has no specification beyond the code | Sales Operations |
| D-004 | 2026-11-05 | Keep SOAP contract via CoreWCF; add REST alongside | ERP and two partners call the SOAP service | Integration lead |
| D-005 | 2026-11-06 | EF Core maps onto the existing schema; no schema changes in phase 1 | Both apps share one database during coexistence | DBA |
| D-006 | 2026-11-09 | Migrate Membership hashes lazily (verify legacy, rehash on sign-in) | No forced password reset for 1,400 users | Security |
| D-007 | 2026-11-10 | Preserve null-region filter semantics in Orders | Matches EF6 behaviour; change later as a separate decision | Product owner |
