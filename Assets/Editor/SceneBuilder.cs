using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
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

            Text stepText = CreateText("StepPromptText", canvasGo.transform, new Vector2(0f, 220f),
                new Vector2(500f, 60f), "Check the victim's pulse.");
            Text resultText = CreateText("ResultText", canvasGo.transform, new Vector2(0f, 150f),
                new Vector2(500f, 60f), string.Empty);

            GameObject buttonGo = new GameObject("LiftButton", typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(canvasGo.transform, false);
            RectTransform buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(220f, 56f);
            buttonRect.anchoredPosition = new Vector2(0f, -220f);
            Button button = buttonGo.GetComponent<Button>();

            Text buttonLabel = CreateText("Text", buttonGo.transform, Vector2.zero, Vector2.zero, "Lift Patient");
            RectTransform labelRect = buttonLabel.GetComponent<RectTransform>();
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

        private static Text CreateText(string name, Transform parent, Vector2 anchoredPos, Vector2 size, string content)
        {
            GameObject go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

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
