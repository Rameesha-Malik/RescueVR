using UnityEngine;
using RescueVR.Core;

namespace RescueVR.Interaction
{
    /// <summary>
    /// Pair B1 — Touch Detection.
    ///
    /// Detects a screen tap, raycasts from the camera through the tap point, and — if it
    /// hits an object tagged Zone_Pulse / Zone_Breathing / Zone_Neck — reports it to
    /// RescueManager using the exact shared strings "Pulse" / "Breathing" / "Neck".
    ///
    /// Works against plain placeholder cubes first (per the work plan) and, once Pair A2
    /// pushes the real tagged character, keeps working unchanged — it only cares about tags,
    /// not what the object looks like.
    ///
    /// Editor setup:
    /// 1. Put this script on any always-active object in the scene (e.g. the XR Origin, or an
    ///    empty "InputManager" object).
    /// 2. Make sure a RescueManager exists in the scene (see RescueManager.cs).
    /// 3. Each zone object needs a Collider with "Is Trigger" ON and the correct tag, and a
    ///    Rigidbody somewhere in the hierarchy that participates in physics raycasts
    ///    (a kinematic Rigidbody on the zone or the character root is enough).
    /// </summary>
    public class ZoneTouchDetector : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private Camera raycastCamera;
        [SerializeField] private LayerMask raycastLayers = ~0; // everything by default
        [SerializeField] private float maxRayDistance = 100f;

        [Header("Editor Testing")]
        [Tooltip("Also accept left mouse clicks in the Editor / on desktop, so you can test " +
                 "without a phone attached.")]
        [SerializeField] private bool allowMouseInEditor = true;

        private const string TAG_PULSE = "Zone_Pulse";
        private const string TAG_BREATHING = "Zone_Breathing";
        private const string TAG_NECK = "Zone_Neck";

        private void Awake()
        {
            if (raycastCamera == null)
            {
                raycastCamera = Camera.main;
            }
        }

        private void Update()
        {
            // Phone / tablet touch input.
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    TryDetectZone(touch.position);
                }
                return;
            }

            // Desktop / Editor fallback so this can be tested without a device.
#if UNITY_EDITOR || UNITY_STANDALONE
            if (allowMouseInEditor && Input.GetMouseButtonDown(0))
            {
                TryDetectZone(Input.mousePosition);
            }
#endif
        }

        private void TryDetectZone(Vector2 screenPosition)
        {
            if (raycastCamera == null)
            {
                Debug.LogWarning("[ZoneTouchDetector] No camera assigned/found — cannot raycast.");
                return;
            }

            Ray ray = raycastCamera.ScreenPointToRay(screenPosition);

            // Tapping empty space should do nothing, not throw — Physics.Raycast returning
            // false already handles that safely.
            if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, raycastLayers))
            {
                Debug.Log($"[ZoneTouchDetector] Tap at {screenPosition} hit nothing.");
                return;
            }

            Debug.Log($"[ZoneTouchDetector] Tap at {screenPosition} hit '{hit.collider.name}' " +
                      $"(tag: '{hit.collider.tag}').");

            string zoneName = MapTagToZoneName(hit.collider.tag);
            if (zoneName == null)
            {
                // Hit something real (e.g. the car, the ground) that just isn't a zone — ignore.
                return;
            }

            if (RescueManager.Instance == null)
            {
                Debug.LogWarning("[ZoneTouchDetector] Hit a valid zone, but no RescueManager " +
                    "exists in the scene to report it to.");
                return;
            }

            RescueManager.Instance.ZoneTouched(zoneName);
            Debug.Log($"[ZoneTouchDetector] Reported '{zoneName}' to RescueManager.");
        }

        private static string MapTagToZoneName(string colliderTag)
        {
            switch (colliderTag)
            {
                case TAG_PULSE: return "Pulse";
                case TAG_BREATHING: return "Breathing";
                case TAG_NECK: return "Neck";
                default: return null;
            }
        }
    }
}
