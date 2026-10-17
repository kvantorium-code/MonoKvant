# Dependency Injection

DI framework: TBD. The project currently does not use a DI container.

Rules:
- Do not instantiate services directly inside gameplay or UI classes when they belong to DI.
- Keep project-wide bindings separate from scene-specific bindings.
- Prefer constructor injection for plain C# services.
- For `MonoBehaviour`, use the injection pattern supported by the DI framework chosen later, and avoid using injected dependencies before injection is complete.
- Document every new long-lived service binding in this file.
- Until a DI framework is selected, keep wiring in `Assets/AngryBirds/Scripts/Infrastracture` and pass dependencies explicitly.

## Binding Locations

- Project-level bindings:
  - TBD (no DI framework selected yet)
- Scene-level bindings:
  - TBD (no DI framework selected yet)

## Validation

- Verify that all injected dependencies have bindings once a DI framework is introduced.
- Check Unity Console after changing installers or bindings.
