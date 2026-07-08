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
using Game.Gameplay.Vehicles.Helicopter;
using Game.Gameplay.Vehicles.Glider;
using Game.Gameplay.Vehicles.Rocket;
using Game.Gameplay.Vehicles.Tank;
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
                                new Vector3( 0f, -0.5f,  2.5f), 0.30f, 0.3f, fwdStiffness: 1f);
            var gearLeft  = CreateWheelCollider(go.transform, "GearLeft",
                                new Vector3(-2f, -0.5f, -1.0f), 0.35f, 0.3f, fwdStiffness: 1f);
            var gearRight = CreateWheelCollider(go.transform, "GearRight",
                                new Vector3( 2f, -0.5f, -1.0f), 0.35f, 0.3f, fwdStiffness: 1f);

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
        // HELICOPTER   Game / Vehicle Setup / Helicopter
        // ════════════════════════════════════════════════════════════════════════

        [MenuItem("Game/Vehicle Setup/Helicopter")]
        public static void SetupHelicopter()
        {
            var go = FindOrCreateVehicleGO<HelicopterController>("Helicopter");
            go.transform.position = new Vector3(10f, 0.5f, 10f);

            EnsureComponent<HelicopterInputAdapter>(go);
            EnsureComponent<HelicopterCameraProvider>(go);
            EnsureComponent<HelicopterHUDProvider>(go);
            EnsureComponent<HelicopterController>(go);

            var rb = go.GetComponent<Rigidbody>();
            if (rb) { rb.mass = 1500f; rb.linearDamping = 0.05f; rb.angularDamping = 1.0f; rb.useGravity = false; }

            if (!go.GetComponent<BoxCollider>())
            {
                var col    = go.AddComponent<BoxCollider>();
                col.size   = new Vector3(2.5f, 1.5f, 6f);
                col.center = new Vector3(0f, 0.5f, 0f);
            }

            // Visual hierarchy: RollRoot → MeshRoot (for tilt)
            var rollRoot = CreateChild(go.transform, "RollRoot", Vector3.zero);
            var meshRoot = CreateChild(rollRoot.transform, "MeshRoot", Vector3.zero);

            // Rotor placeholders
            var mainRotor = CreateChild(go.transform, "MainRotor", new Vector3(0f,  2f,  0f));
            var tailRotor = CreateChild(go.transform, "TailRotor", new Vector3(0f,  1f, -3f));

            var enterAnchor = CreateChild(go.transform, "EnterAnchor", new Vector3( 0f, 0.5f, 0f));
            var exitAnchor  = CreateChild(go.transform, "ExitAnchor",  new Vector3( 2f, 0f,   0f));

            var vcam = FindOrCreateVehicleVCam("Helicopter_Vcam", go.transform,
                           new Vector3(0f, 5f, -15f));

            const string hudFolder = "Assets/_Project/Gameplay/Vehicles/Helicopter/HUD/Prefabs";
            EnsureFolderExists(hudFolder);
            var speedoPrefab   = CreateTMProPrefab<HelicopterSpeedoModule>(hudFolder,    "HeliSpeedo");
            var altitudePrefab = CreateTMProPrefab<HelicopterAltitudeModule>(hudFolder,  "HeliAltitude");
            var vertSpeedPrefab = CreateTMProPrefab<HelicopterVertSpeedModule>(hudFolder, "HeliVertSpeed");

            var ctrl = go.GetComponent<HelicopterController>();
            SetField(ctrl, "_enterAnchor", enterAnchor.transform);
            SetField(ctrl, "_exitAnchor",  exitAnchor.transform);
            SetField(ctrl, "_meshRoot",    meshRoot.transform);
            SetField(ctrl, "_rollRoot",    rollRoot.transform);
            SetField(ctrl, "_mainRotor",   mainRotor.transform);
            SetField(ctrl, "_tailRotor",   tailRotor.transform);

            SetField(go.GetComponent<HelicopterCameraProvider>(), "_vcamGameObject",     vcam);
            SetField(go.GetComponent<HelicopterHUDProvider>(),    "_speedoPrefab",       speedoPrefab);
            SetField(go.GetComponent<HelicopterHUDProvider>(),    "_altitudePrefab",     altitudePrefab);
            SetField(go.GetComponent<HelicopterHUDProvider>(),    "_verticalSpeedPrefab", vertSpeedPrefab);

            var interactable = EnsureComponent<EnterVehicleInteractable>(go);
            SetField(interactable, "_vehicle", ctrl);

            Finish("Helicopter",
                "Replace RollRoot/MeshRoot with your real fuselage mesh. " +
                "Assign real rotor meshes to MainRotor / TailRotor. " +
                "Press Space in-game to TakeOff; press Space again near ground to Land. " +
                "Place vehicle on Interactable layer.");
        }

        // ════════════════════════════════════════════════════════════════════════
        // GLIDER       Game / Vehicle Setup / Glider
        // ════════════════════════════════════════════════════════════════════════

        [MenuItem("Game/Vehicle Setup/Glider")]
        public static void SetupGlider()
        {
            var go = FindOrCreateVehicleGO<GliderController>("Glider");
            go.transform.position = new Vector3(0f, 60f, 0f);  // spawn high in the air

            EnsureComponent<GliderInputAdapter>(go);
            EnsureComponent<GliderCameraProvider>(go);
            EnsureComponent<GliderHUDProvider>(go);
            EnsureComponent<GliderController>(go);

            var rb = go.GetComponent<Rigidbody>();
            if (rb) { rb.mass = 250f; rb.linearDamping = 0.02f; rb.angularDamping = 0.5f; rb.useGravity = false; }

            if (!go.GetComponent<BoxCollider>())
            {
                var col    = go.AddComponent<BoxCollider>();
                col.size   = new Vector3(10f, 0.8f, 5f);   // wide wingspan
                col.center = Vector3.zero;
            }

            // Visual hierarchy
            var rollRoot = CreateChild(go.transform, "RollRoot", Vector3.zero);
            var meshRoot = CreateChild(rollRoot.transform, "MeshRoot", Vector3.zero);

            var enterAnchor = CreateChild(go.transform, "EnterAnchor", new Vector3(0f,  0.2f, 0f));
            var exitAnchor  = CreateChild(go.transform, "ExitAnchor",  new Vector3(0f, -1f,   0f));

            var vcam = FindOrCreateVehicleVCam("Glider_Vcam", go.transform,
                           new Vector3(0f, 3f, -12f));

            const string hudFolder = "Assets/_Project/Gameplay/Vehicles/Glider/HUD/Prefabs";
            EnsureFolderExists(hudFolder);
            var speedoPrefab    = CreateTMProPrefab<GliderSpeedoModule>(hudFolder,   "GliderSpeedo");
            var altitudePrefab  = CreateTMProPrefab<GliderAltitudeModule>(hudFolder, "GliderAltitude");
            var vertSpeedPrefab = CreateTMProPrefab<GliderVertSpeedModule>(hudFolder, "GliderVertSpeed");

            var ctrl = go.GetComponent<GliderController>();
            SetField(ctrl, "_enterAnchor", enterAnchor.transform);
            SetField(ctrl, "_exitAnchor",  exitAnchor.transform);
            SetField(ctrl, "_meshRoot",    meshRoot.transform);
            SetField(ctrl, "_rollRoot",    rollRoot.transform);

            SetField(go.GetComponent<GliderCameraProvider>(), "_vcamGameObject",      vcam);
            SetField(go.GetComponent<GliderHUDProvider>(),    "_speedoPrefab",        speedoPrefab);
            SetField(go.GetComponent<GliderHUDProvider>(),    "_altitudePrefab",      altitudePrefab);
            SetField(go.GetComponent<GliderHUDProvider>(),    "_verticalSpeedPrefab", vertSpeedPrefab);

            var interactable = EnsureComponent<EnterVehicleInteractable>(go);
            SetField(interactable, "_vehicle", ctrl);

            Finish("Glider",
                "Glider spawns at Y=60 (in-air). Possess it with E while Character is nearby. " +
                "Replace RollRoot/MeshRoot with real glider mesh. " +
                "UpArrow/DownArrow = Pitch; LeftArrow/RightArrow = Roll; LShift = Brake. " +
                "Auto-lands when touching ground. Place GO on Interactable layer.");
        }

        // ════════════════════════════════════════════════════════════════════════
        // ROCKET       Game / Vehicle Setup / Rocket
        // ════════════════════════════════════════════════════════════════════════

        [MenuItem("Game/Vehicle Setup/Rocket")]
        public static void SetupRocket()
        {
            var go = FindOrCreateVehicleGO<RocketController>("Rocket");
            go.transform.position = new Vector3(0f, 50f, 0f);  // spawn in the air

            EnsureComponent<RocketInputAdapter>(go);
            EnsureComponent<RocketCameraProvider>(go);
            EnsureComponent<RocketHUDProvider>(go);
            EnsureComponent<RocketController>(go);

            var rb = go.GetComponent<Rigidbody>();
            if (rb) { rb.mass = 500f; rb.linearDamping = 0f; rb.angularDamping = 0.5f; rb.useGravity = false; }

            if (!go.GetComponent<BoxCollider>())
            {
                var col    = go.AddComponent<BoxCollider>();
                col.size   = new Vector3(0.8f, 0.8f, 4f);   // slim rocket body
                col.center = Vector3.zero;
            }

            // Visual hierarchy
            var rollRoot = CreateChild(go.transform, "RollRoot", Vector3.zero);
            var meshRoot = CreateChild(rollRoot.transform, "MeshRoot", Vector3.zero);

            // Exhaust particle placeholder
            var exhaustGO = CreateChild(go.transform, "Exhaust", new Vector3(0f, 0f, -2f));
            var ps = exhaustGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSpeed    = 5f;
            main.startLifetime = 0.5f;
            main.startColor    = new Color(1f, 0.4f, 0f);
            var emission = ps.emission;
            emission.rateOverTime = 0f;   // RocketController drives this

            var enterAnchor = CreateChild(go.transform, "EnterAnchor", new Vector3(0f, 0f,  0f));
            var exitAnchor  = CreateChild(go.transform, "ExitAnchor",  new Vector3(1f, 0f, -1f));

            var vcam = FindOrCreateVehicleVCam("Rocket_Vcam", go.transform,
                           new Vector3(0f, 2f, -12f));

            const string hudFolder = "Assets/_Project/Gameplay/Vehicles/Rocket/HUD/Prefabs";
            EnsureFolderExists(hudFolder);
            var speedoPrefab   = CreateTMProPrefab<RocketSpeedoModule>(hudFolder,   "RocketSpeedo");
            var altitudePrefab = CreateTMProPrefab<RocketAltitudeModule>(hudFolder, "RocketAltitude");
            var throttlePrefab = CreateTMProPrefab<RocketThrottleModule>(hudFolder, "RocketThrottle");

            var ctrl = go.GetComponent<RocketController>();
            SetField(ctrl, "_enterAnchor",       enterAnchor.transform);
            SetField(ctrl, "_exitAnchor",        exitAnchor.transform);
            SetField(ctrl, "_meshRoot",          meshRoot.transform);
            SetField(ctrl, "_rollRoot",          rollRoot.transform);
            SetField(ctrl, "_exhaustParticles",  ps);
            SetField(ctrl, "_exhaustTransform",  exhaustGO.transform);

            SetField(go.GetComponent<RocketCameraProvider>(), "_vcamGameObject",  vcam);
            SetField(go.GetComponent<RocketHUDProvider>(),    "_speedoPrefab",    speedoPrefab);
            SetField(go.GetComponent<RocketHUDProvider>(),    "_altitudePrefab",  altitudePrefab);
            SetField(go.GetComponent<RocketHUDProvider>(),    "_throttlePrefab",  throttlePrefab);

            var interactable = EnsureComponent<EnterVehicleInteractable>(go);
            SetField(interactable, "_vehicle", ctrl);

            Finish("Rocket",
                "Rocket spawns at Y=50 (in-air). Possess with E while Character is nearby. " +
                "W = Throttle; Arrow keys = Pitch/Roll; F = Exit. " +
                "Replace Exhaust ParticleSystem with a proper VFX. " +
                "Replace RollRoot/MeshRoot with real rocket mesh. Place GO on Interactable layer.");
        }

        // ════════════════════════════════════════════════════════════════════════
        // TANK         Game / Vehicle Setup / Tank
        // ════════════════════════════════════════════════════════════════════════

        [MenuItem("Game/Vehicle Setup/Tank")]
        public static void SetupTank()
        {
            var go = FindOrCreateVehicleGO<TankController>("Tank");

            EnsureComponent<TankInputAdapter>(go);
            EnsureComponent<TankCameraProvider>(go);
            EnsureComponent<TankHUDProvider>(go);
            EnsureComponent<TankController>(go);

            var rb = go.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.mass           = 8000f;
                rb.linearDamping  = 0.5f;
                rb.angularDamping = 2f;
                rb.centerOfMass   = new Vector3(0f, -0.5f, 0f);   // lower CoM prevents tipping
            }

            if (!go.GetComponent<BoxCollider>())
            {
                var col    = go.AddComponent<BoxCollider>();
                col.size   = new Vector3(3.4f, 1.8f, 6.0f);
                col.center = new Vector3(0f,   0.9f, 0f);
            }

            // Turret hierarchy: TurretRoot → BarrelRoot → BarrelTip
            var turretRoot = CreateChild(go.transform, "TurretRoot", new Vector3(0f, 1.8f,  0f));
            var barrelRoot = CreateChild(turretRoot.transform, "BarrelRoot", new Vector3(0f, 0f, 0f));
            var barrelTip  = CreateChild(barrelRoot.transform, "BarrelTip",  new Vector3(0f, 0f, 3.5f));

            var enterAnchor = CreateChild(go.transform, "EnterAnchor", new Vector3(0f, 1.5f, 0f));
            var exitAnchor  = CreateChild(go.transform, "ExitAnchor",  new Vector3(3f, 0f,   0f));

            var vcam = FindOrCreateVehicleVCam("Tank_Vcam", go.transform,
                           new Vector3(0f, 4f, -12f));

            const string hudFolder = "Assets/_Project/Gameplay/Vehicles/Tank/HUD/Prefabs";
            EnsureFolderExists(hudFolder);
            var speedoPrefab = CreateTMProPrefab<TankSpeedoModule>(hudFolder, "TankSpeedo");
            var ammoPrefab        = CreateTMProPrefab<TankAmmoModule>(hudFolder,       "TankAmmo");
            var crosshairPrefab   = CreateImagePrefab<TankCrosshairModule>(hudFolder,  "TankCrosshair");
            var fireCooldownPrefab = CreateSliderPrefab<TankFireCooldownModule>(hudFolder, "TankFireCooldown");

            const string shellFolder = "Assets/_Project/Gameplay/Vehicles/Tank/Projectile/Prefabs";
            EnsureFolderExists(shellFolder);
            var shellPrefab = CreateShellPrefab(shellFolder, "TankShell");

            var ctrl = go.GetComponent<TankController>();
            SetField(ctrl, "_enterAnchor", enterAnchor.transform);
            SetField(ctrl, "_exitAnchor",  exitAnchor.transform);
            SetField(ctrl, "_turretRoot",  turretRoot.transform);
            SetField(ctrl, "_barrelRoot",  barrelRoot.transform);
            SetField(ctrl, "_barrelTip",   barrelTip.transform);
            SetField(ctrl, "_shellPrefab", shellPrefab);

            var hud = go.GetComponent<TankHUDProvider>();
            SetField(go.GetComponent<TankCameraProvider>(), "_vcamGameObject",    vcam);
            SetField(hud, "_speedoPrefab",      speedoPrefab);
            SetField(hud, "_ammoPrefab",        ammoPrefab);
            SetField(hud, "_crosshairPrefab",   crosshairPrefab);
            SetField(hud, "_fireCooldownPrefab", fireCooldownPrefab);

            var interactable = EnsureComponent<EnterVehicleInteractable>(go);
            SetField(interactable, "_vehicle", ctrl);

            Finish("Tank",
                "Attach hull mesh under Tank GO; turret mesh under TurretRoot; barrel mesh under BarrelRoot. " +
                "Adjust TurretRoot Y to match turret pivot height on your model. " +
                "Adjust BarrelTip Z to sit at the muzzle end. " +
                "Create Input Action Map 'Vehicle_Tank' with: Throttle(1D), Steer(1D), Look(2D), Fire(Btn), Exit(Btn). " +
                "Place GO on Interactable layer.");
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
                                              Vector3 localPos, float radius, float suspDist,
                                              float fwdStiffness = 2f)
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

            // Higher stiffness prevents forward spin and lateral slide at speed.
            var fwd = wc.forwardFriction;
            fwd.stiffness      = fwdStiffness;
            wc.forwardFriction = fwd;

            var side = wc.sidewaysFriction;
            side.stiffness      = fwdStiffness;
            wc.sidewaysFriction = side;

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

        static GameObject CreateImagePrefab<T>(string folder, string prefabName) where T : MonoBehaviour
        {
            var path     = $"{folder}/{prefabName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(prefabName);
            go.AddComponent<RectTransform>();
            go.AddComponent<UnityEngine.UI.Image>();
            go.AddComponent<T>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[VehicleSetup] Created prefab: {path}");
            return prefab;
        }

        static GameObject CreateSliderPrefab<T>(string folder, string prefabName) where T : MonoBehaviour
        {
            var path     = $"{folder}/{prefabName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(prefabName);
            go.AddComponent<RectTransform>();
            go.AddComponent<UnityEngine.UI.Slider>();
            go.AddComponent<T>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[VehicleSetup] Created prefab: {path}");
            return prefab;
        }

        static GameObject CreateShellPrefab(string folder, string prefabName)
        {
            var path     = $"{folder}/{prefabName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(prefabName);

            var rb = go.AddComponent<Rigidbody>();
            rb.mass                  = 5f;
            rb.useGravity            = false;   // straight flight; enable for arc trajectory
            rb.linearDamping         = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var col    = go.AddComponent<SphereCollider>();
            col.radius = 0.12f;

            go.AddComponent<TankShell>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[VehicleSetup] Created prefab: {path}");
            return prefab;
        }

        static void Finish(string vehicleName, string manualSteps)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[VehicleSetup] {vehicleName} done. Manual steps: {manualSteps} Save scene (Ctrl+S).");
        }
    }
}
