using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Cinemachine;
using Game.Gameplay.Character;
using Game.Gameplay.Character.HUD;
using Game.Services;

namespace Game.Editor
{
    /// <summary>
    /// Menu: Game / Phase 3 — Setup Scene
    /// Tự động hoá toàn bộ scene setup Phase 3:
    ///   1. Tạo Character GO với đầy đủ components.
    ///   2. Tạo TP_Vcam (CinemachineOrbitalFollow) và FP_Vcam (CinemachinePanTilt, child của Character).
    ///   3. Wire CharacterCameraProvider với 2 vcam vừa tạo.
    ///   4. Tạo 3 HUD prefab assets (HealthBar, StaminaBar, Crosshair).
    ///   5. Wire CharacterHUDProvider với 3 prefab.
    ///   6. Wire GameBootstrapper._initialPossessable → Character.
    ///   7. Wire PossessionTester._character → Character.
    /// </summary>
    public static class Phase3SetupWizard
    {
        private const string PrefabFolder = "Assets/_Project/Gameplay/Character/HUD/Prefabs";

        [MenuItem("Game/Phase 3 — Setup Scene")]
        public static void Run()
        {
            var character    = FindOrCreateCharacter();
            var (tpVcam, fpVcam) = FindOrCreateVCams(character);

            WireCameraProvider(character, tpVcam, fpVcam);

            EnsureFolderExists(PrefabFolder);
            var healthBar  = CreateBarPrefab<HealthBarModule>("HealthBar");
            var staminaBar = CreateBarPrefab<StaminaBarModule>("StaminaBar");
            var crosshair  = CreateCrosshairPrefab();

            WireHUDProvider(character, healthBar, staminaBar, crosshair);
            WireBootstrapper(character);
            WirePossessionTester(character);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Phase3Setup] Done. Save the scene (Ctrl+S).");
        }

        // ── 1. Character GO ──────────────────────────────────────────────────

        static GameObject FindOrCreateCharacter()
        {
            var existing = Object.FindFirstObjectByType<Character>();
            if (existing != null)
            {
                Debug.Log("[Phase3Setup] Found existing Character GO.");
                return existing.gameObject;
            }

            var go = new GameObject("Character");
            go.AddComponent<CharacterController>();
            go.AddComponent<CharacterInputAdapter>();
            go.AddComponent<CharacterCameraProvider>();
            go.AddComponent<CharacterHUDProvider>();
            go.AddComponent<Character>();          // last: depends on the 4 above
            go.transform.position = new Vector3(0, 1f, 0);

            Debug.Log("[Phase3Setup] Created Character GO.");
            return go;
        }

        // ── 2. VCams ─────────────────────────────────────────────────────────

        static (GameObject tp, GameObject fp) FindOrCreateVCams(GameObject character)
        {
            var tp = FindOrCreateTPVcam(character);
            var fp = FindOrCreateFPVcam(character);
            return (tp, fp);
        }

        static GameObject FindOrCreateTPVcam(GameObject character)
        {
            var existing = GameObject.Find("TP_Vcam");
            if (existing != null) return existing;

            var go   = new GameObject("TP_Vcam");
            var vcam = go.AddComponent<CinemachineCamera>();
            go.AddComponent<CinemachineOrbitalFollow>();
            go.AddComponent<CinemachineRotationComposer>();

            // Point at character — CameraTarget is a Cinemachine 3.x struct.
            var ct = vcam.Target;
            ct.TrackingTarget = character.transform;
            vcam.Target = ct;

            go.transform.position = character.transform.position + new Vector3(0f, 2f, -5f);

            Debug.Log("[Phase3Setup] Created TP_Vcam.");
            return go;
        }

        static GameObject FindOrCreateFPVcam(GameObject character)
        {
            var existingTf = character.transform.Find("FP_Vcam");
            if (existingTf != null) return existingTf.gameObject;

            var go = new GameObject("FP_Vcam");
            go.transform.SetParent(character.transform, false);
            go.transform.localPosition = new Vector3(0f, 1.7f, 0.1f);
            go.transform.localRotation = Quaternion.identity;

            var vcam = go.AddComponent<CinemachineCamera>();
            go.AddComponent<CinemachinePanTilt>();

            var ct = vcam.Target;
            ct.TrackingTarget = character.transform;
            vcam.Target = ct;

            Debug.Log("[Phase3Setup] Created FP_Vcam (child of Character).");
            return go;
        }

        // ── 3. Wire CameraProvider ───────────────────────────────────────────

        static void WireCameraProvider(GameObject character, GameObject tpVcam, GameObject fpVcam)
        {
            var provider = character.GetComponent<CharacterCameraProvider>();
            if (provider == null) { Debug.LogWarning("[Phase3Setup] CharacterCameraProvider missing."); return; }

            var so = new SerializedObject(provider);
            so.FindProperty("_thirdPersonVcam").objectReferenceValue = tpVcam;
            so.FindProperty("_firstPersonVcam").objectReferenceValue = fpVcam;
            so.ApplyModifiedProperties();

            Debug.Log("[Phase3Setup] Wired CharacterCameraProvider.");
        }

        // ── 4. HUD Prefabs ───────────────────────────────────────────────────

        static GameObject CreateBarPrefab<T>(string prefabName) where T : MonoBehaviour
        {
            var path     = $"{PrefabFolder}/{prefabName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(prefabName);
            go.AddComponent<RectTransform>();
            go.AddComponent<Slider>();   // [RequireComponent] on T satisfied explicitly
            go.AddComponent<T>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            Debug.Log($"[Phase3Setup] Created prefab: {path}");
            return prefab;
        }

        static GameObject CreateCrosshairPrefab()
        {
            var path     = $"{PrefabFolder}/Crosshair.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("Crosshair");
            go.AddComponent<RectTransform>();
            go.AddComponent<Image>();
            go.AddComponent<CrosshairModule>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            Debug.Log("[Phase3Setup] Created Crosshair prefab.");
            return prefab;
        }

        // ── 5. Wire HUDProvider ──────────────────────────────────────────────

        static void WireHUDProvider(GameObject character,
                                    GameObject healthBar, GameObject staminaBar, GameObject crosshair)
        {
            var provider = character.GetComponent<CharacterHUDProvider>();
            if (provider == null) { Debug.LogWarning("[Phase3Setup] CharacterHUDProvider missing."); return; }

            var so = new SerializedObject(provider);
            so.FindProperty("_healthBarPrefab").objectReferenceValue  = healthBar;
            so.FindProperty("_staminaBarPrefab").objectReferenceValue = staminaBar;
            so.FindProperty("_crosshairPrefab").objectReferenceValue  = crosshair;
            so.ApplyModifiedProperties();

            Debug.Log("[Phase3Setup] Wired CharacterHUDProvider.");
        }

        // ── 6. GameBootstrapper ──────────────────────────────────────────────

        static void WireBootstrapper(GameObject character)
        {
            var bootstrapper = Object.FindFirstObjectByType<GameBootstrapper>();
            if (bootstrapper == null) { Debug.LogWarning("[Phase3Setup] GameBootstrapper not in scene."); return; }

            var so = new SerializedObject(bootstrapper);
            so.FindProperty("_initialPossessable").objectReferenceValue = character.GetComponent<Character>();
            so.ApplyModifiedProperties();

            Debug.Log("[Phase3Setup] Wired GameBootstrapper._initialPossessable.");
        }

        // ── 7. PossessionTester ──────────────────────────────────────────────

        static void WirePossessionTester(GameObject character)
        {
            var tester = Object.FindFirstObjectByType<PossessionTester>();
            if (tester == null) { Debug.LogWarning("[Phase3Setup] PossessionTester not in scene — skipped."); return; }

            var so = new SerializedObject(tester);
            so.FindProperty("_character").objectReferenceValue = character;
            so.ApplyModifiedProperties();

            Debug.Log("[Phase3Setup] Wired PossessionTester._character.");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            var parts  = folderPath.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
