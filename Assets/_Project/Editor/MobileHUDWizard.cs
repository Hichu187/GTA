using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.InputSystem.OnScreen;
using Game.Gameplay.Character;
using Game.Gameplay.Character.HUD;
using Game.Gameplay.Vehicles.Motorcycle;
using Game.Gameplay.Vehicles.Car;
using Game.Gameplay.Vehicles.Airplane;
using Game.Gameplay.Vehicles.Helicopter;
using Game.Gameplay.Vehicles.Glider;
using Game.Gameplay.Vehicles.Rocket;
using Game.Gameplay.Vehicles.Tank;

namespace Game.Editor
{
    /// <summary>
    /// Menu: Game / Mobile HUD — Create All Prefabs
    /// Character: joystick move + action buttons.
    /// Vehicles: button-based — Left=Forward/Back, Right=Direction cross, Others=top-left (user repositions).
    /// </summary>
    public static class MobileHUDWizard
    {
        private const string PrefabRoot = "Assets/_Project/Gameplay/Character/HUD/Prefabs";
        private const int BehaviourDynamic = 1;  // ExactPositionWithDynamicOrigin (floating joystick)

        [MenuItem("Game/Mobile HUD/Create All")]
        public static void RunAll()
        {
            EnsureFolderExists(PrefabRoot);
            BuildCharacter();
            BuildMotorcycle();
            BuildCar();
            BuildAirplane();
            BuildHelicopter();
            BuildGlider();
            BuildRocket();
            BuildTank();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MobileHUDWizard] All prefabs created.");
        }

        // ═══════════════════════════════════════════════════════════════════
        // CHARACTER — keeps joystick + action buttons
        // ═══════════════════════════════════════════════════════════════════

        [MenuItem("Game/Mobile HUD/Character")]
        static void BuildCharacter()
        {
            var prefab = CreatePrefab("MobileControlsHUD_Character", root =>
            {
                AddLookPad(root);
                AddMoveStick(root, "<Gamepad>/leftStick");
                AddButtons(root, new[]
                {
                    Btn("Jump",   "<Gamepad>/buttonSouth",     -90f,  90f),
                    Btn("Sprint", "<Gamepad>/leftStickButton", -210f, 90f),
                    Btn("Crouch", "<Gamepad>/buttonEast",      -90f,  210f),
                    Btn("Enter",  "<Gamepad>/buttonWest",      -210f, 210f),
                    Btn("Fire",   "<Gamepad>/rightTrigger",    -90f,  330f),
                    Btn("Aim",    "<Gamepad>/leftTrigger",     -210f, 330f),
                    Btn("Reload", "<Gamepad>/rightShoulder",   -330f, 90f),
                    Btn("Throw",  "<Gamepad>/buttonNorth",     -330f, 210f),
                });
            });
            WireTo<CharacterHUDProvider>(prefab, "_mobileControlsPrefab");
        }

        // ═══════════════════════════════════════════════════════════════════
        // MOTORCYCLE — Gas/Brake left, Steer L/R right
        // ═══════════════════════════════════════════════════════════════════

        [MenuItem("Game/Mobile HUD/Motorcycle")]
        static void BuildMotorcycle()
        {
            var prefab = CreatePrefab("MobileControlsHUD_Motorcycle", root =>
            {
                AddLookPad(root);
                AddForwardBackButtons(root, "<Keyboard>/w", "<Keyboard>/s", "GAS", "BRAKE");
                AddSteer2Buttons(root, "<Keyboard>/a", "<Keyboard>/d");
                AddOtherButtons(root, new[] { ("Exit", "<Keyboard>/f") });
            });
            WireTo<MotorcycleHUDProvider>(prefab, "_mobileControlsPrefab");
        }

        // ═══════════════════════════════════════════════════════════════════
        // CAR — Gas/Brake left, Steer L/R right
        // ═══════════════════════════════════════════════════════════════════

        [MenuItem("Game/Mobile HUD/Car")]
        static void BuildCar()
        {
            var prefab = CreatePrefab("MobileControlsHUD_Car", root =>
            {
                AddLookPad(root);
                AddForwardBackButtons(root, "<Keyboard>/w", "<Keyboard>/s", "GAS", "BRAKE");
                AddSteer2Buttons(root, "<Keyboard>/a", "<Keyboard>/d");
                AddOtherButtons(root, new[] { ("Horn", "<Keyboard>/h"), ("Exit", "<Keyboard>/f") });
            });
            WireTo<CarHUDProvider>(prefab, "_mobileControlsPrefab");
        }

        // ═══════════════════════════════════════════════════════════════════
        // AIRPLANE — Throttle/Brake left, Pitch+Roll cross right
        // upArrow=negative Pitch composite (nose up), downArrow=positive (nose down)
        // ═══════════════════════════════════════════════════════════════════

        [MenuItem("Game/Mobile HUD/Airplane")]
        static void BuildAirplane()
        {
            var prefab = CreatePrefab("MobileControlsHUD_Airplane", root =>
            {
                AddLookPad(root);
                AddForwardBackButtons(root, "<Keyboard>/w", "<Keyboard>/space", "THROTTLE", "BRAKE");
                AddDirection4Buttons(root,
                    "<Keyboard>/upArrow", "<Keyboard>/downArrow",
                    "<Keyboard>/leftArrow", "<Keyboard>/rightArrow",
                    "UP", "DN", "RL", "RR");
                AddOtherButtons(root, new[]
                {
                    ("YawL", "<Keyboard>/q"),
                    ("YawR", "<Keyboard>/e"),
                    ("Exit", "<Keyboard>/f"),
                });
            });
            WireTo<AirplaneHUDProvider>(prefab, "_mobileControlsPrefab");
        }

        // ═══════════════════════════════════════════════════════════════════
        // HELICOPTER — Up/Down left (Q=up E=down), WASD cross right (Horizontal)
        // ═══════════════════════════════════════════════════════════════════

        [MenuItem("Game/Mobile HUD/Helicopter")]
        static void BuildHelicopter()
        {
            var prefab = CreatePrefab("MobileControlsHUD_Helicopter", root =>
            {
                AddLookPad(root);
                // Q=positive Vertical (up), E=negative Vertical (down)
                AddForwardBackButtons(root, "<Keyboard>/q", "<Keyboard>/e", "UP", "DOWN");
                // WASD = Horizontal 2DVector composite
                AddDirection4Buttons(root,
                    "<Keyboard>/w", "<Keyboard>/s",
                    "<Keyboard>/a", "<Keyboard>/d",
                    "FWD", "BWD", "LFT", "RGT");
                AddOtherButtons(root, new[]
                {
                    ("YawL",    "<Keyboard>/leftArrow"),
                    ("YawR",    "<Keyboard>/rightArrow"),
                    ("TakeOff", "<Keyboard>/space"),
                    ("Exit",    "<Keyboard>/f"),
                });
            });
            WireTo<HelicopterHUDProvider>(prefab, "_mobileControlsPrefab");
        }

        // ═══════════════════════════════════════════════════════════════════
        // GLIDER — Brake only on left, Pitch+Roll cross right
        // ═══════════════════════════════════════════════════════════════════

        [MenuItem("Game/Mobile HUD/Glider")]
        static void BuildGlider()
        {
            var prefab = CreatePrefab("MobileControlsHUD_Glider", root =>
            {
                AddLookPad(root);
                AddSingleLeftButton(root, "<Keyboard>/leftShift", "BRAKE");
                AddDirection4Buttons(root,
                    "<Keyboard>/upArrow", "<Keyboard>/downArrow",
                    "<Keyboard>/leftArrow", "<Keyboard>/rightArrow",
                    "UP", "DN", "RL", "RR");
                AddOtherButtons(root, new[] { ("Exit", "<Keyboard>/f") });
            });
            WireTo<GliderHUDProvider>(prefab, "_mobileControlsPrefab");
        }

        // ═══════════════════════════════════════════════════════════════════
        // ROCKET — Throttle only on left, Pitch+Roll cross right
        // ═══════════════════════════════════════════════════════════════════

        [MenuItem("Game/Mobile HUD/Rocket")]
        static void BuildRocket()
        {
            var prefab = CreatePrefab("MobileControlsHUD_Rocket", root =>
            {
                AddLookPad(root);
                AddSingleLeftButton(root, "<Keyboard>/w", "THROTTLE");
                AddDirection4Buttons(root,
                    "<Keyboard>/upArrow", "<Keyboard>/downArrow",
                    "<Keyboard>/leftArrow", "<Keyboard>/rightArrow",
                    "UP", "DN", "RL", "RR");
                AddOtherButtons(root, new[] { ("Exit", "<Keyboard>/f") });
            });
            WireTo<RocketHUDProvider>(prefab, "_mobileControlsPrefab");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TANK — Gas/Brake left, Steer L/R right, Fire prominent bottom-right
        // ═══════════════════════════════════════════════════════════════════

        [MenuItem("Game/Mobile HUD/Tank")]
        static void BuildTank()
        {
            var prefab = CreatePrefab("MobileControlsHUD_Tank", root =>
            {
                AddLookPad(root);
                AddForwardBackButtons(root, "<Keyboard>/w", "<Keyboard>/s", "GAS", "BRAKE");
                AddSteer2Buttons(root, "<Keyboard>/a", "<Keyboard>/d");
                // Fire — large button above the steer pair, bottom-right quadrant
                MakeActionButton(root, "Btn_Fire", "<Keyboard>/space", "FIRE",
                    Vector2.right, new Vector2(0.5f, 0.5f),
                    new Vector2(-175f, 320f), new Vector2(175f, 130f));
                AddOtherButtons(root, new[] { ("Exit", "<Keyboard>/f") });
            });
            WireTo<TankHUDProvider>(prefab, "_mobileControlsPrefab");
        }

        // ═══════════════════════════════════════════════════════════════════
        // LAYOUT HELPERS
        // ═══════════════════════════════════════════════════════════════════

        // Transparent drag area covering right 65% of screen — LookDragHandler added at runtime
        static void AddLookPad(GameObject canvas)
        {
            var go            = new GameObject("LookPad");
            go.transform.SetParent(canvas.transform, false);
            var rt            = go.AddComponent<RectTransform>();
            rt.anchorMin      = new Vector2(0.35f, 0f);
            rt.anchorMax      = new Vector2(1f,    1f);
            rt.offsetMin      = Vector2.zero;
            rt.offsetMax      = Vector2.zero;
            var img           = go.AddComponent<Image>();
            img.color         = new Color(0f, 0f, 0f, 0f);
            img.raycastTarget = true;
        }

        // Character only: floating joystick at bottom-left
        static void AddMoveStick(GameObject canvas, string controlPath)
        {
            var bg   = MakeCircle(canvas, "MoveStick_BG", new Color(0.1f, 0.1f, 0.1f, 0.45f));
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin        = bgRt.anchorMax = Vector2.zero;
            bgRt.pivot            = new Vector2(0.5f, 0.5f);
            bgRt.anchoredPosition = new Vector2(160f, 160f);
            bgRt.sizeDelta        = new Vector2(200f, 200f);

            var knob = MakeCircle(bg, "MoveStick_Knob", new Color(0.85f, 0.85f, 0.85f, 0.65f));
            CenterInParent(knob.GetComponent<RectTransform>(), 80f);
            SetupOnScreenStick(knob, controlPath, 60f, BehaviourDynamic);
        }

        // Two stacked buttons on the left: forward (top) + backward (bottom)
        static void AddForwardBackButtons(GameObject canvas,
            string fwdPath, string bwdPath, string fwdLabel, string bwdLabel)
        {
            MakeActionButton(canvas, "Btn_" + fwdLabel, fwdPath, fwdLabel,
                Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(110f, 330f), new Vector2(150f, 130f));
            MakeActionButton(canvas, "Btn_" + bwdLabel, bwdPath, bwdLabel,
                Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(110f, 180f), new Vector2(150f, 130f));
        }

        // Single button on the left (glider/rocket — no back action)
        static void AddSingleLeftButton(GameObject canvas, string path, string label)
        {
            MakeActionButton(canvas, "Btn_" + label, path, label,
                Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(110f, 240f), new Vector2(150f, 150f));
        }

        // Two side-by-side steer buttons anchored bottom-right
        static void AddSteer2Buttons(GameObject canvas, string leftPath, string rightPath)
        {
            MakeActionButton(canvas, "Btn_SteerL", leftPath, "LSTR",
                Vector2.right, new Vector2(0.5f, 0.5f), new Vector2(-260f, 160f), new Vector2(130f, 130f));
            MakeActionButton(canvas, "Btn_SteerR", rightPath, "RSTR",
                Vector2.right, new Vector2(0.5f, 0.5f), new Vector2(-120f, 160f), new Vector2(130f, 130f));
        }

        // D-pad cross layout anchored bottom-right
        static void AddDirection4Buttons(GameObject canvas,
            string upPath, string downPath, string leftPath, string rightPath,
            string upLabel, string downLabel, string leftLabel, string rightLabel)
        {
            var anchor = Vector2.right;
            var pivot  = new Vector2(0.5f, 0.5f);
            const float sz = 115f;

            MakeActionButton(canvas, "Btn_DirUp",    upPath,    upLabel,    anchor, pivot, new Vector2(-175f, 315f), new Vector2(sz, sz));
            MakeActionButton(canvas, "Btn_DirDown",  downPath,  downLabel,  anchor, pivot, new Vector2(-175f, 130f), new Vector2(sz, sz));
            MakeActionButton(canvas, "Btn_DirLeft",  leftPath,  leftLabel,  anchor, pivot, new Vector2(-300f, 222f), new Vector2(sz, sz));
            MakeActionButton(canvas, "Btn_DirRight", rightPath, rightLabel, anchor, pivot, new Vector2( -50f, 222f), new Vector2(sz, sz));
        }

        // Misc extras in a row at top-left — user repositions later
        static void AddOtherButtons(GameObject canvas, (string label, string path)[] extras)
        {
            for (int i = 0; i < extras.Length; i++)
            {
                MakeActionButton(canvas, "Btn_" + extras[i].label, extras[i].path, extras[i].label,
                    new Vector2(0f, 1f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(70f + i * 120f, -70f),
                    new Vector2(100f, 100f));
            }
        }

        // Legacy fixed bottom-right buttons — Character only
        static void AddButtons(GameObject canvas, (string label, string path, float x, float y)[] defs)
        {
            foreach (var d in defs)
            {
                var go = MakeCircle(canvas, "Btn_" + d.label, new Color(0.15f, 0.15f, 0.15f, 0.55f));
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin        = rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot            = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(d.x, d.y);
                rt.sizeDelta        = new Vector2(110f, 110f);
                SetupOnScreenButton(go, d.path);
                AddLabel(go, d.label);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // PREFAB FACTORY
        // ═══════════════════════════════════════════════════════════════════

        delegate void CanvasBuilder(GameObject canvas);

        static GameObject CreatePrefab(string name, CanvasBuilder build)
        {
            var path = $"{PrefabRoot}/{name}.prefab";

            var root   = new GameObject(name);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;

            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<MobileControlsModule>();

            build(root);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            Debug.Log($"[MobileHUDWizard] Saved: {path}");
            return prefab;
        }

        static void WireTo<T>(GameObject prefab, string fieldName) where T : Component
        {
            var provider = Object.FindFirstObjectByType<T>();
            if (provider == null)
            {
                Debug.LogWarning($"[MobileHUDWizard] {typeof(T).Name} not in scene — assign '{fieldName}' manually.");
                return;
            }
            var so = new SerializedObject(provider);
            so.FindProperty(fieldName).objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            Debug.Log($"[MobileHUDWizard] Wired {prefab.name} -> {typeof(T).Name}.{fieldName}");
        }

        // ═══════════════════════════════════════════════════════════════════
        // ON-SCREEN COMPONENT SETUP
        // ═══════════════════════════════════════════════════════════════════

        static void SetupOnScreenStick(GameObject go, string controlPath, float range, int behaviour)
        {
            var stick = go.AddComponent<OnScreenStick>();
            var so    = new SerializedObject(stick);
            so.FindProperty("m_ControlPath").stringValue  = controlPath;
            so.FindProperty("m_MovementRange").floatValue = range;
            so.FindProperty("m_Behaviour").enumValueIndex = behaviour;
            so.ApplyModifiedProperties();
        }

        static void SetupOnScreenButton(GameObject go, string controlPath)
        {
            var btn = go.AddComponent<OnScreenButton>();
            var so  = new SerializedObject(btn);
            so.FindProperty("m_ControlPath").stringValue = controlPath;
            so.ApplyModifiedProperties();
        }

        // ═══════════════════════════════════════════════════════════════════
        // UI PRIMITIVES
        // ═══════════════════════════════════════════════════════════════════

        static GameObject MakeActionButton(GameObject canvas, string name, string path, string label,
            Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var go = MakeCircle(canvas, name, new Color(0.15f, 0.15f, 0.15f, 0.55f));
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = rt.anchorMax = anchor;
            rt.pivot            = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            SetupOnScreenButton(go, path);
            AddLabel(go, label);
            return go;
        }

        static GameObject MakeCircle(GameObject parent, string name, Color color)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            var img   = go.AddComponent<Image>();
            img.color  = color;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            return go;
        }

        static void CenterInParent(RectTransform rt, float size)
        {
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = new Vector2(size, size);
        }

        static void AddLabel(GameObject parent, string text)
        {
            var go  = new GameObject("Label");
            go.transform.SetParent(parent.transform, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var txt       = go.AddComponent<Text>();
            txt.text      = text;
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize  = 18;
            txt.fontStyle = FontStyle.Bold;
            txt.color     = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
        }

        static (string label, string path, float x, float y) Btn(string label, string path, float x, float y)
            => (label, path, x, y);

        // ═══════════════════════════════════════════════════════════════════
        // FOLDER UTIL
        // ═══════════════════════════════════════════════════════════════════

        static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            var parts = folderPath.Split('/');
            var cur   = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
