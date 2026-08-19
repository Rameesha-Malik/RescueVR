# RescueVR — Pair B Work Plan
### Interaction & Logic Module | CATCH_VR Bootcamp, GIK Institute

> CONFIRMED: Pair B starts working the moment the team meeting ends — Hour 0, at the exact
> same time as Pair A. You do NOT need anything from Pair A to begin. You test against
> placeholder cubes in your own scene and only combine with Pair A's real character later, at
> the Integrator's checkpoint.

## Team Roles in This Pair
- **Person B1** — Touch Detection (screen tap → raycast → identify zone hit)
- **Person B2** — Sequence & Feedback Logic (`RescueManager.cs`, pass/fail states, UI)

## Shared Names to Remember (agreed with the whole team)
- Function everyone calls: `RescueManager.Instance.ZoneTouched(string zoneName)`
- Exact string values passed in: `"Pulse"`, `"Breathing"`, `"Neck"`
- These must match exactly on both sides — this is the #1 source of integration bugs if mismatched.

## PERSON B1 — Step by Step
Task: Detect which body-part zone the user tapped on the phone screen.

1. Create your own blank test scene, completely separate from Pair A's work.
2. Add a plain 3D cube, tag it `Zone_Pulse` (temporary placeholder for testing).
3. Write a script that: detects a screen touch, casts a ray from the camera through the touch point, checks what collider the ray hit, and reports the zone name if the tag matches. → `Assets/Scripts/Interaction/ZoneTouchDetector.cs`
4. Core logic: `Input.touchCount > 0` → `Camera.main.ScreenPointToRay(touchPosition)` → `Physics.Raycast` → check `hit.collider.tag` → call `RescueManager.Instance.ZoneTouched(zoneName)`.
5. Test: tapping the cube on your phone screen correctly detects and logs "Pulse".
6. Add two more test cubes tagged `Zone_Breathing` and `Zone_Neck`.
7. Test: confirm detection correctly distinguishes all three zones without mixing them up.
8. Test edge cases: tapping empty space (should do nothing, not crash), tapping near-but-not-on a cube. (`ZoneTouchDetector` already handles a missed raycast safely.)
9. Once Person A2 (Pair A) pushes their real tagged character, swap your test cubes for the real prefab.
10. Confirm detection still works exactly the same way on the real character.
11. Push this branch once verified on the real character.

## PERSON B2 — Step by Step
Task: Build the decision-making system that tracks progress and decides success or failure.

1. Before writing any code, sketch the logic on paper (or ask Claude to help): what counts as a valid sequence, what triggers a failure state?
2. Create a new script: `RescueManager.cs`. → `Assets/Scripts/Core/RescueManager.cs`
3. Add a public static `Instance` reference (singleton pattern) so other scripts can call it easily.
4. Add a public function: `ZoneTouched(string zoneName)` — sets an internal flag (`pulseChecked`, `breathingChecked`, or `neckBraced`) to true depending on which zone name was passed in.
5. Add a function: `AttemptLift()` — checks if all three flags are true. If yes, trigger a Success state. If no, trigger a Fail state with the message "Spinal Fracture".
6. Test this purely in code first: manually call `ZoneTouched("Pulse")` etc. from a temporary test button or the Unity console, and confirm `AttemptLift()` behaves correctly — before any real detection is hooked up.
7. Build the UI: add a Screen Space Canvas. → wire it to `Assets/Scripts/UI/RescueUIController.cs`
8. Add a text field showing the current step prompt (e.g. "Check the victim's pulse").
9. Add a result text field for the pass/fail message.
10. Add a "Lift Patient" button, wired to call `AttemptLift()` when pressed.
11. Once Person B1's real detection script is ready and calling `ZoneTouched()` correctly, connect and test together.
12. Push this branch once the full B-side logic is tested end-to-end with B1's real detection.

## Output of Pair B
A working detection-and-logic system that correctly identifies which zone was tapped, tracks
progress through the pulse/breathing/neck-brace sequence, and shows the correct pass or
"Spinal Fracture" fail message when the Lift Patient button is pressed — ready to be merged
onto Pair A's scene by the Integrator.

## If You Get Stuck
- **Tap doesn't register at all:** check the collider has "Is Trigger" on, and that a Rigidbody exists on at least one side (can be kinematic).
- **Zone identified as wrong type:** double-check the exact tag spelling matches on both the object and the script.
- **`RescueManager.Instance` is null:** make sure `Awake()` sets `Instance = this`, and the RescueManager object exists in the scene before anything tries to call it.
- **Stuck more than 20–30 minutes on one issue:** flag the Integrator immediately, don't sit on it silently.
