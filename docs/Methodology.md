# RescueVR — Complete Beginner Methodology
### CATCH_VR Erasmus+ Summer School 2026, GIK Institute

---

## 0. Before You Touch Unity — What You Need

### Accounts (free, set up first — takes 15 min total)
- [ ] **Unity ID** — unity.com/account (required to activate Unity Hub license, per bootcamp SOP)
- [ ] **Mixamo account** — mixamo.com (free, owned by Adobe, sign in with Adobe ID) — this is where your injured "victim" character and animations come from
- [ ] **GitHub account** — github.com (free) — to download/reference existing open-source VR first-aid projects
- [ ] **Claude.ai Pro account** — you already have this; see Section 6 for exactly how to use it

### Software (install on your lab PC)
- [ ] **Unity Hub** + **Unity 2022 LTS** (install via Hub, add "Android Build Support" + "Windows Build Support" modules)
- [ ] **XR Interaction Toolkit** (installed inside Unity via Package Manager — Section 2 shows exactly how)
- [ ] **Visual Studio Code** or Visual Studio (for editing C# scripts — usually auto-installed with Unity)
- [ ] **Blender** (optional — only needed if you must edit a 3D model; skip if using ready-made assets)

### Hardware
- [ ] **VR headset** — Meta Quest (2/3) or whatever GIKI provides. **Confirm you physically have access to this right now, before writing any code.** This is your single biggest risk — verify it today.
- [ ] **USB-C cable or Quest Link/Air Link setup** — to connect headset to your dev PC for testing (Meta Quest Developer Mode must be enabled — Section 1 covers this)
- [ ] **Desktop/laptop** capable of running Unity smoothly (any GIKI lab PC should be fine)

### Free assets you'll need (all download links in Section 1)
- [ ] A crashed car 3D model (Unity Asset Store, free tier)
- [ ] A human character + animations (Mixamo — free)
- [ ] A neck brace / medical prop model (Asset Store or simple primitive shapes if none found)

---

## 1. Phase 1 — Environment Setup (Hours 0–4)

**Goal: an empty but running VR project where you can put on the headset and look around a 3D scene.**

### Step 1.1 — Create the Unity project
1. Open Unity Hub → New Project → select **3D (URP)** template (URP = Universal Render Pipeline, better performance on Quest)
2. Name it `RescueVR`
3. Once it opens, go to **Window → Package Manager**
4. Install these packages (search by name, click Install):
   - `XR Interaction Toolkit`
   - `XR Plugin Management`
   - `OpenXR Plugin` (works across most headsets, including Quest)

**Output of this step:** Unity opens with no errors, packages show as installed in Package Manager.

### Step 1.2 — Configure XR settings
1. **Edit → Project Settings → XR Plug-in Management**
2. Under the **Android tab** (for standalone Quest) and **Windows tab** (for PC-linked testing), check **OpenXR**
3. Click into OpenXR settings, add **Meta Quest Support** (or your headset's interaction profile) under "Interaction Profiles"

**Output:** No red error icons in XR Plug-in Management panel.

### Step 1.3 — Get a working VR camera rig
1. In your Scene, delete the default Main Camera
2. Go to **GameObject → XR → XR Origin (VR)** — this creates a pre-built camera rig with head tracking and both hand controllers already wired up
3. Press Play (even without a headset connected, this confirms no errors)

**Output:** An `XR Origin` object in your Hierarchy with `Camera Offset`, `Left Controller`, `Right Controller` as children.

### Step 1.4 — Test on the actual headset (do this TODAY, not later)
1. On Quest: **Settings → System → Developer Mode → ON** (you may need a Meta developer account, free, sign up at developer.oculus.com)
2. Connect via USB-C cable, allow debugging prompt on headset
3. In Unity: **File → Build Settings → Android → Switch Platform**
4. Click **Build and Run**

**Output:** You put on the headset and see the empty Unity scene in 3D, and moving the controllers moves the hands in-game. **If this doesn't work, stop everything else and fix this first — this is your foundation.**

---

## 2. Phase 2 — Reference Existing Projects (Hours 4–6)

This step will save you significant time. Real, working, open-source VR first-aid/CPR training projects already exist — you're not starting from a blank page conceptually.

### Projects worth downloading and studying:

| Project | What it shows you | Link |
|---|---|---|
| **SamiSha99/CPR-VR** | Closest match to your idea — controller + hand tracking, gamifies a medical procedure, gives feedback/scoring, uses Unity+C# | github.com/SamiSha99/CPR-VR |
| **anddegilevich/CPRSimulator** | Full "assess victim → perform correct sequence → get evaluated" flow, SteamVR-based | github.com/anddegilevich/CPRSimulator |
| **sharnajh/VR_CPR_Training** | Simple, beginner-readable Unity C# — assessing + performing steps + a knowledge quiz | github.com/sharnajh/VR_CPR_Training |
| **2hiTee/COMP0113-Group-Project** | "VR First Aid Training" — broader than just CPR, closer to your general first-aid framing | github.com/2hiTee/COMP0113-Group-Project |

### What to actually do with these
1. **Don't blindly import them into your own project** — they may use different Unity/XR Toolkit versions and could break things
2. **Clone/download them separately**, open each in its own Unity instance, and **study their scripts** — specifically look for:
   - How they detect "did the user touch/interact with the right body part?"
   - How they sequence steps (Step 1 must happen before Step 2)
   - How they show pass/fail feedback
3. **Copy the *pattern*, rewrite the code yourself** in your own project — this is faster than fighting someone else's project structure, and you'll actually understand your own code when judges ask questions

**Output of this phase:** Your team has looked at real working code and has a clear mental model of "trigger zone + sequence checker + feedback UI" — the three pieces you need to build.

---

## 3. Phase 3 — Scene & Character (Hours 6–12)

### Step 3.1 — Get your crashed car
1. Unity Asset Store (inside Unity: **Window → Asset Store**, or via browser at assetstore.unity.com) → search "crashed car" or "damaged car free"
2. Filter by **Free**, download and import
3. Drag it into your scene, position it reasonably (roadside, slightly angled)

### Step 3.2 — Get your victim character
1. Go to mixamo.com, sign in
2. Search characters — pick any humanoid (e.g., "Y Bot" or a more realistic free character)
3. Pick an **idle/injured pose animation** (search "lying down," "injured idle," or similar)
4. Download as **FBX for Unity**, with skin
5. Drag the FBX into your Unity **Assets** folder, then drag it into your Scene near the car
6. Position/rotate it to look like it's lying on the ground near the crashed car

**Output:** A scene with a car and a humanoid character lying on the ground next to it, viewable in VR.

### Step 3.3 — Mark the interaction zones
1. On the character, create 3 empty child GameObjects positioned at: **wrist (pulse point)**, **chest (breathing check)**, **neck (brace application)**
2. Add a **Sphere Collider** (set to "Is Trigger") to each — these are invisible zones your controller will need to touch
3. Name them clearly: `Zone_Pulse`, `Zone_Breathing`, `Zone_Neck`

**Output:** Three trigger zones positioned correctly on the character, visible as gizmos in the Unity editor (not visible in actual VR — that's correct, they're invisible detection zones).

---

## 4. Phase 4 — Interaction Logic (Hours 12–20)

This is the technical core. Assign this to your strongest programmer(s), and use Claude Pro heavily here (see Section 6).

### Step 4.1 — Detect controller touching a zone
Write a script (e.g., `TriggerZone.cs`) attached to each zone that detects when the VR controller (which has its own collider) enters it. Basic pattern:

```csharp
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("VRController"))
    {
        // this zone was touched — report to a central manager
        RescueManager.Instance.ZoneTouched(zoneName);
    }
}
```

*(Your VR controller GameObjects need a small collider + the tag "VRController" set on them for this to work.)*

### Step 4.2 — Build the sequence checker
A central script (`RescueManager.cs`) tracks: has Pulse been checked? Has Breathing been checked? Has Neck brace been applied? **In the correct order?**
- If the user tries to move/lift the victim (you can simulate this as a 4th "Zone_Lift" trigger, or a button press) **before** the neck brace step is done → trigger **"Spinal Fracture" fail state**
- If all 3 steps are completed correctly in order, then lifting succeeds → trigger **"Success" state**

### Step 4.3 — Feedback UI
- A simple **World Space Canvas** (a UI panel that exists in 3D space, visible in VR) showing:
  - Current step prompt ("Check the victim's pulse")
  - Pass/fail result text ("SPINAL FRACTURE — Restart" or "Great job! Patient stabilized.")
- Optional: audio cue (a simple "success chime" or "failure buzz" sound clip, free ones available on freesound.org)

**Output of this phase:** You can put on the headset, reach out and touch each zone in order, and see the UI update correctly. Touching zones out of order or attempting to lift early shows the fail message.

---

## 5. Phase 5 — Polish, Test, Rehearse (Hours 20–24, or 20–46 if you have 48h)

1. **Playtest repeatedly yourselves** — have someone who *hasn't* seen the code try it blind. If they get confused about what to do, add clearer on-screen prompts.
2. **Fix hand/controller collider size** — this is the #1 thing that breaks in VR interaction demos (too small = frustrating, too big = unrealistic). Tune it based on real testing in the headset, not guessing in the editor.
3. **Record a backup video** of a successful full run-through on a phone, in case the live headset demo glitches during judging — this is a safety net, not a replacement for the live demo.
4. **Rehearse the 3-minute pitch**, using your CATCH_VR pitch deck template:
   - Slide 1: Title — "RescueVR"
   - Slide 2: Problem → Solution (the spinal injury statistic + your VR training answer)
   - Slide 3: How it works (Input: VR controllers → System: Unity XR + NPC victim → Logic: sequence + fail-state checks → Output: pass/fail report)
   - Slide 4: Evidence — show a screenshot/clip of the working sequence catching a mistake
   - Slide 5: Demo & close — live headset demo if possible, video backup ready

---

## 6. How Claude (Pro Account) Can Actually Help You, Step by Step

Since you have Claude Pro, here's specifically where to use it at each phase — this will save you real hours:

**Phase 1 (Setup):**
- Paste any Unity/XR error message directly into Claude and ask "what does this error mean and how do I fix it" — this is often faster than searching forums
- Ask Claude to explain unfamiliar Unity concepts in plain language (e.g., "explain what an XR Origin does in simple terms")

**Phase 3 (Scene/Character):**
- Ask Claude to help you write a short **C# script for a simple idle animation trigger** if Mixamo's default doesn't look right
- Ask Claude for guidance on positioning/scaling objects correctly relative to the XR Origin (a common beginner mistake — reference the "hand touches zone" step, since misaligned Sphere Colliders often need repositioning)

**Phase 4 (Interaction Logic — biggest time-saver):**
- **This is where Claude Code (if available on your Pro plan) is most valuable** — you can literally hand it your project's file structure and have it write, debug, and refine the `TriggerZone.cs` and `RescueManager.cs` scripts directly
- Paste your actual script and ask "review this and tell me what's wrong" when something doesn't trigger correctly — this is much faster than trial-and-error debugging alone
- Ask Claude to generate the sequence-checking logic pseudocode first in plain English, so your whole team understands the plan before anyone writes code
- If stuck on collider/trigger detection not firing, paste your exact setup (tags, collider settings) and ask Claude to spot the mismatch — this is one of the most common Unity beginner bugs and very diagnosable from a description

**Phase 5 (Polish/Rehearse):**
- Ask Claude to review your pitch script for clarity and timing (does it fit 3 minutes?)
- Ask Claude to help you write clear, natural-sounding UI prompt text ("Check the victim's pulse" vs more awkward phrasing)
- If you want, ask Claude to role-play as a skeptical judge and ask you tough questions about your project, so you can rehearse answers

**General tip:** Keep one ongoing Claude conversation as your "project log" — paste in what you built each phase and ask it to help you track what's left, so you don't lose momentum switching between coding and planning.

---

## 7. Should You Just Fork an Existing GitHub Project Instead of Building From Scratch?

**Honest answer: reference them, don't fork them wholesale.** Here's why:

- These projects (SamiSha99/CPR-VR, CPRSimulator, etc.) are built around **CPR chest-compression detection**, not **your exact scenario** (car crash → pulse/breathing check → neck brace → lift). The core mechanic is different enough that adapting someone else's full project will likely take *longer* than writing your own simpler version, because you'll spend time understanding and stripping out code you don't need.
- They're also built on different Unity/XR Toolkit versions in some cases, which can cause frustrating compatibility errors that eat your limited time.
- **What IS worth reusing:** the *general pattern* of "trigger zone → sequence check → pass/fail feedback," which you now understand from Phase 2. Also feel free to lift specific small pieces (e.g., a UI feedback panel design, or how they structured a "step manager" script) if you find one that's clean and readable — just don't try to import and adapt an entire foreign project under time pressure.

---

## 8. Quick Troubleshooting Reference

| Problem | Likely cause | Fix |
|---|---|---|
| Controllers don't move in headset | XR Plug-in Management not configured for your platform | Redo Step 1.2, check correct tab (Android for standalone Quest) |
| Trigger zone doesn't detect touch | Collider not set to "Is Trigger," or missing Rigidbody on one side, or wrong tag | Check both colliders — one needs a Rigidbody (can be kinematic) |
| Character looks frozen/T-pose | Animation not applied, or Animator Controller missing | Re-check Mixamo export included "with skin," reapply Animator |
| Build fails for Android | Android Build Support module not installed in Unity Hub | Unity Hub → Installs → your Unity version → Add Modules |
| Scene runs in editor but not in headset | Common — editor Play mode doesn't always reflect real device performance | Always test builds on-device early and often, not just in editor |

---

## 9. Final Checklist Before Demo Day

- [ ] Full sequence (pulse → breathing → neck brace → lift) works correctly on the actual headset, tested at least 5 times without failure
- [ ] Fail state ("Spinal Fracture") triggers correctly when steps are skipped/wrong order
- [ ] Backup video recorded
- [ ] Pitch deck built from CATCH_VR template, rehearsed under 3 minutes
- [ ] Headset battery charged, USB cable/build tested morning-of
- [ ] Every team member knows the full flow, not just their own module — anyone should be able to explain and demo it if needed
