# Task 0003: Fix HUD restart button placement and broken sprite scaling

## Goal

After task 0002 the level scenes run, but two visual problems remained:

- The HUD restart button (and the live stars) floated in the middle of the screen over
  the play area instead of sitting in a corner.
- The graphics looked low quality: blocks and the ground rendered at the wrong size, the
  sky was invisible (flat camera color), and sprites shimmered when zoomed out.

Feature document: `Doc/Features/core-gameplay.md`.
Follows: `Doc/Tasks/0002-fix-game-launch-ui-input.md`.

## Findings (root causes)

1. **`SpriteRenderer.size` was ignored.** `SpriteRenderer.size` only takes effect in
   `SpriteDrawMode.Sliced` or `Tiled`; in `Simple` mode the sprite keeps its native size.
   The code sets `size` without `drawMode` in four places:
   - `Block.ApplySize` — a `3 × 0.35` beam rendered as the `2 × 0.5` wide texture, so the
     art did not match the collider and the structures looked broken.
   - `LevelDirector.BuildGround` — segments rendered at native size by luck (approx. the
     requested size), but still not honoring `groundSegmentWidth`.
   - `SpriteBurst` pieces — always 0.32 units instead of `pieceSize`.
   - `GameContentBuilder.CreateBackdrop` — the `8 × 256` sky sprite should be stretched to
     `90 × 16` but stayed a 0.04-unit sliver, so the “sky” was just the flat camera clear
     color; clouds stayed at native `3 × 1.5` instead of `5.5 × 2.8`.
2. **HUD widgets were center-anchored.** The live stars (`-194…-46, -70`) and the restart
   button (`120, 60`) were anchored to the center of the stretched HUD, so they appeared in
   the middle of the screen, over the gameplay.
3. **No mipmaps.** Sprites were imported without mipmaps and are displayed far smaller than
   their source resolution in the play area, causing shimmering/aliasing.

## Affected areas

- `Assets/AngryBirds/Scripts/Core/Gameplay/Block.cs`
- `Assets/AngryBirds/Scripts/Core/Level/LevelDirector.cs`
- `Assets/AngryBirds/Scripts/Core/Gameplay/SpriteBurst.cs`
- `Assets/AngryBirds/Scripts/Editor/Builder/GameContentBuilder.cs`
- `Assets/AngryBirds/Scripts/Editor/Art/PlaceholderArtGenerator.cs`
- `Assets/AngryBirds/Content/Sprites/*.png.meta` (mipmaps enabled)

## Plan

1. Set `SpriteRenderer.drawMode = SpriteDrawMode.Sliced` wherever `size` scales a sprite:
   blocks, ground segments, burst pieces, and the builder backdrop (sky + clouds).
2. Move the HUD live stars to the top-left (under the info block) and pin the restart
   button to the top-right, in the builder `CreateHud`.
3. Enable mipmaps for all placeholder sprites in `PlaceholderArtGenerator` and flip
   `enableMipMap` in the existing `.png.meta` files so the change applies without
   regenerating art.

## Acceptance criteria

- [ ] The HUD restart button sits in the top-right corner and the stars in the top-left;
      neither covers the play area.
- [ ] Blocks render at exactly their spawned size (collider and art match).
- [ ] The sky is a stretched vertical gradient and the clouds are 5.5 × 2.8.
- [ ] No sprite shimmer while zoomed out; no new Console warnings.
- [ ] Re-running `Tools/AngryBirds/2. Build Game Content` reproduces the same result.

## Validation performed

- `dotnet build Assembly-CSharp-Editor.csproj` compiles with 0 errors (the csproj has two
  stale `TutorialInfo` compile entries for deleted files; these were bypassed locally for
  the check and the csproj files were restored afterwards — Unity will clean them up on
  the next recompile).
- Unity play-mode validation still pending: the editor was open, so scenes were not
  patched directly. Re-running `Tools/AngryBirds/2. Build Game Content` regenerates the
  scenes with the new HUD layout and backdrop sizing.

## Outcome

- Runtime fixes (blocks, ground, bursts) apply immediately on the next Play.
- Builder fixes (HUD layout, sky/cloud sizing) apply after re-running
  `Tools/AngryBirds/2. Build Game Content`.
- Mipmaps enabled on all placeholder sprites (already in effect after the Unity reimport).