using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Cinemachine;
using Game.Gameplay.Vehicles.Common;
using Game.Gameplay.Vehicles.Motorcycle;
using Game.Gameplay.Vehicles.Car;
using Game.Gameplay.Vehicles.Airplane;
using Game.Gameplay.Interactables;

namespace Game.Editor
{
    public static class VehicleSetupWizard
    {
        // ════════════════════════════════════════════════════════════════════════
        // MOTORCYCLE   Game / Vehicle Setup / Motorcycle
        // ════════════════════════════════════════════════════════════════════════

        [MenuItem("Game/Vehicle Setup/Motorcycle")]
        public static void SetupMotorcycle()
        {
            var go = FindOrCreateVehicleGO<MotorcycleController>("Motorcycle");

            EnsureComponent<MotorcycleInputAdapter>(go);
            EnsureComponent<MotorcycleCameraProvider>(go);
            EnsureComponent<MotorcycleHUDProvider>(go);
            EnsureComponent<MotorcycleController>(go);   // RequireComponent adds Rigidbody

            var rb = go.GetComponent<Rigidbody>();
            if (rb) { rb.mass = 180f; rb.linearDamping = 0.05f; rb.angularDamping = 0.5f; }

            if (!go.GetComponent<BoxCollider>())
            {
                var col   = go.AddComponent<BoxCollider>();
                col.size   = new Vector3(0.7f, 1.0f, 2.0f);
                col.center = new Vector3(0f,   0.5f, 0f);
            }

            // Wheel colliders
            var frontWC = CreateWheelCollider(go.transform, "FrontWheelCollider",
                              new Vector3(0f, 0f,  1.0f), radius: 0.35f, suspDist: 0.15f);
            var rearWC  = CreateWheelCollider(go.transform, "RearWheelCollider",
                              new Vector3(0f, 0f, -0.7f), radius: 0.35f, suspDist: 0.15f);

            // Wheel mesh placeholders
            var frontWM = CreateChild(go.transform, "FrontWheelMesh", new Vector3(0f, 0f,  1.0f));
            var rearWM  = CreateChild(go.transform, "RearWheelMesh",  new Vector3(0f, 0f, -0.7f));

            // Anchors
            var enterAnchor = CreateChild(go.transform, "EnterAnchor", new Vector3(0f,   0.5f, 0f));
            var exitAnchor  = CreateChild(go.transform, "ExitAnchor",  new Vector3(1.2f, 0f,   0f));

            // VCam
            var vcam = FindOrCreateVehicleVCam("Motorcycle_Vcam", go.transform,
                           new Vector3(0f, 2f, -6f));

            // HUD prefabs
            const string hudFolder = "Assets/_Project/Gameplay/Vehicles/Motorcycle/HUD/Prefabs";
            EnsureFolderExists(hudFolder);
            var speedoPrefab = CreateTMProPrefab<SpeedoModule>(hudFolder, "MotorSpeedo");
            var rpmPrefab    = CreateTMProPrefab<RPMModule>(hudFolder,    "MotorRPM");

            // Wire controller (inherits VehicleControllerBase fields via SerializedObject)
            var ctrl = go.GetComponent<MotorcycleController>();
            SetField(ctrl, "_frontWheelCollider", frontWC.GetComponent<WheelCollider>());
            SetField(ctrl, "_rearWheelCollider",  rearWC.GetComponent<WheelCollider>());
            SetField(ctrl, "_frontWheelMesh",     frontWM.transform);
            SetField(ctrl, "_rearWheelMesh",      rearWM.transform);
            SetField(ctrl, "_enterAnchor",        enterAnchor.transform);
            SetField(ctrl, "_exitAnchor",         exitAnchor.transform);

            SetField(go.GetComponent<MotorcycleCameraProvider>(), "_vcamGameObject", vcam);
            SetField(go.GetComponent<MotorcycleHUDProvider>(),    "_speedoPrefab",   speedoPrefab);
            SetField(go.GetComponent<MotorcycleHUDProvider>(),    "_rpmPrefab",      rpmPrefab);

            var interactable = EnsureComponent<EnterVehicleInteractable>(go);
            SetField(interactable, "_vehicle", ctrl);

            Finish("Motorcycle",
                "Assign real wheel meshes to FrontWheelMesh / RearWheelMesh. " +
                "Adjust WheelCollider radius/position to match your model. " +
                "Optionally assign _handlerBar. " +
                "Place vehicle on Interactable layer.");
        }

        // ════════════════════════════════════════════════════════════════════════
        // CAR          Game / Vehicle Setup / Car
        // ════════════════════════════════════════════════════════════════════════

        [MenuItem("Game/Vehicle Setup/Car")]
        public static void SetupCar()
        {
            var go = FindOrCreateVehicleGO<CarController>("Car");

            EnsureComponent<CarInputAdapter>(go);
            EnsureComponent<CarCameraProvider>(go);
            EnsureComponent<CarHUDProvider>(go);
            EnsureComponent<CarController>(go);

            var rb = go.GetComponent<Rigidbody>();
            if (rb) { rb.mass = 1200f; rb.linearDamping = 0.02f; rb.angularDamping = 0.3f; }

            if (!go.GetComponent<BoxCollider>())
            {
                var col   = go.AddComponent<BoxCollider>();
                col.size   = new Vector3(1.8f, 1.4f, 4.4f);
                col.center = new Vector3(0f,   0.7f, 0f);
            }

            // 4 WheelColliders
            var wFL = CreateWheelCollider(go.transform, "WheelFL",
                          new Vector3(-0.8f, -0.2f,  1.3f), 0.35f, 0.2f);
            var wFR = CreateWheelCollider(go.transform, "WheelFR",
                          new Vector3( 0.8f, -0.2f,  1.3f), 0.35f, 0.2f);
            var wRL = CreateWheelCollider(go.transform, "WheelRL",
                          new Vector3(-0.8f, -0.2f, -1.3f), 0.35f, 0.2f);
            var wRR = CreateWheelCollider(go.transform, "WheelRR",
                          new Vector3( 0.8f, -0.2f, -1.3f), 0.35f, 0.2f);

            // 4 wheel mesh placeholders
            var mFL = CreateChild(go.transform, "MeshFL", new Vector3(-0.8f, -0.2f,  1.3f));
            var mFR = CreateChild(go.transform, "MeshFR", new Vector3( 0.8f, -0.2f,  1.3f));
            var mRL = CreateChild(go.transform, "MeshRL", new Vector3(-0.8f, -0.2f, -1.3f));
            var mRR = CreateChild(go.transform, "MeshRR", new Vector3( 0.8f, -0.2f, -1.3f));

            var enterAnchor = CreateChild(go.transform, "EnterAnchor", new Vector3( 0f,   0.5f, 0f));
            var exitAnchor  = CreateChild(go.transform, "ExitAnchor",  new Vector3( 1.8f, 0f,   0f));

            var vcam = FindOrCreateVehicleVCam("Car_Vcam", go.transform,
                           new Vector3(0f, 3f, -8f));

            const string hudFolder = "Assets/_Project/Gameplay/Vehicles/Car/HUD/Prefabs";
            EnsureFolderExists(hudFolder);
            var speedoPrefab = CreateTMProPrefab<CarSpeedoModule>(hudFolder, "CarSpeedo");
            var gearPrefab   = CreateTMProPrefab<GearModule>(hudFolder,      "CarGear");

            var ctrl = go.GetComponent<CarController>();
            SetField(ctrl, "_wheelFL", wFL.GetComponent<WheelCollider>());
            SetField(ctrl, "_wheelFR", wFR.GetComponent<WheelCollider>());
            SetField(ctrl, "_wheelRL", wRL.GetComponent<WheelCollider>());
            SetField(ctrl, "_wheelRR", wRR.GetComponent<WheelCollider>());
            SetField(ctrl, "_meshFL",  mFL.transform);
            SetField(ctrl, "_meshFR",  mFR.transform);
            SetField(ctrl, "_meshRL",  mRL.transform);
            SetField(ctrl, "_meshRR",  mRR.transform);
            SetField(ctrl, "_enterAnchor", enterAnchor.transform);
            SetField(ctrl, "_exitAnchor",  exitAnchor.transform);

            SetField(go.GetComponent<CarCameraProvider>(), "_vcamGameObject", vcam);
            SetField(go.GetComponent<CarHUDProvider>(),    "_speedoPrefab",   speedoPrefab);
            SetField(go.GetComponent<CarHUDProvider>(),    "_gearPrefab",     gearPrefab);

            var interactable = EnsureComponent<EnterVehicleInteractable>(go);
            SetField(interactable, "_vehicle", ctrl);

            Finish("Car",
                "Assign real wheel meshes to MeshFL/FR/RL/RR. " +
                "Adjust WheelCollider positions to match your car model. " +
                "Place vehicle on Interactable layer.");
        }

        // ════════════════════════════════════════════════════════════════════════
        // AIRPLANE     Game / Vehicle Setup / Airplane
        // ════════════════════════════════════════════════════════════════════════

        [MenuItem("Game/Vehicle Setup/Airplane")]
        public static void SetupAirplane()
        {
            var go = FindOrCreateVehicleGO<AirplaneController>("Airplane");
            go.transform.position = new Vector3(10f, 0.5f, 10f);  // elevated spawn

            EnsureComponent<AirplaneInputAdapter>(go);
            EnsureComponent<AirplaneCameraProvider>(go);
            EnsureComponent<AirplaneHUDProvider>(go);
            EnsureComponent<AirplaneController>(go);

            var rb = go.GetComponent<Rigidbody>();
            if (rb) { rb.mass = 800f; rb.linearDamping = 0.05f; rb.angularDamping = 1.0f; }

            if (!go.GetComponent<BoxCollider>())
            {
                var col   = go.AddComponent<BoxCollider>();
                col.size   = new Vector3(9f, 2f, 8f);
                col.center = Vector3.zero;
            }

            // Landing gear — 3-point: nose + 2 main
            var gearNose  = CreateWheelCollider(go.transform, "GearNose",
                                new Vector3( 0f, -0.5f,  2.5f), 0.30f, 0.3f);
            var gearLeft  = CreateWheelCollider(go.transform, "GearLeft",
                                new Vector3(-2f, -0.5f, -1.0f), 0.35f, 0.3f);
            var gearRight = CreateWheelCollider(go.transform, "GearRight",
                                new Vector3( 2f, -0.5f, -1.0f), 0.35f, 0.3f);

            var meshNose  = CreateChild(go.transform, "GearNoseMesh",  new Vector3( 0f, -0.5f,  2.5f));
            var meshLeft  = CreateChild(go.transform, "GearLeftMesh",  new Vector3(-2f, -0.5f, -1.0f));
            var meshRight = CreateChild(go.transform, "GearRightMesh", new Vector3( 2f, -0.5f, -1.0f));

            var enterAnchor = CreateChild(go.transform, "EnterAnchor", new Vector3(0f,   0.5f, 0f));
            var exitAnchor  = CreateChild(go.transform, "ExitAnchor",  new Vector3(2.5f, 0f,   0f));

            var vcam = FindOrCreateVehicleVCam("Airplane_Vcam", go.transform,
                           new Vector3(0f, 5f, -18f));

            const string hudFolder = "Assets/_Project/Gameplay/Vehicles/Airplane/HUD/Prefabs";
            EnsureFolderExists(hudFolder);
            var speedoPrefab   = CreateTMProPrefab<AirplaneSpeedoModule>(hudFolder, "AirSpeedo");
            var altitudePrefab = CreateTMProPrefab<AltitudeModule>(hudFolder,       "AirAltitude");
            var headingPrefab  = CreateTMProPrefab<HeadingModule>(hudFolder,        "AirHeading");

            var ctrl = go.GetComponent<AirplaneController>();
            SetField(ctrl, "_enterAnchor", enterAnchor.transform);
            SetField(ctrl, "_exitAnchor",  exitAnchor.transform);

            SetFieldArray(ctrl, "_landingGearWheels", new Object[]
            {
                gearNose.GetComponent<WheelCollider>(),
                gearLeft.GetComponent<WheelCollider>(),
                gearRight.GetComponent<WheelCollider>(),
            });
            SetFieldArray(ctrl, "_landingGearMeshes", new Object[]
            {
                meshNose.transform,
                meshLeft.transform,
                meshRight.transform,
            });

            SetField(go.GetComponent<AirplaneCameraProvider>(), "_vcamGameObject",  vcam);
            SetField(go.GetComponent<AirplaneHUDProvider>(),    "_speedoPrefab",    speedoPrefab);
            SetField(go.GetComponent<AirplaneHUDProvider>(),    "_altitudePrefab",  altitudePrefab);
            SetField(go.GetComponent<AirplaneHUDProvider>(),    "_headingPrefab",   headingPrefab);

            var interactable = EnsureComponent<EnterVehicleInteractable>(go);
            SetField(interactable, "_vehicle", ctrl);

            Finish("Airplane",
                "Assign real fuselage/wing mesh. Adjust landing gear WheelCollider positions. " +
                "Optionally assign _propeller. Tune AirplaneConfig (LiftCoefficient, StallSpeed) in Inspector. " +
                "Place vehicle on Interactable layer.");
        }

        // ════════════════════════════════════════════════════════════════════════
        // SHARED HELPERS
        // ════════════════════════════════════════════════════════════════════════

        static GameObject FindOrCreateVehicleGO<T>(string goName) where T : MonoBehaviour
        {
            var existing = Object.FindFirstObjectByType<T>();
            if (existing != null)
            {
                Debug.Log($"[VehicleSetup] Found existing {goName} GO.");
                return existing.gameObject;
            }
            var go = new GameObject(goName);
            go.transform.position = new Vector3(5f, 0f, 5f);
            Debug.Log($"[VehicleSetup] Created {goName} GO.");
            return go;
        }

        static T EnsureComponent<T>(GameObject go) where T : Component
            => go.GetComponent<T>() ?? go.AddComponent<T>();

        static GameObject CreateWheelCollider(Transform parent, string name,
                                              Vector3 localPos, float radius, float suspDist)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            var wc = go.AddComponent<WheelCollider>();
            wc.radius             = radius;
            wc.suspensionDistance = suspDist;
            wc.mass               = 20f;

            var spring = wc.suspensionSpring;
            spring.spring         = 35000f;
            spring.damper         = 4500f;
            spring.targetPosition = 0.5f;
            wc.suspensionSpring   = spring;

            return go;
        }

        static GameObject CreateChild(Transform parent, string name, Vector3 localPos)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go;
        }

        static GameObject FindOrCreateVehicleVCam(string vcamName, Transform trackTarget,
                                                   Vector3 localOffset)
        {
            var existing = GameObject.Find(vcamName);
            if (existing != null) return existing;

            var go   = new GameObject(vcamName);
            var vcam = go.AddComponent<CinemachineCamera>();
            go.AddComponent<CinemachineOrbitalFollow>();
            go.AddComponent<CinemachineRotationComposer>();

            var ct = vcam.Target;
            ct.TrackingTarget = trackTarget;
            vcam.Target       = ct;

            go.transform.position = trackTarget.position + localOffset;
            Debug.Log($"[VehicleSetup] Created {vcamName}.");
            return go;
        }

        static GameObject CreateTMProPrefab<T>(string folder, string prefabName) where T : MonoBehaviour
        {
            var path     = $"{folder}/{prefabName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(prefabName);
            go.AddComponent<RectTransform>();
            go.AddComponent<TextMeshProUGUI>();
            go.AddComponent<T>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[VehicleSetup] Created prefab: {path}");
            return prefab;
        }

        static void SetField(Object target, string fieldName, Object value)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[VehicleSetup] Field not found: {target.GetType().Name}.{fieldName}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        static void SetFieldArray(Object target, string fieldName, Object[] values)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[VehicleSetup] Array field not found: {target.GetType().Name}.{fieldName}");
                return;
            }
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedProperties();
        }

        static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            var parts   = folderPath.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static void Finish(string vehicleName, string manualSteps)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[VehicleSetup] {vehicleName} done. Manual steps: {manualSteps} Save scene (Ctrl+S).");
        }
    }
}
