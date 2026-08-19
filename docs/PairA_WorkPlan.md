# RescueVR — Pair A Work Plan
### Scene & Character Module | CATCH_VR Bootcamp, GIK Institute

> CONFIRMED: Pair A starts working the moment the team meeting ends — Hour 0. You do NOT
> need to wait for Pair B. You are building entirely independently in your own test scene,
> and only come together with Pair B later, at the Integrator's checkpoint.

## Team Roles in This Pair
- **Person A1** — AR Scene Setup (camera, plane detection, tap-to-place, car model)
- **Person A2** — Character & Animation (Mixamo victim, zone tagging)

## Shared Names to Remember (agreed with the whole team)
- Zone tags: `Zone_Pulse`, `Zone_Breathing`, `Zone_Neck` (exact spelling, case-sensitive)
- These tags are what Pair B's detection script will look for later — get them exactly right.

## PERSON A1 — Step by Step
Task: Build the AR scene that detects real-world surfaces and places the crashed car when tapped.

1. Open Unity, create a new empty test scene (your own, separate from Pair B).
2. Delete the default Main Camera.
3. Add: **GameObject → XR → AR Session**.
4. Add: **GameObject → XR → XR Origin (AR)**.
5. On the XR Origin, add component: **AR Plane Manager**.
6. On the XR Origin, add component: **AR Raycast Manager**.
7. Switch build platform: **File → Build Settings → Android → Switch Platform**.
8. On your phone: enable Developer Options + USB Debugging (Settings → About Phone → tap Build Number 7 times).
9. Click **Build and Run**. Confirm the camera feed appears on your phone.
10. Write a tap-to-place script: on screen tap, raycast against detected AR planes using AR Raycast Manager, and instantiate a placeholder cube at the hit position. → `Assets/Scripts/AR/TapToPlace.cs`
11. Test repeatedly on your phone — tap a table/floor, confirm the cube appears reliably in different lighting.
12. Import a free crashed car model from the Unity Asset Store (search "crashed car free").
13. Replace the placeholder cube in your placement script with the car prefab (assign it to `TapToPlace.placementPrefab` in the Inspector).
14. Test again — tapping places the car correctly and it looks reasonably sized/positioned.
15. Push this branch to the shared repo. This becomes the foundation the Integrator merges first.

## PERSON A2 — Step by Step
Task: Get an animated, correctly-tagged victim character ready to drop into the AR scene.

1. Go to mixamo.com and sign in with a free Adobe ID.
2. Browse and pick any humanoid character.
3. Search for an injured / lying-down idle animation and select it.
4. Download as **FBX for Unity, with skin**.
5. Import the FBX into the Unity project's `Assets` folder.
6. Drag the character into your OWN blank test scene (do not touch AR yet).
7. Press Play and confirm the animation plays correctly and looks believable.
8. On the character, create 3 empty child GameObjects positioned at: wrist (pulse point), chest (breathing), neck (brace point). → Use `Assets/Scripts/Character/VictimZoneSetup.cs` (right-click the component → "Create Zones") instead of doing this by hand — it creates all three with the correct tags automatically.
9. Add a Collider to each (Sphere or Box), and set **Is Trigger** to ON for each. (Already done if you used `VictimZoneSetup`.)
10. Tag each exactly: `Zone_Pulse`, `Zone_Breathing`, `Zone_Neck`. (Already done if you used `VictimZoneSetup`; the tags are pre-registered in `ProjectSettings/TagManager.asset`.)
11. Check in with Person A1 — once their AR placement pipeline is working, don't wait passively, go ask for it.
12. Combine: place your tagged character next to the car in the working AR scene, positioned naturally on the ground.
13. Push this branch once the character + zones are correctly positioned in the combined scene.

## Output of Pair A
A working AR scene where tapping a real-world surface places a crashed car with a properly
animated, correctly zone-tagged victim character next to it — ready for Pair B's detection
script and the Integrator's first merge.

## If You Get Stuck
- **Camera feed not showing:** check XR Plug-in Management → Android tab → ARCore is checked.
- **Tap doesn't place object:** needs good lighting and a textured (not blank white) flat surface to detect a plane.
- **Character T-poses / frozen:** re-check Mixamo export included "with skin", reapply Animator Controller.
- **Stuck more than 20–30 minutes on one issue:** flag the Integrator immediately, don't sit on it silently.
