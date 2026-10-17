# AGENTS.md

## Project Context

- Read `Doc/Architect-baseline/README.md` before architectural changes.
- Read `Doc/Dependency-injection/README.md` before adding or modifying dependencies.
- Read relevant files in `Doc/Features` before changing feature behavior.
- Create implementation task notes in `Doc/Tasks` when the work is task-based.
- Put review results in `Doc/Reviews`.
- Record long-lived architecture decisions as ADR files in `Doc/ADR`.

## General Rules

- Make minimal, targeted changes.
- Preserve existing architecture and public APIs unless the task explicitly requires changes.
- Prefer Unity APIs and established project patterns.
- Avoid unnecessary allocations in frequently called runtime code.
- Do not rewrite unrelated systems.

## Unity Rules

- Avoid heavy logic in `Update()`.
- Cache component references instead of repeatedly calling `GetComponent`.
- Prefer event-driven logic where practical.
- Use serialized references for scene and prefab wiring when appropriate.
- Keep runtime code separate from editor-only code.
- Put editor scripts inside `Editor` folders.

## Dependency Injection

- Follow `Doc/Dependency-injection/README.md`.
- Do not create services directly inside gameplay or UI classes when they belong in DI.
- Register project-wide services in the project-level composition root.
- Register scene-specific dependencies in scene-level installers/scopes.
- For `MonoBehaviour` classes, prefer method/property injection patterns supported by the selected DI framework.

## Documentation Routing

- Architecture rules: `Doc/Architect-baseline/README.md`.
- Dependency rules: `Doc/Dependency-injection/README.md`.
- Feature behavior: `Doc/Features/`.
- Architecture decisions: `Doc/ADR/`.
- Setup and tooling: `Doc/Setup/`.
- Task execution notes: `Doc/Tasks/`.
- Review notes: `Doc/Reviews/`.

## Validation

- After script changes, verify Unity compilation.
- Check Console errors and warnings.
- Mention affected scenes and prefabs when manual inspector setup is required.
- If validation cannot be run, explain why.
