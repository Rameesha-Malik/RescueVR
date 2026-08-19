# Integrator Checklist

Both work plans repeatedly point to "the Integrator's checkpoint" without spelling out what
that person actually does. This is that missing page — whoever on the team merges Pair A and
Pair B's branches should work through it in order.

## 1. Merge order
1. Merge **Pair A's branch** first (AR scene + car + tagged victim character). This is the
   foundation — per the Pair A plan, it's "the foundation the Integrator merges first."
2. Merge **Pair B's branch** (`ZoneTouchDetector.cs`, `RescueManager.cs`, UI) on top.
3. Resolve any conflicts in scene files by re-doing the losing side's changes by hand in the
   Editor afterward — Unity scene/prefab YAML conflicts are rarely safe to merge textually.

## 2. Wire the combined scene
In the merged scene:
- [ ] One `RescueManager` component exists in the scene (on its own empty GameObject).
- [ ] `ZoneTouchDetector` exists in the scene and can see `Camera.main`.
- [ ] The victim character's three zones are tagged exactly `Zone_Pulse`, `Zone_Breathing`,
      `Zone_Neck` (check `Edit → Project Settings → Tags and Layers` if any look off).
- [ ] The Canvas UI (`RescueUIController`) is present, its Step Prompt / Result Text / Lift
      Button references are all assigned in the Inspector, and it is not double-wired (the
      Lift button's `OnClick()` should call `AttemptLift()` either from the Inspector *or*
      from `RescueUIController`'s code — not both).
- [ ] `TapToPlace.placementPrefab` on Pair A's AR Origin points at the real crashed-car prefab,
      not the debug placeholder cube.

## 3. End-to-end test pass
Run through this exact sequence on a real device and confirm each line:
- [ ] Tap a real-world surface → car + victim character appear correctly placed.
- [ ] Tap the pulse zone → prompt updates to "Check the victim's breathing."
- [ ] Tap the breathing zone → prompt updates to "Apply the neck brace."
- [ ] Tap the neck zone → prompt updates to "All checks complete — lift the patient."
- [ ] Press **Lift Patient** now → "Great job! Patient stabilized."
- [ ] Restart, press **Lift Patient** immediately (skip all zones) → "SPINAL FRACTURE — Restart".
- [ ] Tapping empty space / the car / the ground does nothing and does not throw errors in
      the Console.
- [ ] Repeat the full pass 5 times without a single failure (see the Final Checklist in the
      main methodology doc).

## 4. If something doesn't connect
- `RescueManager.Instance` null → the RescueManager GameObject isn't in the scene, or was
  destroyed/disabled before another script's `Awake()`/`Start()` ran.
- Zone detected as the wrong type, or not at all → tag spelling mismatch. Compare the literal
  string in `ZoneTouchDetector.cs` against the tag on the GameObject, character by character.
- UI never updates → `RescueUIController`'s references weren't dragged into the Inspector
  after the merge (this commonly resets when a prefab is re-imported).
