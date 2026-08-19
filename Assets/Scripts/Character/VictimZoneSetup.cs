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

        private const string ZONE_PULSE_NAME = "Zone_Pulse";
        private const string ZONE_BREATHING_NAME = "Zone_Breathing";
        private const string ZONE_NECK_NAME = "Zone_Neck";

        [ContextMenu("Create Zones")]
        public void CreateZones()
        {
            CreateZoneIfMissing(ZONE_PULSE_NAME, pulseOffset);
            CreateZoneIfMissing(ZONE_BREATHING_NAME, breathingOffset);
            CreateZoneIfMissing(ZONE_NECK_NAME, neckOffset);

            Debug.Log("[VictimZoneSetup] Zones ready: Zone_Pulse, Zone_Breathing, Zone_Neck " +
                      "(reposition their transforms in the Inspector to match the model).");
        }

        private void CreateZoneIfMissing(string zoneName, Vector3 localOffset)
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
        }
    }
}
