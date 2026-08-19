using System;
using UnityEngine;

namespace RescueVR.Core
{
    /// <summary>
    /// Pair B2 — Sequence &amp; Feedback Logic.
    ///
    /// Central decision-making system for the rescue sequence. Any script in the
    /// project (touch detection, VR trigger zones, debug buttons, etc.) reports a
    /// completed check by calling:
    ///
    ///     RescueManager.Instance.ZoneTouched("Pulse");
    ///     RescueManager.Instance.ZoneTouched("Breathing");
    ///     RescueManager.Instance.ZoneTouched("Neck");
    ///
    /// These three exact strings are the shared contract with Pair B1's detection
    /// script (see ZoneTouchDetector.cs) — do not rename them without updating
    /// both sides and telling the whole team.
    ///
    /// Wiring notes for the Editor:
    /// 1. Put this script on a single empty GameObject in the scene, e.g. "RescueManager".
    /// 2. Subscribe UI (RescueUIController.cs) to OnStepPromptChanged / OnResult.
    /// 3. Wire the "Lift Patient" button's OnClick() to RescueManager.Instance.AttemptLift().
    /// </summary>
    public class RescueManager : MonoBehaviour
    {
        public static RescueManager Instance { get; private set; }

        [Header("Sequence State (read-only, visible for debugging)")]
        [SerializeField] private bool pulseChecked;
        [SerializeField] private bool breathingChecked;
        [SerializeField] private bool neckBraced;

        [Header("Rules")]
        [Tooltip("If enabled, zones must be checked in the exact order Pulse -> Breathing -> Neck. " +
                 "A zone touched out of order is ignored and reported as a mistake instead of being " +
                 "silently accepted. Matches the stricter 'in the correct order' rule from the " +
                 "team methodology; leave off if Pair B just wants 'all three before lift'.")]
        [SerializeField] private bool enforceOrder = false;

        public bool PulseChecked => pulseChecked;
        public bool BreathingChecked => breathingChecked;
        public bool NeckBraced => neckBraced;
        public bool AllChecksComplete => pulseChecked && breathingChecked && neckBraced;

        /// <summary>Fired whenever the current step prompt should update in the UI.</summary>
        public event Action<string> OnStepPromptChanged;

        /// <summary>Fired with (success, message) when AttemptLift() resolves, or when an
        /// out-of-order / duplicate touch happens while enforceOrder is on.</summary>
        public event Action<bool, string> OnResult;

        /// <summary>Fired every time a valid zone is registered, in case something else
        /// (audio cues, animation, scoring) wants to react without caring about prompts.</summary>
        public event Action<string> OnZoneRegistered;

        private const string ZONE_PULSE = "Pulse";
        private const string ZONE_BREATHING = "Breathing";
        private const string ZONE_NECK = "Neck";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[RescueManager] Duplicate RescueManager in scene — destroying the new one. " +
                                  "Only one should exist.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            RaiseStepPrompt();
        }

        /// <summary>
        /// Called by whichever detection script (touch raycast on mobile AR, or a VR
        /// controller trigger zone) determines the user interacted with a body-part zone.
        /// Exact accepted values: "Pulse", "Breathing", "Neck". Anything else is logged and ignored.
        /// </summary>
        public void ZoneTouched(string zoneName)
        {
            switch (zoneName)
            {
                case ZONE_PULSE:
                    if (enforceOrder && (breathingChecked || neckBraced))
                    {
                        ReportMistake($"You already moved past the pulse check — restart the sequence.");
                        return;
                    }
                    pulseChecked = true;
                    break;

                case ZONE_BREATHING:
                    if (enforceOrder && !pulseChecked)
                    {
                        ReportMistake("Check the pulse before checking breathing.");
                        return;
                    }
                    breathingChecked = true;
                    break;

                case ZONE_NECK:
                    if (enforceOrder && (!pulseChecked || !breathingChecked))
                    {
                        ReportMistake("Check pulse and breathing before applying the neck brace.");
                        return;
                    }
                    neckBraced = true;
                    break;

                default:
                    Debug.LogWarning($"[RescueManager] ZoneTouched() got an unrecognized zone name: " +
                                      $"\"{zoneName}\". Expected exactly \"Pulse\", \"Breathing\", or \"Neck\".");
                    return;
            }

            OnZoneRegistered?.Invoke(zoneName);
            RaiseStepPrompt();
        }

        /// <summary>
        /// Wired to the "Lift Patient" button. Succeeds only if all three checks are done;
        /// otherwise triggers the "Spinal Fracture" fail state.
        /// </summary>
        public void AttemptLift()
        {
            if (AllChecksComplete)
            {
                OnResult?.Invoke(true, "Great job! Patient stabilized.");
            }
            else
            {
                OnResult?.Invoke(false, "SPINAL FRACTURE — Restart");
            }
        }

        /// <summary>Resets all flags so the scenario can be replayed without reloading the scene.</summary>
        public void ResetSequence()
        {
            pulseChecked = false;
            breathingChecked = false;
            neckBraced = false;
            RaiseStepPrompt();
        }

        private void ReportMistake(string message)
        {
            Debug.Log($"[RescueManager] Out-of-order attempt: {message}");
            OnResult?.Invoke(false, message);
        }

        private void RaiseStepPrompt()
        {
            string prompt;
            if (!pulseChecked) prompt = "Check the victim's pulse.";
            else if (!breathingChecked) prompt = "Check the victim's breathing.";
            else if (!neckBraced) prompt = "Apply the neck brace.";
            else prompt = "All checks complete — lift the patient.";

            OnStepPromptChanged?.Invoke(prompt);
        }
    }
}
