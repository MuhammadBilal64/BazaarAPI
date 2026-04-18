# Phase 2 - Conversational Commerce v1

## Goal
Enable natural-language shopping without introducing heavy RAG infrastructure yet.

## What We Will Cover
- Assistant API surface (`/api/assistant/query`, `/api/assistant/feedback`)
- Intent extraction from user text into structured filters
- Deterministic candidate retrieval from catalog using extracted constraints
- Response composition with recommendation explanations
- Session context handling for multi-turn shopping flows

## Key Deliverables
- Assistant controller + DTOs for query/response and feedback
- Intent parser abstraction (LLM-backed or rule-backed adapter)
- Recommendation orchestration service (retrieve -> rank -> explain)
- Conversation session storage model (minimal memory per user/session)
- Fallback behavior when intent extraction fails

## Definition of Done
- User can ask for products in natural language and receive relevant results
- Assistant returns reasons for each recommendation
- Follow-up message can reuse prior session context
- Assistant failure paths degrade gracefully to basic filters/search
