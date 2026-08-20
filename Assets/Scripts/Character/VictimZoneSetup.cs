using UnityEngine;

namespace RescueVR.Character
{
    /// <summary>
    /// Pair A2 — Character &amp; Animation helper.
    ///
    /// Auto-creates the three interaction zones (wrist/pulse, chest/breathing, neck/brace) as
    /// correctly-tagged, correctly-configured child trigger colliders, so nobody has to
    /// hand-type "Zone_Pulse" / "Zone_Breathing" / "Zone_Neck" into the tag field and risk a
    /// typo — which the whole team was warned is the #1 source of integration bugs.
    ///
    /// Editor setup:
    /// 1. Add this component to the Mixamo character's root GameObject, after it's in the scene.
    /// 2. Adjust the local offsets in the Inspector so they roughly line up with the wrist,
    ///    chest, and neck of the actual model (they default to a rough humanoid layout).
    /// 3. Right-click the component header (or the ⋮ menu) and choose "Create Zones".
    ///    This is safe to re-run — it won't duplicate zones that already exist.
    /// 4. IMPORTANT: the Zone_Pulse / Zone_Breathing / Zone_Neck tags must already exist in
    ///    Edit -> Project Settings -> Tags and Layers (they're pre-added in this project's
    ///    ProjectSettings/TagManager.asset, but if Unity ever regenerates that file, re-add them
    ///    manually before running this).
    /// </summary>
    public class VictimZoneSetup : MonoBehaviour
    {
        [Header("Local Offsets (relative to this object)")]
        [SerializeField] private Vector3 pulseOffset = new Vector3(0.35f, 0.9f, 0f);
        [SerializeField] private Vector3 breathingOffset = new Vector3(0f, 1.3f, 0.1f);
        [SerializeField] private Vector3 neckOffset = new Vector3(0f, 1.55f, 0f);

        [Header("Zone Size")]
        [SerializeField] private float zoneRadius = 0.08f;

        [Header("Debug Visibility")]
        [Tooltip("Adds a small colored sphere at each zone so you can actually see where to " +
                 "tap while testing in the Editor/on a phone. The real trigger collider stays " +
                 "invisible either way — these markers are purely visual and have no collider " +
                 "of their own, so they never interfere with tap detection. Turn this off (and " +
                 "re-run Create Zones, or use 'Remove Debug Markers') before your final demo if " +
                 "you want the zones fully invisible again.")]
        [SerializeField] private bool showDebugMarkers = true;

        private const string ZONE_PULSE_NAME = "Zone_Pulse";
        private const string ZONE_BREATHING_NAME = "Zone_Breathing";
        private const string ZONE_NECK_NAME = "Zone_Neck";
        private const string MARKER_NAME = "DebugMarker";

        [ContextMenu("Create Zones")]
        public void CreateZones()
        {
            CreateZoneIfMissing(ZONE_PULSE_NAME, pulseOffset, new Color(1f, 0.3f, 0.3f, 0.85f));
            CreateZoneIfMissing(ZONE_BREATHING_NAME, breathingOffset, new Color(0.3f, 0.8f, 1f, 0.85f));
            CreateZoneIfMissing(ZONE_NECK_NAME, neckOffset, new Color(1f, 0.9f, 0.2f, 0.85f));

            Debug.Log("[VictimZoneSetup] Zones ready: Zone_Pulse (red), Zone_Breathing (blue), " +
                      "Zone_Neck (yellow) — reposition their transforms in the Inspector to match " +
                      "the model. Colored markers are visible for testing; see 'Remove Debug " +
                      "Markers' to hide them again before your final demo.");
        }

        [ContextMenu("Remove Debug Markers")]
        public void RemoveDebugMarkers()
        {
            foreach (string zoneName in new[] { ZONE_PULSE_NAME, ZONE_BREATHING_NAME, ZONE_NECK_NAME })
            {
                Transform zone = transform.Find(zoneName);
                Transform marker = zone != null ? zone.Find(MARKER_NAME) : null;
                if (marker != null)
                {
                    DestroyImmediate(marker.gameObject);
                }
            }
            Debug.Log("[VictimZoneSetup] Debug markers removed — zones are invisible again.");
        }

        private void CreateZoneIfMissing(string zoneName, Vector3 localOffset, Color markerColor)
        {
            Transform existing = transform.Find(zoneName);
            if (existing != null)
            {
                return;
            }

            GameObject zone = new GameObject(zoneName)
            {
                tag = zoneName
            };
            zone.transform.SetParent(transform, worldPositionStays: false);
            zone.transform.localPosition = localOffset;

            SphereCollider collider = zone.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = zoneRadius;

            if (showDebugMarkers)
            {
                CreateDebugMarker(zone.transform, markerColor);
            }
        }

        /// <summary>Purely visual — no collider — so it can never block or redirect a raycast
        /// meant for the real trigger collider on the zone's parent GameObject.</summary>
        private void CreateDebugMarker(Transform parent, Color color)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = MARKER_NAME;
            marker.transform.SetParent(parent, worldPositionStays: false);
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = Vector3.one * (zoneRadius * 2.2f);

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                DestroyImmediate(markerCollider);
            }

            Renderer renderer = marker.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Unlit/Color"))
            {
                color = color
            };
            renderer.sharedMaterial = mat;
        }
    }
}
