# RescueVR

A mobile-AR first-aid training scenario built for the **CATCH_VR Erasmus+ Summer School 2026**
(GIK Institute). A user points their phone at a real surface, places a crashed-car scene with
an injured victim, and must correctly check the victim's **pulse → breathing → neck brace**
before attempting to lift them — skipping a step or lifting too early triggers a
**"Spinal Fracture"** fail state.

> This repo *is* the Unity project. Clone it, open the folder directly in Unity Hub
> (`Add project from disk`), and Unity will resolve the packages listed below on first open.

## Tech stack
- Unity **2022 LTS** (`ProjectSettings/ProjectVersion.txt` pins `2022.3.21f1` — a nearby 2022
  LTS patch version will open fine, Unity will just ask to confirm)
- URP (Universal Render Pipeline)
- AR Foundation + ARCore XR Plugin (Android mobile AR — camera passthrough, plane detection,
  tap-to-place)
- XR Interaction Toolkit + XR Management (kept available for an optional VR-headset input path;
  the current build target is phone AR, see [Why AR and not a headset?](#why-ar-and-not-a-headset))
- TextMeshPro / uGUI for the in-scene feedback UI

## Repo structure
```
Assets/
  Scripts/
    AR/          TapToPlace.cs            — Pair A1: tap a detected plane, spawn the car
    Character/   VictimZoneSetup.cs       — Pair A2: auto-creates the 3 tagged trigger zones
    Interaction/ ZoneTouchDetector.cs     — Pair B1: screen tap -> raycast -> zone name
    Core/        RescueManager.cs         — Pair B2: sequence state machine, pass/fail rules
    UI/          RescueUIController.cs    — Pair B2: wires RescueManager to on-screen text/button
Packages/manifest.json                    — required Unity packages
ProjectSettings/
  ProjectVersion.txt                      — pinned Unity version
  TagManager.asset                        — pre-registers Zone_Pulse / Zone_Breathing / Zone_Neck
docs/
  PairA_WorkPlan.md                       — full Pair A task breakdown
  PairB_WorkPlan.md                       — full Pair B task breakdown
  INTEGRATION.md                          — merge + wiring + end-to-end test checklist
  Methodology.md                          — full bootcamp methodology (setup, phases, pitch)
```

**What isn't in this repo yet, and can't be added by an agent:** the imported crashed-car
model, the Mixamo victim FBX, the built scene files, and any Editor-only wiring (dragging
component references onto GameObjects, building the Canvas UI, tagging zones on the actual
imported character). Those require the Unity Editor GUI and Mixamo/Asset Store logins — every
script above has an "Editor setup" doc-comment block at the top telling you exactly what to
create and connect by hand. Follow `docs/PairA_WorkPlan.md` and `docs/PairB_WorkPlan.md` in
order and you'll hit every one of those steps.

## The shared contract (get this exactly right)
Both pairs' scripts already implement this — just don't rename anything without updating both
sides:
- Zone tags on the victim character: `Zone_Pulse`, `Zone_Breathing`, `Zone_Neck`
- The function every detection script reports through:
  `RescueManager.Instance.ZoneTouched(string zoneName)`
- The exact strings passed in: `"Pulse"`, `"Breathing"`, `"Neck"`

## Getting started
1. Install **Unity Hub** → install **Unity 2022 LTS** with the **Android Build Support**
   module (and its OpenJDK + Android SDK/NDK sub-modules).
2. In Unity Hub: **Add → Add project from disk** → select this cloned folder.
3. Let Unity resolve packages from `Packages/manifest.json` on first open (may take a few
   minutes).
4. Read `docs/PairA_WorkPlan.md` or `docs/PairB_WorkPlan.md` depending on your role, and work
   through it top to bottom — each numbered step tells you which script (if any) already
   covers it.
5. When both branches are ready, follow `docs/INTEGRATION.md` to merge and verify end-to-end.

## Why AR and not a headset?
The original bootcamp methodology (`docs/Methodology.md`) describes a VR-headset build
(Quest, XR Origin (VR), controller-collider trigger zones). The team's actual Pair A/B task
breakdown pivoted to **phone-based AR** (AR Session, XR Origin (AR), AR Plane Manager, screen
taps instead of controllers) — that's what's implemented here, since it's the concrete,
current spec. The XR Interaction Toolkit packages are still included so a headset input path
could be added later without re-architecting `RescueManager.cs` (it doesn't know or care
whether `ZoneTouched()` was called by a touchscreen raycast or a VR controller trigger).

## Troubleshooting
See the "If You Get Stuck" sections in `docs/PairA_WorkPlan.md`, `docs/PairB_WorkPlan.md`, and
the fuller table in `docs/Methodology.md` §8.

## Demo day checklist
See `docs/Methodology.md` §9 for the full pre-demo checklist (5 clean run-throughs, backup
video, pitch deck rehearsed under 3 minutes, etc.).
