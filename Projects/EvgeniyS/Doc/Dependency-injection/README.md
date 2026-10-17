# Dependency Injection

DI framework: not used.

## Rules

- The project does not use a DI container at this stage.
- Pass dependencies through explicit initialization, serialized references, or plain constructors.
- Do not use service locators or global singletons for gameplay/UI dependencies unless agreed.
- Do not instantiate services directly inside gameplay or UI classes; wire them via composition root or explicit initialization.
- If a DI framework is introduced later, record the decision as an ADR in `Doc/ADR` and update this file.

## Binding Locations

- Project-level composition: TBD
- Scene-level wiring: TBD

## Validation

- Verify that manually wired references are assigned in the relevant scenes/prefabs.
- Check Unity Console after changing wiring or initialization flow.
