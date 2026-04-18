# Phase 4 - RAG and Semantic Intelligence (v2)

## Goal
Add semantic understanding and grounded long-form answers while preserving deterministic commerce logic.

## What We Will Cover
- Embedding pipeline for product/catalog text and support knowledge
- Vector search for semantic retrieval (query-to-context chunks)
- Hybrid retrieval (structured filters + semantic candidates)
- Grounded answer generation with context citations/explanations
- Guardrails against hallucinated products or unsupported claims

## Key Deliverables
- Indexing job/service for embeddings and vector persistence
- Semantic retrieval adapter with top-k context chunks
- Assistant response pipeline upgraded to use retrieved context
- Citation/explanation structure in API responses
- Safety checks to ensure final products come from valid catalog IDs

## Definition of Done
- Assistant answers nuanced product questions with grounded context
- Semantic retrieval improves relevance for vague/complex queries
- No hallucinated non-existent items in final recommendation set
- Latency remains within acceptable bounds for chat interactions
