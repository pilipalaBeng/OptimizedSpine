# Decision 0001: Use A Measurement-First Spine Optimization Sandbox

- Date: 2026-07-06
- Status: Accepted
- Related discussion: `019f275d-c197-7260-a9a1-c90d907240ab`

## Context

The project is intended for Spine optimization experiments in Unity. The prior discussion framed this as a separate, low-risk sandbox rather than a production requirement.

Spine performance can be affected by asset structure, atlas/material setup, mesh generation, update visibility, batching, and runtime behavior. Those factors interact, so undocumented one-off changes can easily produce misleading conclusions.

## Decision

Use this project as a measurement-first sandbox.

The preferred order is:

1. Build repeatable benchmark scenes.
2. Add editor-side analyzer tools.
3. Implement runtime optimization strategies only after enough measurements exist.

## Consequences

- Early work should prioritize repeatability and records over clever runtime changes.
- Analyzer recommendations should be explainable and should not silently mutate assets.
- Any optimization claim needs a matching experiment record.
- Prior discussion is useful context, but current project measurements override assumptions.
