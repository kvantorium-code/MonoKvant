# Architecture Decision Records

Purpose:
- Long-lived architecture decisions of the project, with the context that produced them.
- One file per decision, named `NNNN-short-decision-title.md` with monotonic numbering.

Rules:
- Create an ADR when a change affects architecture, layers, module boundaries, or dependency direction.
- Create an ADR when a technology, SDK, DI framework, save system, localization, or networking layer is added, replaced, or removed.
- Create an ADR when a new durable pattern is introduced, or when resource, scene, prefab wiring, or bootstrap flow rules change.
- Do not create an ADR for minor bugfixes, local UI tweaks, renames, formatting, or tasks without architectural consequences.
- Never rewrite an existing ADR retroactively. Create a new ADR and mark the old one `Superseded` with a link to the replacement.
- Keep the ADR index table below up to date.

Statuses:
- `Proposed` - proposed but not yet accepted.
- `Accepted` - accepted and must be followed.
- `Deprecated` - obsolete, but historically important.
- `Superseded` - replaced by another ADR; must link to the new ADR.

Template:

```markdown
# ADR-NNNN: Short Decision Title

## Status

Accepted

## Context

What problem are we solving?
What constraints, project rules, or existing systems matter?

## Decision

What decision was made?

## Alternatives Considered

- Option A:
- Option B:
- Option C:

## Consequences

- Positive:
- Negative:
- Risks:

## Follow-up

- Documentation to update:
- Code areas affected:
- Validation needed:
```

## Index

| ADR | Status | Title | Date |
| --- | --- | --- | --- |
| 0001 | Accepted | Project structure under Assets/AngryBirds | 2026-09-26 |
