# Plan Progress — Possession-Based Multi-Entity Control System

*Cập nhật lần cuối: 2026-07-13 (Swim/Dive 🔄 gameplay xong, đang wire animation — Phase 9+)*

---

## Tổng quan tiến độ

| Phase | Tên | Trạng thái | % Hoàn thành |
|---|---|---|---|
| Phase 0 | Core Foundation | ✅ Hoàn thành | 100% |
| Phase 1 | Ba track song song (Input / Camera / HUD) | ✅ Hoàn thành | 100% |
| Phase 2 | Possession Skeleton + DummyVehicle | ✅ Hoàn thành | 100% |
| Phase 3 | Character Locomotion + Camera/HUD thật | ✅ Hoàn thành | 100% |
| Phase 4 | Interaction System | ✅ Hoàn thành | 100% |
| Phase 5 | Vehicle thật đầu tiên (Motorcycle) | 🔄 Đang làm | 90% |
| Phase 6 | Vehicle thứ 2 & 3 (Car, Airplane) | 🔄 Đang làm | 80% |
| Phase 7 | Weapon System (súng / melee / throwable / consumable) | 🔄 Đang làm | 75% |
| Phase 8 | Ability System + Save/Load nền tảng | 🔄 Đang làm | 90% (Track D + Track E code xong; còn scene setup) |
| Phase 9+ | Mở rộng theo roadmap | ⬜ Chưa bắt đầu | 0% |

**Trạng thái ký hiệu:** ✅ Hoàn thành | 🔄 Đang làm | ⏸ Tạm dừng | ⬜ Chưa bắt đầu | ❌ Bị chặn

---

## Phase 0 — Core Foundation ✅

> **Ưu tiên:** Cần hoàn thành trước mọi phase khác. Mọi asmdef đều phụ thuộc vào phase này.

### Checklist

- [x] Tạo cấu trúc folder `Assets/_Project/Core/` với subfolders (Possession, Camera, HUD, Input, Interaction)
- [x] Thiết lập Assembly Definitions cho tất cả module (Game.Core + 10 asmdef còn lại)
- [x] `Game.Core.asmdef` — `autoReferenced: false`, không reference project asmdef nào
- [x] `IPossessable` interface (OnPossess, OnUnpossess, CameraProvider, HUDProvider, InputProvider)
- [x] `ICameraContextProvider` interface (GetActiveCameraRig, GetBlendSettings)
- [x] `IHUDContextProvider` interface (GetActiveHUDModules)
- [x] `IInputActionMapProvider` interface (ActionMapName, BindActions)
- [x] `IInputBinder` interface (BindAxis2D, BindAxis1D, BindButton — pure C# delegates, không import Unity.InputSystem)
- [x] `IInteractable` interface (CanInteract, Interact)
- [x] `IInteractor` interface (InteractorTransform)
- [x] `IHUDModule` interface (Bind, Show, Hide)
- [x] `PossessionContext` struct — ✅ AMENDED (2026-07-07): thêm `Transform AnchorPoint` (nullable). Justify: Vehicle.EnterAnchor/ExitAnchor cần truyền cho Character.OnPossess/OnUnpossess để biết vị trí ngồi/xuất; không thể giải quyết ở tầng thấp hơn mà không vi phạm kiến trúc.
- [x] `CameraRigHandle` struct — giữ `GameObject` thay vì Cinemachine type để Core không phụ thuộc Cinemachine package
- [x] `CameraBlendSettings` struct (BlendTime, BlendStyle enum tự định nghĩa)
- [x] `HUDModuleHandle` struct (ModuleId, ModulePrefab)
- [x] `ISaveable` interface (SaveKey, CaptureState, RestoreState) — đặt tại `Core/Persistence/`

### Ghi chú & Quyết định thiết kế (2026-07-06)

**PossessionContext review — AMENDED (2026-07-07):**
- `PlayerIndex` (int): cần từ đầu để hỗ trợ split-screen tương lai, zero-cost nếu không dùng.
- `Transform AnchorPoint` (nullable): thêm 2026-07-07 để truyền EnterAnchor/ExitAnchor qua Possess. PossessionManager nhét đúng anchor theo chiều transition:
  - `OnUnpossess(ctx { AnchorPoint = target.EnterAnchor })` → Character biết chỗ ngồi
  - `OnPossess(ctx { AnchorPoint = _current.ExitAnchor })` → Character biết cửa thoát
- **VẪN KHÔNG thêm:** `IPossessable PreviousPossessable` (reference cycle), `bool IsRestoringFromSave` (dùng ISaveable riêng).

**CameraRigHandle dùng `GameObject`, không dùng `CinemachineVirtualCameraBase`:**
- CameraManager (Game.Systems.Camera) tự làm `GetComponent<CinemachineVirtualCameraBase>()`.
- Core không phụ thuộc Cinemachine package — quan trọng nếu sau này muốn swap camera solution.

**IInputBinder dùng plain C# delegates:**
- Không import `Unity.InputSystem` vào Core — InputManager (Game.Systems.Input) implement IInputBinder và ánh xạ sang InputAction.CallbackContext nội bộ.

**Track còn lại của Phase 0:** chỉ còn `ISaveable` (sẽ tạo khi bắt đầu Phase 7), và asmdef cho các module khác (tạo khi bắt đầu từng Phase).

- Core **không được** reference bất kỳ project asmdef nào khác (chỉ UnityEngine built-ins là OK).

---

## Phase 1 — Ba track song song ✅

> **Điều kiện bắt đầu:** Phase 0 hoàn thành.  
> **Có thể chạy song song:** Track A, B, C độc lập nhau.

### Track A — Input System ✅

- [x] `GameInputActions.inputactions` — map `"Character"` (Move/Look/Jump/Sprint/Crouch/Interact) + map `"Dummy_Vehicle"` (rỗng)
- [x] Bindings: Keyboard&Mouse (WASD, Mouse Delta, Space, LShift, LCtrl, E) + Gamepad
- [x] `InputManager` MonoBehaviour — implements `IInputBinder`, `SwitchCurrentActionMap()` tự động unbind bindings cũ trước khi switch
- [x] `IInputBinder` interface — đã có từ Phase 0 (Core/Input/)
- [x] `CharacterMoveCommand` struct (MoveAxis, LookAxis, JumpPressed, SprintHeld, CrouchPressed, InteractPressed) tại `Gameplay/Character/Input/`

### Track B — Camera System ✅

- [x] `CameraManager` MonoBehaviour — `ApplyContext(ICameraContextProvider)`, `RegisterCamera(CinemachineCamera)`
- [x] Priority-based activation: tất cả vcam về 0, target lên 10
- [x] `SetBlend()` → ánh xạ `CameraBlendStyle` → `CinemachineBlendDefinition.Styles` (Cinemachine 3.x / Unity.Cinemachine)
- [x] Không có nhánh rẽ theo loại entity — CameraManager chỉ biết `ICameraContextProvider`
- [ ] **[Scene setup]** CinemachineBrain trên Main Camera + 2 VirtualCamera tạm thời → làm trong Unity Editor khi bắt đầu Phase 2

### Track C — HUD System ✅

- [x] `HUDManager` MonoBehaviour — `ApplyContext(IHUDContextProvider)`, instantiate/destroy modules theo handle list
- [x] `DummyHUDModule` — implements `IHUDModule`, dùng `TextMeshProUGUI` hiển thị label test
- [x] `Game.Systems.HUD.asmdef` được cập nhật thêm `Unity.TextMeshPro` reference
- [x] `IHUDModule` interface — đã có từ Phase 0 (Core/HUD/)
- [ ] **[Scene setup]** Canvas gốc → setup trong Unity Editor khi bắt đầu Phase 2
- [ ] Pool prefab: defer đến Phase 6 (cần đủ Vehicle thật mới thiết kế hợp lý)

---

## Phase 2 — Possession Skeleton + DummyVehicle 🔄

> **Điều kiện bắt đầu:** Phase 0 + Phase 1 (cả 3 track).  
> **Đây là milestone kiểm chứng kiến trúc quan trọng nhất — phải pass trước khi đầu tư vào Phase 3+.**

### Checklist

- [x] `PossessionManager` — `Possess(IPossessable)`, route đúng thứ tự: SwitchMap → OnPossess → BindActions → ApplyCamera → ApplyHUD
- [x] `GameplayServiceLocator` — scene-scoped, reset `Current` khi scene unload
- [x] `GameBootstrapper` — gọi `FindAndRegisterAllCameras()`, sau đó `Possess(initialPossessable)`
- [x] `PossessionTester` — phím F toggle giữa Character và DummyVehicle (temp, xóa ở Phase 4)
- [x] `CharacterStub` + 3 providers — CharacterController.Move thô, map `"Character"`
- [x] `DummyVehicle` + 3 providers — đứng yên, map `"Dummy_Vehicle"` rỗng
- [x] `CameraManager.FindAndRegisterAllCameras()` — auto-discover từ scene, đặt tất cả priority = 0
- [x] `Game.Services.asmdef` — thêm refs: Game.Systems.Input, Game.Systems.Camera, Game.Systems.HUD
- [ ] **[Scene setup trong Unity Editor]** — cần làm thủ công:
  - [ ] Main Camera: gắn `CinemachineBrain`
  - [ ] Tạo 2 `CinemachineCamera` (CharVCam, VehicleVCam) trong scene
  - [ ] `CharacterStubCameraProvider._vcamGameObject` → CharVCam
  - [ ] `DummyVehicleCameraProvider._vcamGameObject` → VehicleVCam
  - [ ] Canvas + `HUDManager`
  - [ ] Manager GameObject: GameplayServiceLocator, PossessionManager, GameBootstrapper, PossessionTester, InputManager, CameraManager, HUDManager
  - [ ] `InputManager._actionAsset` → GameInputActions.inputactions
- [ ] **Chạy thử milestone:**
  - [ ] WASD di chuyển Character, bị tắt khi nhấn F (enter DummyVehicle)
  - [ ] Camera blend giữa CharVCam ↔ VehicleVCam
  - [ ] Không có `if (target is DummyVehicle)` ở bất kỳ Manager nào

### Tiêu chí pass milestone
- Luồng Enter/Exit chạy bằng **một lời gọi duy nhất** `PossessionManager.Possess(target)`.
- Thêm DummyVehicle2 mới **không phải sửa** PossessionManager, CameraManager, HUDManager.
- Không có `if (target is DummyVehicle)` ở tầng Manager.

---

## Phase 3 — Character Locomotion thật + Camera/HUD thật ✅

> **Điều kiện bắt đầu:** Phase 2 pass milestone.

### Core / System (additive, không breaking)
- [x] `ICameraContextProvider` thêm `event System.Action CameraRigChanged`
- [x] `HUDModuleHandle` thêm `object DataSource`
- [x] `HUDManager.ApplyContext` gọi `module.Bind(handle.DataSource)` trước `Show()`
- [x] `PossessionManager.Possess` subscribe/unsubscribe `CameraRigChanged`
- [x] `GameInputActions.inputactions` thêm action `ToggleCamera` (V key, Gamepad right stick)
- [x] Stubs cập nhật no-op event (`CharacterStubCameraProvider`, `DummyVehicleCameraProvider`)

### Locomotion State Machine
- [x] `ILocomotionState` interface
- [x] `LocomotionStateId` enum
- [x] `LocomotionContext` class (Command, Controller, Config, VerticalVelocity, MoveSpeed, StateTimer)
- [x] `CharacterConfig` serializable class (speeds, jump, gravity, thresholds)
- [x] `LocomotionStateMachine` class
- [x] State: Idle
- [x] State: Walk
- [x] State: Run
- [x] State: Sprint
- [x] State: Jump
- [x] State: Fall
- [x] State: Land
- [x] State: Crouch
- [x] `CharacterInputAdapter` (IInputActionMapProvider, binds 7 actions, exposes Command + ConsumeToggleCamera)

### Character Camera Provider
- [x] `CharacterCameraProvider` (ThirdPerson/FirstPerson toggle, fires CameraRigChanged)
- [x] `ICharacterStats` interface (Health, MaxHealth, Stamina, MaxStamina)

### Character Component
- [x] `CharacterHUDProvider` (3 HUDModuleHandles với DataSource = ICharacterStats)
- [x] `Character` MonoBehaviour — implements IPossessable + ICharacterStats + IInteractor, drives FSM

### Character HUD
- [x] HUDModule: `HealthBarModule` (Slider, polls ICharacterStats)
- [x] HUDModule: `StaminaBarModule` (Slider, polls ICharacterStats)
- [x] HUDModule: `CrosshairModule` (static image)

### Transition Mechanics: Character ↔ Vehicle (2026-07-07)
- [x] `IPossessable` thêm `EnterAnchor` + `ExitAnchor` (Transform, nullable) — Character trả về null
- [x] `IPossessable.OnUnpossess()` → `OnUnpossess(PossessionContext)` để nhận anchor
- [x] `Character.OnUnpossess(ctx)` — tắt locomotion, `SetParent(ctx.AnchorPoint)` nếu có (ngồi vào ghế xe)
- [x] `Character.OnPossess(ctx)` — `SetParent(null)`, teleport ra `ctx.AnchorPoint` nếu có (xuất hiện cạnh cửa xe)
- [x] `PossessionManager` truyền `target.EnterAnchor` vào OnUnpossess và `_current.ExitAnchor` vào OnPossess
- [x] `CharacterStub` cập nhật OnUnpossess signature (no-op, đủ compile)

### Scene Setup
- [x] **Editor Setup Wizard** tạo tại `Assets/_Project/Editor/Phase3SetupWizard.cs`
  - Menu: **Game → Phase 3 — Setup Scene** (1 click)
  - Tự động: tạo Character GO, TP_Vcam, FP_Vcam, 3 HUD prefabs, wire toàn bộ SerializedField references
  - `Game.Editor.Setup.asmdef` (Editor-only, references Game.Gameplay.Character + Game.Services + Unity.Cinemachine)
- [ ] **Chạy wizard trong Unity Editor** → lưu scene (Ctrl+S)
- [ ] Trên DummyVehicle: tạo 2 child GO `EnterAnchor` + `ExitAnchor`, gán vào VehicleControllerBase Inspector
- [ ] **Test milestone**: WASD di chuyển, V đổi camera FP/TP, F toggle sang DummyVehicle, Character hiện ở ghế xe

---

## Phase 4 — Interaction System ✅

> **Điều kiện bắt đầu:** Phase 2.

### Checklist

- [x] `InteractionDetector` — `Physics.OverlapSphereNonAlloc`, chọn `IInteractable` gần nhất, `TryInteract(IInteractor)`
- [x] `IInteractor` thêm `SetLocomotionLocked(bool)` — Character implement
- [x] `CharacterInputAdapter.ConsumeInteract()` — one-shot pattern như ConsumeToggleCamera
- [x] `Character.LocomotionLocked` + `SetLocomotionLocked()` — khi locked: FSM không tick, MoveSpeed = 0; E lần 2 unlock
- [x] `EnterVehicleInteractable` — gọi `PossessionManager.Possess(vehicle)` qua GameplayServiceLocator
- [x] `PickItemInteractable` — Destroy GO khi pick up
- [x] `PushObjectInteractable` — `SetLocomotionLocked(true)`; E lần 2 unlock
- [x] `SitInteractable` — teleport đến `_seatPoint` + `SetLocomotionLocked(true)`; E lần 2 unlock
- [x] Exit Vehicle — `PossessionContext.OnExitRequested` callback; `PossessionManager.PossessPrevious()`; `DummyVehicle.OnOccupiedUpdate()` poll F key → invoke callback
- [x] `Game.Gameplay.Interactables.asmdef` thêm ref `Game.Services`

### Scene Setup (cần làm trong Unity Editor)
- [ ] Thêm `InteractionDetector` component lên Character GO (RequireComponent sẽ tự thêm)
- [ ] Trên DummyVehicle: thêm `EnterVehicleInteractable`, gán `_vehicle` = DummyVehicle MonoBehaviour
- [ ] Đặt `_interactableLayer` trên InteractionDetector cho đúng layer của các interactable object
- [ ] Xoá `PossessionTester` khỏi scene (thay thế bởi EnterVehicleInteractable)

---

## Phase 5 — Vehicle thật đầu tiên: Motorcycle ⬜

> **Điều kiện bắt đầu:** Phase 3 + Phase 4.

### Checklist

- [x] `VehicleControllerBase` abstract class (Rigidbody, EnterAnchor, ExitAnchor, IsOccupied) — tạo sớm 2026-07-07
- [x] `VehicleControllerBase.OnOccupiedFixedUpdate()` hook — thêm 2026-07-07 để motorcycle dùng FixedUpdate
- [x] `MotorcycleController : VehicleControllerBase, IMotorcycleStats` — WheelCollider physics (Mức 2)
- [x] Motorcycle physics: WheelCollider motorTorque/brakeTorque drive + steerAngle + PD lean torque controller
- [x] **Mức 1**: AnimationCurve steer restriction (thay speedFactor linear), dynamic air resistance (linearDamping), ground check (WheelCollider.isGrounded)
- [x] **Mức 2**: WheelCollider drive/steer/groundCheck, GetWorldPose mesh sync, handlebar visual, CoM setup, PD lean
- [x] Input map `"Vehicle_Motorcycle"` (W/S/AD/Mouse/F + Gamepad RT/LT/LeftStick/RightStick/B)
- [x] `MotorcycleInputAdapter` (IInputActionMapProvider, ConsumeExitPressed)
- [x] `MotorcycleMoveCommand` struct (Throttle, Brake, Steer, Look)
- [x] `MotorcycleConfig` [Serializable] với [Header] sections: Drive/Steering/Lean/Aerodynamics + AnimationCurve
- [x] `MotorcycleCameraProvider` (ICameraContextProvider, EaseInOut 0.4s)
- [x] `IMotorcycleStats` interface (SpeedKmh, RPM)
- [x] `MotorcycleHUDProvider` (IHUDContextProvider, Speedo + RPM prefab handles)
- [x] `SpeedoModule` (TextMeshProUGUI, polls IMotorcycleStats.SpeedKmh)
- [x] `RPMModule` (TextMeshProUGUI, polls IMotorcycleStats.RPM)
- [x] `Game.Gameplay.Vehicles.Motorcycle.asmdef` thêm Unity.TextMeshPro reference
- [ ] **[Scene setup trong Unity Editor]**:
  - [ ] Tạo Motorcycle GO từ model có sẵn: thêm Rigidbody (mass 180) + MeshCollider/BoxCollider cho thân
  - [ ] Thêm component: MotorcycleController, MotorcycleInputAdapter, MotorcycleCameraProvider, MotorcycleHUDProvider
  - [ ] Tạo 2 WheelCollider child GO (FrontWheel, RearWheel), config: radius, suspensionDistance, forwardFriction, sidewaysFriction. Gán vào MotorcycleController
  - [ ] Gán _frontWheelMesh / _rearWheelMesh (mesh GO của bánh xe). Gán _handlerBar nếu model có
  - [ ] Tạo Motorcycle_Vcam (CinemachineCamera + CinemachineOrbitalFollow + CinemachineRotationComposer), thêm `CinemachineInputAxisController` → `Vehicle_Motorcycle/Look`; gán vào MotorcycleCameraProvider._vcamGameObject
  - [ ] Tạo EnterAnchor + ExitAnchor child GO, gán vào VehicleControllerBase Inspector
  - [ ] Tạo SpeedoPrefab (TextMeshProUGUI + SpeedoModule) + RPMPrefab (TextMeshProUGUI + RPMModule), gán vào MotorcycleHUDProvider
  - [ ] Thêm `EnterVehicleInteractable` lên Motorcycle GO, gán `_vehicle` = MotorcycleController
  - [ ] Test milestone: E lên xe → W tăng tốc → bánh xe mesh xoay đúng → AD quẹo + lean → F xuống xe
- [ ] **[Mức 3 — để sau]** IK Rider: MotorcycleRiderIK component, tay bám tay lái, chân trên bàn đạp, dựa chân khi đứng yên
- [ ] Kiểm chứng: PossessionManager/CameraManager/HUDManager/InputManager không bị sửa ✅

### Tiêu chí pass
- Thêm MotorcycleController **không sửa** PossessionManager, CameraManager, HUDManager, InputManager.
- `Vehicles.Motorcycle` asmdef chỉ reference `Vehicles.Common` + `Core`.

---

## Phase 6 — Car + Airplane (song song) 🔄

> **Điều kiện bắt đầu:** Phase 5.  
> **Có thể giao 2 người làm đồng thời.**

### Editor Setup Wizard ✅
- [x] `VehicleSetupWizard.cs` — 3 menu items: **Game → Vehicle Setup → Motorcycle / Car / Airplane**
  - Mỗi wizard: tạo GO + Rigidbody + BoxCollider + WheelCollider children + Anchor children + VCam + HUD prefabs + wire toàn bộ SerializedField + EnterVehicleInteractable
  - `Game.Editor.Setup.asmdef` thêm refs: Vehicles.Common/Motorcycle/Car/Airplane, Interactables, Unity.TextMeshPro
  - Idempotent: chạy lại không tạo duplicate

### Car ✅ code xong
- [x] `CarController : VehicleControllerBase, ICarStats` — 4 WheelCollider, RWD/FWD/AWD switch
- [x] `GearState` enum (Drive/Neutral/Reverse) + `ICarStats` interface
- [x] Car physics: motorTorque theo DriveType, brake 60/40 rear/front, anti-roll bar, dynamic drag
- [x] Input map `"Vehicle_Car"` (W/S/AD/Mouse/F/H + Gamepad RT/LT/LeftStick/RightStick/B/Y)
- [x] `CarInputAdapter` (ConsumeExitPressed, HornPressed)
- [x] `CarMoveCommand` struct + `CarConfig` (MotorTorque/BrakeTorque/DriveType/AntiRollForce/AnimationCurve)
- [x] `CarCameraProvider`, `CarHUDProvider`, `CarSpeedoModule`, `GearModule`
- [x] `Game.Gameplay.Vehicles.Car.asmdef` thêm Unity.TextMeshPro
- [ ] **[Scene setup]**: Tạo Car GO, 4 WheelCollider child (FL/FR/RL/RR), Car_Vcam, SpeedoPrefab, GearPrefab, EnterVehicleInteractable
- [ ] Test: E lên xe → W tăng tốc → AD quẹo → S phanh → S dừng lại = số lùi → F xuống

### Airplane ✅ code xong
- [x] `AirplaneController : VehicleControllerBase, IAirplaneStats` — thrust + lift + control surfaces
- [x] `IAirplaneStats` (SpeedKmh, AltitudeM, HeadingDeg, ThrottlePct)
- [x] Airplane physics: AddForce thrust (taper near TopSpeed) + lift (speed² × coeff khi > StallSpeed) + control surfaces (PD torque pitch/roll/yaw)
- [x] AnimationCurve `ControlEffectiveness` — giảm control dưới stall speed
- [x] Landing gear WheelCollider[] optional + propeller visual optional
- [x] Input map `"Vehicle_Airplane"` (W/Arrow keys/QE/Space/F + Gamepad)
- [x] `AirplaneInputAdapter` (ConsumeExitPressed, Pitch/Roll/Yaw/Brake axes)
- [x] `AirplaneMoveCommand` struct + `AirplaneConfig` (Thrust/Lift/StallSpeed/PitchTorque/RollTorque/YawTorque)
- [x] `AirplaneCameraProvider`, `AirplaneHUDProvider`, `AirplaneSpeedoModule`, `AltitudeModule`, `HeadingModule`
- [x] `Game.Gameplay.Vehicles.Airplane.asmdef` thêm Unity.TextMeshPro
- [ ] **[Scene setup]**: Tạo Airplane GO, landing gear WheelColliders, Airplane_Vcam (xa hơn và cao hơn car), HUD prefabs, EnterVehicleInteractable
- [ ] **[Tune controls]**: Sau khi thử test, điều chỉnh PitchTorque/RollTorque/LiftCoefficient trong AirplaneConfig để feel tốt

### HUD Infrastructure nâng cấp ⬜
- [ ] `ScriptableObject` `HUDModuleSet` (Designer cấu hình qua Inspector)
- [ ] Chuyển HUD prefab sang Addressables

---

## Phase 7 — Weapon System 🔄

> **Điều kiện bắt đầu:** Phase 4 (Interaction System).  
> **Assembly mới `Game.Gameplay.Weapons` — chỉ depend `Game.Core`, không depend `Game.Gameplay.Character`.**

### Core contracts (Game.Core/Weapons/) ✅
- [x] `DamageType` enum (Bullet, Melee, Explosion, Fire)
- [x] `IDamageable` interface (`CurrentHealth`, `TakeDamage(amount, type)`)
- [x] `IWeaponStats` interface (WeaponName, CurrentAmmo, ReserveAmmo, IsReloading, AimProgress)
- [x] `IWeapon` interface (IWeaponStats + IsConsumed + Equip/Unequip/UsePrimary/UseSecondary/StopSecondary/Reload)
- [x] `WeaponCommand` struct (FirePressed, AimHeld, ReloadPressed, SwitchDelta, ThrowPressed)
- [x] `IWeaponHolder` interface (CurrentWeapon, SlotCount, Tick, PickUp, Drop, SwitchTo)

### Assembly + Base classes ✅
- [x] `Game.Gameplay.Weapons.asmdef` — references `[Game.Core, Unity.TextMeshPro]`
- [x] `WeaponBase` abstract MonoBehaviour — grip attach, Equip/Unequip, default IWeaponStats
- [x] `WeaponHolder` MonoBehaviour — inventory slots, Tick(cmd) dispatch, auto switch after consume

### Guns ✅
- [x] `GunBase` abstract — hitscan raycast, magazine + reserve ammo, reload coroutine, aim lerp (AimProgress), muzzle flash spawn, rate-limited fire
- [x] `Pistol` — 12mag/48res, 4 shots/s, 40m range, 25dmg, 1.2s reload
- [x] `Rifle` — 30mag/90res, 10 shots/s, 80m range, 20dmg, 2.0s reload
- [x] `Shotgun` — 8mag/24res, 1.2 shots/s, 8 pellets × spread 0.08rad, 120dmg total, 2.5s reload

### Melee ✅
- [x] `MeleeBase` abstract — SphereCast forward, light/heavy attack with separate cooldowns, self-hit prevention via `IsChildOf(root)`
- [x] `Knife` — 35dmg light / 70dmg heavy, 1.5m range, 0.35s / 0.9s cooldown
- [x] `Bat` — 50dmg light / 100dmg heavy, 2.0m range, 0.7s / 1.5s cooldown

### Throwables ✅
- [x] `ThrowableBase` abstract — physics Rigidbody launch (isKinematic toggle), cook mechanic (UseSecondary hold → StopSecondary throws), max cook auto-throw, IsConsumed = true after throw
- [x] `Grenade` — 3s fuse, 5m blast radius, 150dmg × distance falloff, VFX spawn optional

### Consumables ✅
- [x] `ConsumableBase` abstract — channeled use coroutine (UseTime), IsConsumed = true after effect
- [x] `MedKit` — 50 HP heal, 1.5s channel, `TakeDamage(-healAmount)` on closest IDamageable parent

### HUD + Interactable ✅
- [x] `WeaponAmmoModule` (IHUDModule, TextMeshPro — "12 / 48" / "RELOADING..." / empty nếu melee)
- [x] `WeaponNameModule` (IHUDModule, TextMeshPro — tên weapon hiện tại)
- [x] `WeaponPickupInteractable` (IInteractable — holder.PickUp, self-Destroy sau pick)

### Character integration ✅
- [x] `Character` thêm `IDamageable` — `TakeDamage()` clamp `[0, maxHealth]`
- [x] `Character.Awake()` — `_weaponHolder = GetComponent<IWeaponHolder>()` (null-safe optional)
- [x] `Character.Update()` — `_weaponHolder?.Tick(_inputAdapter.WeaponCommand)` cuối Update
- [x] `CharacterInputAdapter` — thêm WeaponCommand property (FireHeld, AimHeld, ReloadPending, SwitchDelta, ThrowPending với consume pattern)
- [x] `CharacterHUDProvider` — auto-detect WeaponHolder trong Awake, thêm WeaponAmmo + WeaponName handles khi present
- [x] `GameInputActions.inputactions` — thêm 5 actions vào Character map: Fire (LMB/RT), Aim (RMB/LT), Reload (R/RB), SwitchWeapon (scroll/DpadY), Throw (G/Y)

### Scene setup (cần làm trong Unity Editor) ⬜
- [ ] Thêm `WeaponHolder` component lên Character GO, gán `_gripPoint` (hand bone hoặc right hand transform)
- [ ] Tạo prefab cho từng weapon: GO với WeaponBase subclass, MeshRenderer, collider (disabled khi equip)
- [ ] Gán `_weaponAmmoPrefab` và `_weaponNamePrefab` vào `CharacterHUDProvider` trong Inspector
- [ ] Tạo pickup GO cho mỗi weapon: `WeaponPickupInteractable` + weapon component + SphereCollider (IInteractable trigger)
- [ ] Test: E pickup vũ khí → LMB bắn → raycast hit → R reload → scroll switch → G ném lựu đạn → E pick medkit hồi máu

### Mức nâng cấp sau (pending) ⬜
- [ ] **Mức 3 — IK tay**: Character tay bám súng, IK endpoint = muzzle left-hand grip
- [ ] **Aim Down Sight camera**: CharacterCameraProvider blend sang ADS vcam khi AimProgress > 0.8
- [ ] **Shotgun pump reload**: reload từng viên một (thay vì toàn bộ magazine)
- [ ] **Drive-by shooting**: WeaponHolder.Tick() hoạt động khi character ngồi trên vehicle (in-vehicle weapon)
- [ ] **Damage feedback**: flash đỏ màn hình, ragdoll on death

---

## Phase 8 — Ability System + Save/Load (song song) ⬜

> **Điều kiện bắt đầu:** Phase 3 (Track D), Phase 5 (Track E).  
> **Track D và E độc lập nhau.**

### Track D — Ability System ✅

#### Core (Game.Core/Abilities/) ✅
- [x] `ICharacterAbility` interface (LocksLocomotion, IsActive, Activate, Cancel)

#### Gameplay (Game.Gameplay.Character/Abilities/) ✅
- [x] `AbilitySystem` MonoBehaviour — Register/Unregister, IsLocomotionLocked (OR of all active locking abilities), CancelAllLocking()
- [x] `CrouchAbility` — LocksLocomotion=false, IsActive toggle; Character copies IsActive → LocomotionContext.CrouchRequested
- [x] `InteractAbility` — one-shot (IsActive always false), Activate() → InteractionDetector.TryInteract()
- [x] `LocomotionLockAbility` — LocksLocomotion=true; used by SetLocomotionLocked (Sit, PushObject)

#### Locomotion FSM updates ✅
- [x] `LocomotionContext.CrouchRequested` flag — replaces direct Command.CrouchPressed reads in FSM
- [x] `IdleState`, `WalkState`, `RunState` — transition to Crouch via `ctx.CrouchRequested`
- [x] `CrouchState` — exits when `!ctx.CrouchRequested`

#### Character.cs integration ✅
- [x] Xoá `LocomotionLocked` bool — thay bằng `_abilitySystem.IsLocomotionLocked`
- [x] `SetLocomotionLocked(bool)` delegate sang `_lockAbility.Activate/Cancel` (IInteractor compat giữ nguyên)
- [x] `CharacterInputAdapter.ConsumeCrouch()` — one-shot consume pattern giống ConsumeInteract
- [x] `Update()`: crouch toggle → CrouchAbility; interact → InteractAbility hoặc CancelAllLocking; FSM tick guard dùng AbilitySystem

### Track E — Save/Load 🔄 (Phase 8)

#### Core Persistence (Game.Core.Persistence/) ✅
- [x] `ISaveable` interface — JSON string approach: `SaveKey`, `CaptureState()→string`, `RestoreState(string json)`
- [x] `PersistentGUID` MonoBehaviour — stable identity cho world objects, `#if UNITY_EDITOR AssignNewGUID()`

#### Systems (Game.Systems.Persistence/) ✅
- [x] `Game.Systems.Persistence.asmdef` — references Game.Core
- [x] `SaveFile` [Serializable] — version, sceneName, timestamp, SaveEntry[]
- [x] `SaveEntry` [Serializable] — key + data (JSON sub-string)
- [x] `SaveService` MonoBehaviour — Register/Unregister, Save() (gather → write JSON), Load() (read → RestoreState), version check, DeleteSave()

#### Services (Game.Services/) ✅
- [x] `WorldStateTracker` MonoBehaviour, ISaveable — HashSet<string> consumedGuids, MarkConsumed, IsConsumed, ApplyToScene
- [x] `GameplayServiceLocator` — thêm `SaveService` + `WorldStateTracker` properties
- [x] `Game.Services.asmdef` — thêm `Game.Systems.Persistence` reference

#### Gameplay (Game.Gameplay.Weapons/) ✅
- [x] `WeaponRegistry` ScriptableObject — typeName→prefab lookup, `TryGetPrefab(string, out GameObject)`
- [x] `Game.Gameplay.Weapons.asmdef` — thêm `Game.Services` reference
- [x] `WeaponHolder` — implements ISaveable: CaptureState lưu typeName+ammo per slot; RestoreState: ClearAll → re-instantiate từ WeaponRegistry → SwitchTo
- [x] `WeaponHolder.ClearAll()` — Destroy tất cả weapon GO, clear slots
- [x] `GunBase.SetAmmo(magazine, reserve)` — dùng bởi WeaponHolder.RestoreState
- [x] `WeaponPickupInteractable` — MarkConsumed GUID vào WorldStateTracker sau pickup thành công

#### Character (Game.Gameplay.Character/) ✅
- [x] `Game.Gameplay.Character.asmdef` — thêm `Game.Services` reference
- [x] `Character` implements ISaveable — CaptureState: health/stamina/position/rotY; RestoreState: restore tất cả; Start(): Register, OnDestroy(): Unregister

#### Editor Tool ✅
- [x] `PersistentGUIDAssigner.cs` — MenuItem "Game → Assign Persistent GUIDs" (batch assign); CustomEditor cho PersistentGUID với Inspector button

#### Scene setup (cần làm trong Unity Editor) ⬜
- [ ] Thêm `SaveService` + `WorldStateTracker` component lên Manager GO
- [ ] Gán `SaveService` + `WorldStateTracker` vào `GameplayServiceLocator` Inspector
- [ ] Tạo `WeaponRegistry` asset (Assets → Create → GTA → Weapon Registry), điền typeName → prefab cho mỗi weapon
- [ ] Gán `WeaponRegistry` vào `WeaponHolder._registry` trên Character GO
- [ ] Thêm `PersistentGUID` lên mỗi weapon pickup GO trong scene, chạy **Game → Assign Persistent GUIDs**, lưu scene
- [ ] Test: Save game → pickup weapon → Load game → weapon pickup biến mất, ammo restore đúng

#### Không serialize (by design)
- Component reference, state machine object, HUD state, Camera blend runtime

---

## Phase 9+ — Mở rộng theo roadmap ⬜

> Các hạng mục này chưa được thiết kế chi tiết. Mỗi mục cần một vòng thiết kế riêng khi bắt đầu.

- [ ] ClimbLadder / Vault / Prone
- [ ] Boat (lặp quy trình Phase 5-6)
- [ ] `GameFlowStateMachine` (Boot → MainMenu → Loading → Gameplay → Paused → GameOver)
- [ ] Inventory / Equipment (Phase 7 đã làm Damage System + Weapon core)
- [ ] Photo Mode
- [ ] Replay System (dựa trên Command struct từ Phase 1)
- [ ] Accessibility Options
- [ ] Co-op / Split-screen support

### Helicopter ✅ code xong (2026-07-13)

- [x] `HelicopterController : FlyingVehicleBase, IHelicopterStats` — velocity-override flight (đã có sẵn từ trước), rework toàn bộ vertical control + ground/camera trong phiên này
- [x] **Ground check**: `IsNearGround()` thêm `_groundMask` (LayerMask) + ray gốc nâng lên 1m tránh tự trúng collider của chính helicopter (đồng bộ pattern với `AirplaneController`); thêm `DistanceToGround` public property
- [x] **Engine power system** (thay hoàn toàn cho input lên/xuống trực tiếp kiểu Q/E tức thời cũ):
  - `HelicopterMoveCommand` bỏ `Vertical` + `TakeOff`, thêm `EngineUp` / `EngineDown` (bool, giữ nút)
  - `HelicopterInputAdapter` bind 2 button held-pattern (giống `SprintHeld` bên Character)
  - `GameInputActions.inputactions` map `Vehicle_Helicopter`: xoá action `Vertical` + `TakeOff`, thêm `EngineUp` (Q / Gamepad RT) + `EngineDown` (E / Gamepad LT)
  - `_enginePower` (0-100) ramp qua `EnginePowerRampSpeed` khi giữ nút; giữ nguyên khi thả cả hai
  - Tự động cất cánh (`BeginFlight()`) khi power vượt `LiftoffThreshold` trên mặt đất — không cần nút TakeOff riêng
  - Giữ EngineUp (power đủ ngưỡng) → leo cao, giảm dần khi gần `MaxAltitudeAboveGround` (đo từ `DistanceToGround`, tức so với mặt đất, không phải world Y tuyệt đối); giữ EngineDown → hạ; **thả cả hai → hover giữ nguyên độ cao hiện tại** (không tự trôi lên trần / không tự rơi)
  - Dưới `LiftoffThreshold` + trong phạm vi `LandingHeight` (config, không còn hardcode) → tự động `Land()`
- [x] **Fix bug gravity khi đáp**: `Land()` trước đó gọi `EndFlight()` (bật gravity) rồi ghi đè `useGravity=false` ngay sau đó → helicopter không bao giờ thực sự rơi, lơ lửng cứng tại `LandingHeight`. Đã bỏ dòng ghi đè, để gravity + collider tự xử lý tiếp đất vật lý (đồng bộ `AirplaneController`)
- [x] **Fix bug đóng băng kinematic sớm**: gỡ bỏ mọi chỗ tự động bật `isKinematic=true` dựa theo khoảng cách/power trong lúc chơi (từng có ở `OnGroundFixedUpdate` và `OnPossess`) — nguyên nhân khiến helicopter bị đóng băng giữa không trung ngay khi vào vùng `LandingHeight` dù chưa thật sự chạm đất. Giờ `isKinematic` chỉ `true` ở trạng thái ban đầu từ `Awake()`, chuyển `false` vĩnh viễn ngay lần `OnPossess` đầu tiên
- [x] **Tilt khi bay**: cộng thêm `ClimbTiltAngle` vào pitch — ngóc mũi khi leo, cắm mũi khi hạ, cộng dồn với pitch theo vận tốc ngang có sẵn
- [x] **Fix auto-yaw fighting khi lùi/xoay tại chỗ**: auto-yaw (tự xoay mũi theo hướng bay) giờ chỉ kích hoạt khi `cmd.Horizontal.y > 0.1` (đang chủ động giữ tiến) — không còn tự xoay 180° khi bay lùi, không can thiệp khi chỉ xoay tại chỗ
- [x] **A/D chỉ xoay, không strafe**: `Horizontal.x` (A/D) gộp vào yaw input cùng phím mũi tên; `desiredHorizontal` chỉ còn phụ thuộc `Horizontal.y` (W/S)
- [x] **Camera không follow khi đang xoay tại chỗ**: `HelicopterCameraProvider.HandleLook()` thêm tham số `isManualTurning` — bỏ qua recenter khi người chơi đang chủ động giữ Yaw/A-D, chỉ resume khi hết input xoay
- [x] `HelicopterConfig` thêm đầy đủ field config qua Inspector: `EnginePowerRampSpeed`, `LiftoffThreshold`, `MaxAltitudeAboveGround`, `CeilingSoftZone`, `LandingHeight`, `ClimbTiltAngle`; xoá field chết `MaxHorizontalSpeedKmh` (chưa từng được dùng ở đâu)
- [x] Cập nhật Editor tooling: `MobileHUDWizard.cs` (nút mobile Q/E đổi nhãn ENG+/ENG-, xoá nút TakeOff không còn tác dụng), `VehicleSetupWizard.cs` (help text mô tả điều khiển mới)
- [ ] **[Cần làm trong Unity Editor]**:
  - [ ] Scene cũ (`Test Scene.unity`) có helicopter lưu giá trị `CeilingAltitude` (field đã xoá) — vào Inspector set lại `MaxAltitudeAboveGround`/`LiftoffThreshold`/`EnginePowerRampSpeed`/`LandingHeight` theo ý muốn (đang về mặc định)
  - [ ] Gán `_groundMask` trên `HelicopterController` đúng layer mặt đất trong scene (đang mặc định `-1` = mọi layer)
  - [ ] Chạy lại menu **Game → Mobile HUD → Helicopter** để regenerate `MobileControlsHUD_Helicopter.prefab` (bản cũ còn nút `Btn_TakeOff` chết)
  - [ ] Playtest: giữ Q trên đất → tự cất cánh khi qua ngưỡng; giữ Q trên không → leo rồi hover khi thả; giữ E → hạ rồi hover khi thả, tiếp tục giữ tới khi chạm đất → đáp mượt bằng gravity; A/D chỉ xoay không trượt ngang; bay lùi không tự xoay thân, chỉ ngóc mũi; camera không bị kéo xoay khi giữ A/D một mình

### Swim / Dive 🔄 gameplay xong + đang wire animation (2026-07-13)

> Plan chi tiết: `C:\Users\PC\.claude\plans\curious-rolling-penguin.md` (đã qua Plan Mode, user duyệt trước khi code).
> Quyết định kiến trúc: refactor `LocomotionStateMachine` sang **HFSM tối giản** (không giữ flat) vì
> Prone/Climb/Mounted trong roadmap sẽ cần cùng shape; toggle bật/tắt lặn = 1 field `CanDive` trên
> `CharacterConfig` (không xây hệ thống settings mới).

- [x] **HFSM refactor** (`Locomotion/`) — 8 leaf state cũ (Idle/Walk/.../Crouch) **không sửa gì**:
  - `ILocomotionGroup.cs` (interface mới) + `Groups/WaterborneGroup.cs` — group-guard cấp cha chạy
    trước leaf state mỗi `Tick()`, ép vào `Swim` khi `ctx.IsInWater=true` bất kể state con đang tính
    gì, đẩy ra `Fall` khi rời nước. Thêm group mới sau này (vd `MountedGroup` cho ladder/zipline) chỉ
    cần thêm 1 class vào `LocomotionStateMachine._groups`, không đụng `Tick()` hay state cũ
  - `LocomotionStateId` thêm `Swim`, `Dive`
  - `LocomotionContext` thêm `IsInWater`/`SubmersionDepth`/`WaterSurfaceY` — ghi mỗi frame từ
    `Character.Update()`, cùng chỗ với `_ctx.Command`
- [x] **Water detection** (`Gameplay/Character/Water/` — module mới, project trước đó chưa có bất kỳ
  trigger-volume nào): `WaterZone.cs` (trigger + layer **"Water"** đã reserve sẵn nhưng chưa ai dùng,
  expose `SurfaceY`) + `CharacterWaterDetector.cs` (nhận `OnTriggerEnter/Exit` trực tiếp trên
  `CharacterController`, không cần Rigidbody/polling; tính `SubmersionDepth`; ngập ≥60% capsule mới
  coi là bơi — lội nước nông vẫn đi bộ)
- [x] **Di chuyển bơi/lặn theo camera 3D đầy đủ** (không flatten xuống XZ như đi bộ) — nhìn lên/xuống +
  giữ W bơi đúng theo hướng nhìn, áp dụng cho **cả Swim lẫn Dive** (`isWaterborne` trong
  `Character.Update()`)
- [x] **Tự động lặn, không cần bấm nút riêng** (đổi từ thiết kế ban đầu dùng Jump sau khi test thực tế
  thấy bất tiện): `SwimState` tự chuyển `Dive` khi đang giữ phím di chuyển **và** ngập sâu > 0.15m;
  buoyancy giảm còn 50% sức (`Config.BuoyancyStrength`) để không lấn át việc chủ động bơi xuống;
  `DiveState` có grace period 0.25s trước khi được phép tự trồi lại `Swim` (tránh nhấp nháy ở ranh
  giới mặt nước)
- [x] **Fix bug buoyancy sai** (phát hiện qua debug log thực tế): công thức cũ đặt gốc/chân nhân vật
  ngay tại mặt nước+offset thay vì đầu — khiến cả người nổi hẳn trên mặt nước, `IsInWater` nhấp nháy
  đúng/sai liên tục → state bị đá qua lại Swim↔Fall. Đã sửa theo đúng vị trí đầu (head), dùng
  `CharacterController.center`/`height` giống công thức trong `CharacterWaterDetector`
- [x] **Oxygen / Drowning**: `DamageType` thêm `Drown`; `ICharacterStats` thêm `Oxygen`/`MaxOxygen`;
  tick trong `Character.Update()` cạnh block fall-damage — trừ oxy khi `Dive`, hồi khi Swim/trên cạn,
  hết oxy vẫn Dive → trừ máu liên tục
- [x] `OxygenBarModule.cs` (copy khuôn `StaminaBarModule`) — đăng ký vào `CharacterHUDProvider` qua
  `_oxygenBarPrefab` mới
- [x] `CharacterConfig` thêm `[Header("Swim")]`: `CanDive`, `SwimSpeed`, `DiveSpeed`,
  `SwimSurfaceOffset`, `BuoyancyStrength`, `MaxOxygen`, `OxygenDrainRate`, `OxygenRegenRate`,
  `DrownDamagePerSecond`
- [x] Kiểm chứng: PossessionManager/CameraManager/HUDManager/InputManager **không sửa**; đã test thực
  tế trong Play mode (không chỉ đọc code) — bơi/lặn theo camera hoạt động đúng, xác nhận bởi user
- [x] **Animation wiring (code phía C#)**: `ICharacterAnimationData` thêm `SwimVerticalInput` (-1..1,
  optional cho blend 3D) + `IsDrowned`; `CharacterAnimationDriver.cs` bơm thêm param Animator
  `IsSwimming`/`IsDiving`/`MoveZ`/`IsDrowned` mỗi frame (an toàn nếu param chưa tồn tại trong
  Controller — Unity tự bỏ qua); `Character.Die(DamageType)` tách nhánh: chết vì `Drown` → không
  ragdoll, set `IsDrowned=true` giữ nguyên Animator (ragdoll không có buoyancy sẽ trôi/lật kỳ dưới
  nước); các cái chết khác giữ nguyên ragdoll như cũ
- [ ] **Bug đang mở**: chết đuối vẫn bị ragdoll thay vì giữ pose `SwimDrowned01` — user báo lại đang
  không đúng như kỳ vọng, **chưa tìm nguyên nhân**, cần điều tra tiếp (nghi vấn: animation clip nào
  đó trong quá trình chết vẫn kích hoạt cơ chế ragdoll khác, hoặc `TakeDamage` gọi `Die()` qua đường
  khác chưa được cập nhật theo chữ ký `Die(DamageType)` mới)
- [x] User đã tự setup Animator Controller: blend tree Swim (2D MoveX/MoveY, khuôn giống
  `GroundLocomotion`), Up/Down cho Dive, `SwimDrowned01`
- [ ] **[Còn lại trong Unity Editor]**:
  - [ ] Tạo GameObject nước test trong scene: `BoxCollider isTrigger` + component `WaterZone`, layer
    "Water" (nếu chưa có sẵn từ trước)
  - [ ] Tạo prefab `OxygenBar` (Slider + `OxygenBarModule`), gán vào
    `CharacterHUDProvider._oxygenBarPrefab`
  - [ ] Camera underwater (`_underwaterVcam` tuỳ chọn theo khuôn `_aimVcam` có sẵn trong
    `CharacterCameraProvider`) — chưa làm, để sau

---

## Rủi ro đang theo dõi

| Rủi ro | Mức độ | Kế hoạch giảm thiểu |
|---|---|---|
| `PossessionContext` phình to thành God Object | Cao | Review định kỳ, giới hạn chỉ chứa data cần thiết cho OnPossess |
| Team cross-reference asmdef sai hướng để fix nhanh bug | Cao | Code review bắt buộc kiểm tra asmdef reference |
| Locomotion cần HFSM sớm hơn dự kiến | Trung bình | Thiết kế `ILocomotionState` để refactor sang HFSM chỉ trong module Character |
| HUD Bind trực tiếp cần refactor nếu nhiều consumer | Thấp | Dự trù trong mục 8.2 architecture doc |

---

## Nhật ký cập nhật

| Ngày | Người | Nội dung |
|---|---|---|
| 2026-07-06 | — | Khởi tạo file progress, tất cả phase chưa bắt đầu. Project đang ở giai đoạn setup — chỉ có architecture doc và scene mặc định. |
| 2026-07-06 | — | Phase 0: Tạo Game.Core.asmdef + 13 file C# (interfaces & structs). PossessionContext reviewed & frozen với 1 field (PlayerIndex). Còn lại: ISaveable (defer đến Phase 7). |
| 2026-07-06 | — | Phase 0 HOÀN THÀNH: Thêm ISaveable (Core/Persistence/) + 10 asmdef files (Systems/*, Gameplay/*, Services). Toàn bộ dependency graph đã được enforce bởi compiler. Phase 1 sẵn sàng. |
| 2026-07-06 | — | Phase 1 HOÀN THÀNH: InputManager (IInputBinder impl) + GameInputActions.inputactions + CharacterMoveCommand + CameraManager (Cinemachine 3.x, priority-based) + HUDManager + DummyHUDModule. Scene setup (Camera/Canvas) defer đến Phase 2. |
| 2026-07-06 | — | Phase 2 code XONG: PossessionManager + GameplayServiceLocator + GameBootstrapper + PossessionTester + CharacterStub (4 files) + DummyVehicle (4 files). CameraManager thêm FindAndRegisterAllCameras(). Game.Services.asmdef thêm System refs. Còn scene setup trong Unity Editor. |
| 2026-07-06 | — | Phase 2 PASS: kiến trúc Possession xác nhận (CharacterStub ↔ DummyVehicle toggle qua PossessionManager, không có if/instanceof tại Manager). |
| 2026-07-06 | — | Phase 3 code XONG (80%): 21 file mới + 7 file sửa. LocomotionStateMachine (8 states) + CharacterInputAdapter + CharacterCameraProvider (FP/TP toggle, CameraRigChanged event) + CharacterHUDProvider + Character + HealthBarModule + StaminaBarModule + CrosshairModule. Còn scene setup trong Unity Editor. |
| 2026-07-07 | — | Phase 3 → 90%: Hoàn thiện transition mechanics Character ↔ Vehicle. IPossessable thêm EnterAnchor/ExitAnchor + OnUnpossess(ctx). PossessionContext thêm AnchorPoint (justified). Character.OnPossess/OnUnpossess xử lý parent/unparent + teleport. VehicleControllerBase tạo sớm (thuộc Phase 5) để serve transition. DummyVehicle refactor kế thừa base. 7 file thay đổi/tạo mới. |
| 2026-07-07 | — | Phase 3 ✅ HOÀN THÀNH 100%: Viết Phase3SetupWizard (Editor script, menu Game → Phase 3 — Setup Scene). Tự động tạo Character GO + TP_Vcam + FP_Vcam + 3 HUD prefabs + wire toàn bộ references. Game.Editor.Setup.asmdef (Editor-only). Phase 4 sẵn sàng bắt đầu. |
| 2026-07-07 | — | Phase 4 ✅ HOÀN THÀNH: Interaction System. InteractionDetector (OverlapSphereNonAlloc) + IInteractor.SetLocomotionLocked + CharacterInputAdapter.ConsumeInteract + Character.LocomotionLocked. Exit vehicle qua PossessionContext.OnExitRequested callback → PossessionManager.PossessPrevious(). 4 interactable: EnterVehicle, PickItem, PushObject, Sit. PossessionTester deprecated. |
| 2026-07-07 | — | Phase 5 code 90%: MotorcycleController (VehicleControllerBase + IMotorcycleStats, scripted physics: throttle/brake/steer yaw + lean + flat-velocity alignment). Input map Vehicle_Motorcycle (W/S/AD/Mouse/F + Gamepad). MotorcycleInputAdapter (ConsumeExitPressed). MotorcycleCameraProvider + MotorcycleHUDProvider + SpeedoModule + RPMModule (TMPro). VehicleControllerBase thêm OnOccupiedFixedUpdate hook. Còn scene setup trong Unity Editor. |
| 2026-07-07 | — | Phase 5 nâng cấp Mức 1+2: MotorcycleController rewrite sang WheelCollider physics (motorTorque/brakeTorque/steerAngle) + PD lean torque controller + GetWorldPose mesh sync + handlebar visual + CoM setup. MotorcycleConfig rewrite với AnimationCurve SteerRestrictionCurve + [Header] sections. Mức 3 (IK Rider) đánh dấu để làm sau. |
| 2026-07-07 | — | Phase 6 Editor Wizard xong: VehicleSetupWizard.cs (Game → Vehicle Setup → Motorcycle/Car/Airplane). Tự động tạo GO hierarchy, WheelCollider, Anchor, VCam, HUD prefabs, wire toàn bộ SerializedField, EnterVehicleInteractable. Idempotent. Game.Editor.Setup.asmdef thêm 6 refs mới. |
| 2026-07-07 | — | Phase 6 code 80%: CarController (4-WheelCollider, RWD/FWD/AWD, anti-roll, gear D/N/R) + AirplaneController (thrust+lift+ControlEffectiveness curve+pitch/roll/yaw torque+propeller visual+landing gear). Input map Vehicle_Car + Vehicle_Airplane. 8 HUD modules (CarSpeedo, Gear, AirSpeedo, Altitude, Heading). Còn scene setup + control tuning. |
| 2026-07-07 | — | Phase 7 code 75%: Weapon System. 6 Core contracts (IDamageable/IWeapon/IWeaponHolder/WeaponCommand...). Game.Gameplay.Weapons asmdef (chỉ depend Game.Core). WeaponBase + WeaponHolder. GunBase (raycast, ammo, reload, aim lerp) → Pistol/Rifle/Shotgun. MeleeBase (SphereCast, light/heavy) → Knife/Bat. ThrowableBase (physics launch, cook, IsConsumed) → Grenade. ConsumableBase → MedKit. WeaponAmmoModule + WeaponNameModule + WeaponPickupInteractable. Character implement IDamageable + hook optional WeaponHolder.Tick(). CharacterInputAdapter thêm WeaponCommand (Fire/Aim/Reload/Switch/Throw). CharacterHUDProvider auto-add weapon HUD. GameInputActions thêm 5 actions Character map. Còn scene setup trong Unity Editor. |
| 2026-07-07 | — | Phase 8 Track D ✅ HOÀN THÀNH: Ability System. ICharacterAbility (Core) + AbilitySystem + CrouchAbility + InteractAbility + LocomotionLockAbility (Game.Gameplay.Character.Abilities). LocomotionContext thêm CrouchRequested. 4 FSM states cập nhật. CharacterInputAdapter thêm ConsumeCrouch(). Character.cs: xoá LocomotionLocked bool, wire AbilitySystem, SetLocomotionLocked delegate → LocomotionLockAbility. Không sửa PossessionManager/CameraManager/HUDManager/Interactables. |
| 2026-07-07 | — | Phase 8 Track E code hoàn thành (~90%): Save/Load System. ISaveable (JSON string approach) + PersistentGUID (Core). SaveFile/SaveEntry/SaveService (Game.Systems.Persistence — Register pattern, version-checked JSON file). WorldStateTracker (Game.Services — HashSet<GUID> + ApplyToScene). WeaponRegistry ScriptableObject (typeName→prefab). GunBase.SetAmmo + WeaponHolder.ClearAll + WeaponHolder ISaveable (full restore từ registry). Character ISaveable (health/stamina/position). WeaponPickupInteractable thêm MarkConsumed. GameplayServiceLocator thêm SaveService + WorldStateTracker. Editor tool PersistentGUIDAssigner (MenuItem + CustomEditor Inspector button). asmdef cập nhật: Game.Services thêm Persistence, Game.Gameplay.Weapons + Character thêm Game.Services. Còn scene setup. |
| 2026-07-13 | — | Fix bug xoay camera FP ngược trục Y so với TP: `CharacterCameraProvider.HandleLook` — nhánh First Person dùng `-lookAxis.y` trong khi Third Person/Aim dùng `+lookAxis.y`; đổi FP sang `+` cho khớp chiều. |
| 2026-07-13 | — | Helicopter ✅ code xong (rework toàn diện, xem chi tiết ở mục Phase 9+ → Helicopter): ground raycast thêm `_groundMask` + `DistanceToGround`; thay input Vertical/TakeOff bằng engine power system (EngineUp/EngineDown giữ nút, ramp 0-100, tự cất/hạ cánh theo `LiftoffThreshold`, trần bay so với mặt đất `MaxAltitudeAboveGround`, hover khi thả nút thay vì tự trôi lên trần/rơi); thêm tilt theo tốc độ lên/xuống; fix auto-yaw xoay ngược khi bay lùi; A/D đổi thành chỉ xoay (không strafe); camera ngừng follow khi đang xoay tại chỗ; fix bug gravity không bật khi đáp (`Land()` tự ghi đè `useGravity=false`); fix bug đóng băng kinematic sớm giữa không trung tại `LandingHeight`. Cập nhật `GameInputActions.inputactions`, `MobileHUDWizard.cs`, `VehicleSetupWizard.cs` theo input mới. Còn scene setup thủ công (xem checklist). |
| 2026-07-13 | — | Swim/Dive 🔄 code xong Phase 1-4 (research → Plan Mode → user duyệt plan → code, xem chi tiết ở mục Phase 9+ → Swim/Dive): refactor `LocomotionStateMachine` sang HFSM tối giản (group-guard `ILocomotionGroup`/`WaterborneGroup`, 8 leaf state cũ không sửa gì) để nước luôn thắng logic grounded/fall; module `Water/` mới (`WaterZone` + `CharacterWaterDetector`, dùng layer "Water" có sẵn nhưng chưa ai dùng, lần đầu có trigger-volume trong project); `SwimState`/`DiveState` mới; option bật/tắt lặn = field `CanDive` trên `CharacterConfig` (không xây settings system mới); Oxygen stat + `DamageType.Drown` + `OxygenBarModule`. Còn Phase 5 (animation wiring cho clip Swim/Dive có sẵn + camera underwater) và scene setup thủ công (xem checklist). |
| 2026-07-13 | — | Swim/Dive tiếp: qua debug log thực tế phát hiện + fix 2 bug (buoyancy đặt sai vị trí khiến cả người nổi trên mặt nước gây nhấp nháy Swim↔Fall; DiveState bị bounce ngược Swim ngay lập tức). Đổi thiết kế di chuyển sang camera-relative 3D đầy đủ cho cả Swim lẫn Dive (không chỉ Dive như dự định ban đầu). Theo phản hồi thực tế, bỏ hẳn yêu cầu bấm Jump để lặn — giờ tự động lặn khi giữ phím di chuyển + ngập đủ sâu, buoyancy giảm còn 50% để không lấn át. Bắt đầu wire animation: `CharacterAnimationDriver`/`ICharacterAnimationData` thêm IsSwimming/IsDiving/MoveZ/IsDrowned; `Character.Die(DamageType)` tách nhánh Drown không ragdoll (giữ pose SwimDrowned). User đã tự setup Animator Controller (blend tree Swim, Up/Down, SwimDrowned). Bug đang mở: chết đuối vẫn bị ragdoll thay vì giữ pose — chưa rõ nguyên nhân, cần điều tra tiếp. |
