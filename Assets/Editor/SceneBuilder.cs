using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEditor;
using UnityEditor.SceneManagement;
using RescueVR.AR;
using RescueVR.Core;
using RescueVR.Interaction;
using RescueVR.UI;

namespace RescueVR.EditorTools
{
    /// <summary>
    /// Builds the Pair B placeholder-cube test scene entirely from code, so nobody has to
    /// hand-create GameObjects, set tags, or drag Inspector references one at a time.
    ///
    /// Run it one of two ways:
    /// 1. Inside the Editor: menu bar -> RescueVR -> Build Pair B Test Scene
    /// 2. Headless from the command line (what a local Claude Code / CI run would use):
    ///    Unity.exe -batchmode -projectPath "<path>" -executeMethod
    ///    RescueVR.EditorTools.SceneBuilder.BuildPairBTestScene -quit
    ///
    /// Either way it saves the result to Assets/Scenes/PairB_TestScene.unity. Open that scene
    /// and press Play — click the three cubes in order, then Lift Patient.
    /// </summary>
    public static class SceneBuilder
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string ScenePath = ScenesFolder + "/PairB_TestScene.unity";
        private const string ARScenePath = ScenesFolder + "/PairA_ARTestScene.unity";

        /// <summary>
        /// Builds Pair A1's AR tap-to-place scene: AR Session, XR Origin (AR) with AR Plane
        /// Manager + AR Raycast Manager, and TapToPlace.cs attached (spawns its built-in
        /// placeholder cube until a real car prefab is assigned in the Inspector).
        ///
        /// Uses Unity's own "GameObject -> XR -> ..." menu commands under the hood instead of
        /// hand-building the AR Foundation hierarchy, since that hierarchy is version-specific
        /// and easy to get subtly wrong by reconstructing it manually — routing through the
        /// real menu command guarantees the same result you'd get clicking it yourself.
        /// </summary>
        [MenuItem("RescueVR/Build Pair A AR Scene")]
        public static void BuildPairAARScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject session = ExecuteMenuAndCaptureNewRoot("GameObject/XR/AR Session");
            // Named "XR Origin (Mobile AR)" as of AR Foundation 5.2.x — older/newer versions
            // may call it "XR Origin (AR)"; if this stops matching, open GameObject > XR in
            // the Editor and check the submenu's exact wording.
            GameObject xrOrigin = ExecuteMenuAndCaptureNewRoot("GameObject/XR/XR Origin (Mobile AR)");

            if (session == null || xrOrigin == null)
            {
                Debug.LogError("[SceneBuilder] Could not create AR Session / XR Origin (AR) via the " +
                    "GameObject > XR menu — those menu items weren't found. This usually means AR " +
                    "Foundation isn't fully installed yet. Open Window > Package Manager and confirm " +
                    "'AR Foundation' shows under 'In Project', then try this again. Falling back: " +
                    "create them by hand via GameObject > XR > AR Session and > XR Origin (AR).");
                return;
            }

            xrOrigin.AddComponent<ARPlaneManager>();
            xrOrigin.AddComponent<ARRaycastManager>();
            xrOrigin.AddComponent<TapToPlace>();

            if (!Directory.Exists(ScenesFolder))
            {
                Directory.CreateDirectory(ScenesFolder);
            }

            EditorSceneManager.SaveScene(scene, ARScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[SceneBuilder] Pair A AR scene built and saved to {ARScenePath}. " +
                      "Build & Run to your Android phone to test tap-to-place (it spawns a debug " +
                      "cube until you assign a real car prefab to TapToPlace > Placement Prefab).");
        }

        /// <summary>Runs a GameObject-creating menu command and returns whichever new root
        /// GameObject appeared in the scene as a result, regardless of what Unity named it.</summary>
        private static GameObject ExecuteMenuAndCaptureNewRoot(string menuPath)
        {
            var scene = EditorSceneManager.GetActiveScene();
            var before = new HashSet<GameObject>(scene.GetRootGameObjects());

            EditorApplication.ExecuteMenuItem(menuPath);

            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (!before.Contains(go))
                {
                    return go;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds Pair B's logic (RescueManager, ZoneTouchDetector, and the feedback UI) into
        /// whichever scene is CURRENTLY OPEN — unlike the two builders above, this does NOT
        /// create a new scene or delete anything. Use this on PairA_ARTestScene once its car,
        /// character, and tagged zones are already in place, to wire up the missing gameplay
        /// logic without disturbing that work. Safe to run more than once — skips anything
        /// that's already present instead of duplicating it.
        ///
        /// Unlike BuildTouchDetector() (used by the standalone Pair B scene), this does NOT
        /// move or reposition Camera.main — in an AR scene the camera's transform is driven by
        /// real-world tracking at runtime, and forcibly repositioning it here would break that.
        /// </summary>
        [MenuItem("RescueVR/Add Pair B Logic To Current Scene")]
        public static void AddPairBLogicToCurrentScene()
        {
            var scene = EditorSceneManager.GetActiveScene();

            if (Object.FindObjectOfType<RescueManager>() == null)
            {
                BuildRescueManager();
                Debug.Log("[SceneBuilder] Added RescueManager.");
            }
            else
            {
                Debug.Log("[SceneBuilder] RescueManager already present — skipped.");
            }

            if (Object.FindObjectOfType<ZoneTouchDetector>() == null)
            {
                AttachTouchDetectorToMainCameraInPlace();
            }
            else
            {
                Debug.Log("[SceneBuilder] ZoneTouchDetector already present — skipped.");
            }

            if (Object.FindObjectOfType<RescueUIController>() == null)
            {
                BuildUI();
                Debug.Log("[SceneBuilder] Added feedback UI (Canvas + prompt/result text + Lift button).");
            }
            else
            {
                Debug.Log("[SceneBuilder] UI already present — skipped.");
            }

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();
            Debug.Log("[SceneBuilder] Pair B logic added to the current scene and saved. " +
                      "Reminder: this only works if the tags on your character are exactly " +
                      "Zone_Pulse / Zone_Breathing / Zone_Neck.");
        }

        /// <summary>
        /// Fixes the UI text styling in whatever scene is currently open: makes the Lift
        /// Patient button's label readable (it defaulted to white-on-white in an earlier
        /// version of this tool) and bolds all three UI text elements for better readability
        /// on a phone screen. Safe to run repeatedly. Use this if your scene already has the
        /// UI built (e.g. via "Add Pair B Logic To Current Scene") and just needs restyling —
        /// it won't rebuild anything, only adjusts existing components in place.
        /// </summary>
        [MenuItem("RescueVR/Fix UI Style In Current Scene")]
        public static void FixUIStyleInCurrentScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            int fixedCount = 0;

            Text[] allTexts = Object.FindObjectsOfType<Text>(includeInactive: true);
            foreach (Text text in allTexts)
            {
                text.fontStyle = FontStyle.Bold;
                fixedCount++;

                // The Lift button's label is the only one sitting on a light background
                // (the button's white Image) — everything else sits on the dark AR view,
                // so only this one needs a dark color; the others stay white.
                if (text.transform.parent != null && text.transform.parent.name == "LiftButton")
                {
                    text.color = Color.black;
                }
            }

            if (fixedCount == 0)
            {
                Debug.LogWarning("[SceneBuilder] No UI Text components found in this scene — " +
                    "run 'Add Pair B Logic To Current Scene' first to build the UI.");
                return;
            }

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();
            Debug.Log($"[SceneBuilder] Restyled {fixedCount} UI text element(s) (bold, Lift button " +
                      "label set to black for readability) and saved the scene.");
        }

        /// <summary>Adds ZoneTouchDetector to whatever camera is currently tagged MainCamera,
        /// without moving it — safe for an AR camera whose transform is driven by tracking.</summary>
        private static void AttachTouchDetectorToMainCameraInPlace()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[SceneBuilder] No camera tagged MainCamera found in this scene — " +
                    "cannot attach ZoneTouchDetector. Make sure XR Origin's camera child is tagged " +
                    "MainCamera.");
                return;
            }

            cam.gameObject.AddComponent<ZoneTouchDetector>();
        }

        [MenuItem("RescueVR/Build Pair B Test Scene")]
        public static void BuildPairBTestScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            BuildRescueManager();
            BuildZone("PulseCube", "Zone_Pulse", new Vector3(-2.2f, 0.5f, 5f));
            BuildZone("BreathingCube", "Zone_Breathing", new Vector3(0f, 0.5f, 5f));
            BuildZone("NeckCube", "Zone_Neck", new Vector3(2.2f, 0.5f, 5f));
            BuildTouchDetector();
            BuildUI();

            if (!Directory.Exists(ScenesFolder))
            {
                Directory.CreateDirectory(ScenesFolder);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[SceneBuilder] Pair B test scene built and saved to {ScenePath}. " +
                      "Open it and press Play, then click the three cubes in order and hit Lift Patient.");
        }

        private static void BuildRescueManager()
        {
            var go = new GameObject("RescueManager");
            go.AddComponent<RescueManager>();
        }

        private static void BuildZone(string name, string tag, Vector3 position)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.tag = tag;
            cube.transform.position = position;

            BoxCollider col = cube.GetComponent<BoxCollider>();
            col.isTrigger = true;

            Rigidbody rb = cube.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        private static void BuildTouchDetector()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }

            cam.transform.position = new Vector3(0f, 1.6f, -4f);
            cam.transform.LookAt(new Vector3(0f, 0.5f, 5f));

            cam.gameObject.AddComponent<ZoneTouchDetector>();
        }

        private static void BuildUI()
        {
            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            // Anchored to the top/bottom edges (not a fixed pixel offset from center) so this
            // stays on-screen regardless of the actual Game view resolution.
            Text stepText = CreateText("StepPromptText", canvasGo.transform, "Check the victim's pulse.");
            AnchorToTop(stepText.rectTransform, -40f);

            Text resultText = CreateText("ResultText", canvasGo.transform, string.Empty);
            AnchorToTop(resultText.rectTransform, -110f);

            GameObject buttonGo = new GameObject("LiftButton", typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(canvasGo.transform, false);
            RectTransform buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.sizeDelta = new Vector2(220f, 56f);
            buttonRect.anchoredPosition = new Vector2(0f, 40f);
            Button button = buttonGo.GetComponent<Button>();

            Text buttonLabel = CreateText("Text", buttonGo.transform, "Lift Patient");
            buttonLabel.color = Color.black; // the button's Image background defaults to white —
                                              // white label text on it would be invisible
            RectTransform labelRect = buttonLabel.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            RescueUIController controller = canvasGo.AddComponent<RescueUIController>();
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("stepPromptText").objectReferenceValue = stepText;
            so.FindProperty("resultText").objectReferenceValue = resultText;
            so.FindProperty("liftButton").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Stretches a RectTransform's width to the full canvas width and pins it to
        /// the top edge, offset down by -yOffset pixels. Resolution-independent.</summary>
        private static void AnchorToTop(RectTransform rect, float yOffset)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 60f);
            rect.anchoredPosition = new Vector2(0f, yOffset);
        }

        private static Text CreateText(string name, Transform parent, string content)
        {
            GameObject go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);

            Text text = go.GetComponent<Text>();
            text.text = content;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            return text;
        }
    }
}
