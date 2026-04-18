# Phase 1 - Foundation and Reliability

## Goal
Make the commerce core safe, deterministic, and testable before adding AI behavior.

## What We Will Cover
- Idempotent checkout (`POST /api/Order`) using `Idempotency-Key`
- Strong transactional safety for order creation and stock updates
- Consistent API error contracts (`ProblemDetails`) for business and validation errors
- Request validation hardening for DTOs and query params
- Observability baseline (correlation ID + structured logs + health checks)
- Automated tests for critical flows (unit + integration + smoke)

## Key Deliverables
- `IdempotencyRecord` entity + persistence and lookup strategy
- Updated order service/controller to support safe retries
- Integration tests proving no duplicate orders on repeated requests
- Standardized exception mapping and response behavior
- Correlation ID middleware wired into request pipeline
- Stable smoke script for end-to-end regression checks

## Definition of Done
- Repeated `POST /api/Order` with same key does not duplicate order
- Stock is decremented exactly once for idempotent retries
- Core auth/cart/order/admin integration tests pass
- Logs allow request tracing with correlation ID
- API returns predictable status + error payloads for invalid/edge cases
