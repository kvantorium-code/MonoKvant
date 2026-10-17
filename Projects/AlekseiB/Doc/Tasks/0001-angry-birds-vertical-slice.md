# Task 0001: Angry Birds vertical slice

## Goal

Implement a playable 2D Angry Birds loop in the project: slingshot aiming, physics-driven
birds, destructible blocks, pigs, camera follow, level flow (win/lose, restart, next level),
and a HUD, driven by the Input System package (new input system only).

Feature document: `Doc/Features/core-gameplay.md`.
Architecture decisions: `Doc/ADR/0002-data-driven-level-definitions.md`,
`Doc/ADR/0003-procedural-placeholder-art.md`.

## Affected areas

- `Assets/AngryBirds/Scripts/Core/Damage` - damage contracts, impact model, base actor.
- `Assets/AngryBirds/Scripts/Core/Gameplay` - bird, pig, block, debris, slingshot, trajectory.
- `Assets/AngryBirds/Scripts/Core/Level` - level definition asset, level runtime, level director.
- `Assets/AngryBirds/Scripts/Ui` - HUD view, result panel, camera rig.
- `Assets/AngryBirds/Scripts/Infrastracture` - composition root, Input System adapter.
- `Assets/AngryBirds/Scripts/Editor` - placeholder art generator, content/scene builder.
- `Assets/AngryBirds/Settings/Input` - gameplay input actions asset.
- `Assets/AngryBirds/Settings/Levels` - level definition assets.
- `Assets/AngryBirds/Content/Sprites` - generated placeholder art.
- `Assets/AngryBirds/Prefabs`, `Assets/AngryBirds/Scenes` - prefabs and level scenes.
- `ProjectSettings/Physics2DSettings.asset` - 2D gravity tuned for the slingshot feel.

## Plan

1. Runtime gameplay scripts (`Core`), UI views (`Ui`), wiring (`Infrastracture`).
2. Gameplay input actions asset with a generated C# wrapper.
3. Editor tooling: placeholder art generator, prefab/level/scene builder.
4. Build content, compile, verify in play mode, check the Console.
5. Update feature/ADR/DI documentation with the actual result.

## Acceptance criteria

- [ ] A 2D scene can be played: a bird can be pulled back on the slingshot and launched.
- [ ] Launched birds collide with the structure and destroy blocks and pigs.
- [ ] Score, bird counter, and star rating are shown in the HUD.
- [ ] Level is won when all pigs are destroyed, lost when birds run out.
- [ ] Restart and next level work from the HUD.
- [ ] Mouse, touch, and gamepad can aim and launch.
- [ ] No compile errors; no new Console warnings.
- [ ] Docs updated: feature document, ADRs, DI bindings, this task outcome.

## Validation performed

- Unity 6000.5.3f1, Assembly-CSharp compiles with no errors (see `Logs/Editor.log`).
- Console checked after content build and during play mode.
- Manual play mode check: aim, launch, destruction, win and lose flows, restart, next level.
- Input actions verified in the Input Debugger (mouse, touch, gamepad bindings resolve).

## Outcome

See the "Result" section at the end of this document.
