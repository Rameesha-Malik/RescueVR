using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace RescueVR.AR
{
    /// <summary>
    /// Pair A1 — AR Scene Setup.
    ///
    /// On screen tap, raycasts against detected AR planes and instantiates the crashed-car
    /// prefab (or a placeholder cube, if no prefab is assigned yet) at the hit position.
    ///
    /// Editor setup:
    /// 1. Create: GameObject -> XR -> AR Session, and GameObject -> XR -> XR Origin (AR).
    /// 2. On the XR Origin, add "AR Plane Manager" and "AR Raycast Manager" components.
    /// 3. Put this script on the XR Origin (or any object) and drag the AR Raycast Manager
    ///    reference in. Assign the crashed-car prefab once imported from the Asset Store.
    /// 4. Build & Run to an ARCore-capable Android phone to test — this will not do anything
    ///    useful in the plain Editor Game view since there's no real camera feed / planes.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class TapToPlace : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;

        [Header("What to Place")]
        [Tooltip("The crashed car prefab. Leave empty during early testing — a placeholder " +
                 "cube will be spawned instead so you can verify placement works before the " +
                 "real model is imported.")]
        [SerializeField] private GameObject placementPrefab;

        [Header("Behaviour")]
        [Tooltip("If false, only the first tap places the object; further taps are ignored. " +
                 "Turn this on while testing placement repeatedly on different surfaces.")]
        [SerializeField] private bool allowReplacing = true;

        private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();
        private GameObject spawnedInstance;

        private void Awake()
        {
            if (raycastManager == null)
            {
                raycastManager = GetComponent<ARRaycastManager>();
            }

            if (planeManager == null)
            {
                planeManager = GetComponent<ARPlaneManager>();
            }
        }

        private void Update()
        {
            if (Input.touchCount == 0)
            {
                return;
            }

            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began)
            {
                return;
            }

            if (spawnedInstance != null && !allowReplacing)
            {
                return;
            }

            if (!raycastManager.Raycast(touch.position, Hits, TrackableType.PlaneWithinPolygon))
            {
                return;
            }

            Pose hitPose = Hits[0].pose;
            PlaceObject(hitPose);
        }

        private void PlaceObject(Pose pose)
        {
            if (spawnedInstance == null)
            {
                GameObject prefabToUse = placementPrefab != null ? placementPrefab : BuildFallbackCube();
                spawnedInstance = Instantiate(prefabToUse, pose.position, pose.rotation);
            }
            else
            {
                spawnedInstance.transform.SetPositionAndRotation(pose.position, pose.rotation);
            }
        }

        /// <summary>
        /// Placeholder used only until the real crashed-car prefab is imported and assigned,
        /// so placement logic can be tested end-to-end from hour one.
        /// </summary>
        private static GameObject BuildFallbackCube()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "PlaceholderCube (replace with crashed car prefab)";
            cube.transform.localScale = Vector3.one * 0.3f;
            return cube;
        }
    }
}
