using UnityEngine;
using UnityEngine.UI;
using RescueVR.Core;

namespace RescueVR.UI
{
    /// <summary>
    /// Pair B2 — Feedback UI.
    ///
    /// Listens to RescueManager's events and drives the on-screen prompt, the pass/fail
    /// result text, and the "Lift Patient" button.
    ///
    /// Editor setup:
    /// 1. Create a Screen Space Canvas with two Text (or TMP_Text) elements and a Button.
    /// 2. Put this script on the Canvas (or any object) and drag in:
    ///    - Step Prompt Text  -> shows "Check the victim's pulse." etc.
    ///    - Result Text       -> shows the pass/fail message, hidden until AttemptLift() runs.
    ///    - Lift Button       -> its OnClick() can either call RescueManager.AttemptLift()
    ///                           directly in the Inspector, OR be left unwired and driven
    ///                           entirely from this script (see Awake below) — pick one to
    ///                           avoid calling AttemptLift() twice.
    /// </summary>
    public class RescueUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text stepPromptText;
        [SerializeField] private Text resultText;
        [SerializeField] private Button liftButton;

        [Header("Behaviour")]
        [Tooltip("If true, this script wires the Lift button's onClick itself — do NOT also " +
                 "wire AttemptLift() to the button in the Inspector, or it will fire twice.")]
        [SerializeField] private bool wireButtonInCode = true;

        private void Awake()
        {
            if (resultText != null)
            {
                resultText.text = string.Empty;
            }

            if (wireButtonInCode && liftButton != null)
            {
                liftButton.onClick.AddListener(HandleLiftButtonPressed);
            }
        }

        private void OnEnable()
        {
            if (RescueManager.Instance != null)
            {
                Subscribe(RescueManager.Instance);
            }
        }

        private void OnDisable()
        {
            if (RescueManager.Instance != null)
            {
                Unsubscribe(RescueManager.Instance);
            }
        }

        private void Subscribe(RescueManager manager)
        {
            manager.OnStepPromptChanged += HandleStepPromptChanged;
            manager.OnResult += HandleResult;
        }

        private void Unsubscribe(RescueManager manager)
        {
            manager.OnStepPromptChanged -= HandleStepPromptChanged;
            manager.OnResult -= HandleResult;
        }

        private void HandleLiftButtonPressed()
        {
            RescueManager.Instance?.AttemptLift();
        }

        private void HandleStepPromptChanged(string prompt)
        {
            if (stepPromptText != null)
            {
                stepPromptText.text = prompt;
            }
        }

        private void HandleResult(bool success, string message)
        {
            if (resultText == null)
            {
                return;
            }

            resultText.text = message;
            resultText.color = success ? Color.green : Color.red;
        }
    }
}
