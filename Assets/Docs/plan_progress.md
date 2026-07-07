# Plan Progress — Possession-Based Multi-Entity Control System

*Cập nhật lần cuối: 2026-07-06 (Phase 3 🔄 — code hoàn thành, cần scene setup)*

---

## Tổng quan tiến độ

| Phase | Tên | Trạng thái | % Hoàn thành |
|---|---|---|---|
| Phase 0 | Core Foundation | ✅ Hoàn thành | 100% |
| Phase 1 | Ba track song song (Input / Camera / HUD) | ✅ Hoàn thành | 100% |
| Phase 2 | Possession Skeleton + DummyVehicle | ✅ Hoàn thành | 100% |
| Phase 3 | Character Locomotion + Camera/HUD thật | 🔄 Đang làm | 80% |
| Phase 4 | Interaction System | ⬜ Chưa bắt đầu | 0% |
| Phase 5 | Vehicle thật đầu tiên (Motorcycle) | ⬜ Chưa bắt đầu | 0% |
| Phase 6 | Vehicle thứ 2 & 3 (Car, Airplane) | ⬜ Chưa bắt đầu | 0% |
| Phase 7 | Ability System + Save/Load nền tảng | ⬜ Chưa bắt đầu | 0% |
| Phase 8+ | Mở rộng theo roadmap | ⬜ Chưa bắt đầu | 0% |

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
- [x] `PossessionContext` struct — ✅ REVIEWED: chỉ giữ `PlayerIndex`, mọi field khác phải justify trước khi thêm
- [x] `CameraRigHandle` struct — giữ `GameObject` thay vì Cinemachine type để Core không phụ thuộc Cinemachine package
- [x] `CameraBlendSettings` struct (BlendTime, BlendStyle enum tự định nghĩa)
- [x] `HUDModuleHandle` struct (ModuleId, ModulePrefab)
- [x] `ISaveable` interface (SaveKey, CaptureState, RestoreState) — đặt tại `Core/Persistence/`

### Ghi chú & Quyết định thiết kế (2026-07-06)

**PossessionContext review — FROZEN với 1 field:**
- `PlayerIndex` (int): cần từ đầu để hỗ trợ split-screen tương lai, zero-cost nếu không dùng.
- **KHÔNG thêm:** `IPossessable PreviousPossessable` (tạo reference cycle tiềm ẩn), `bool IsRestoringFromSave` (Save system dùng `ISaveable.RestoreState()` riêng, không qua OnPossess), `Transform SpawnPoint` (xử lý bởi ExitVehicleInteractable, không phải PossessionContext).

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

## Phase 3 — Character Locomotion thật + Camera/HUD thật 🔄

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

### Scene Setup (cần làm trong Unity Editor)
- [ ] Tạo Character GO mới: Character + CharacterController + CharacterInputAdapter + CharacterCameraProvider + CharacterHUDProvider
- [ ] Thêm FirstPerson CinemachineCamera (child của Character, đặt ở vị trí mắt)
- [ ] Gán ThirdPerson VCam + FirstPerson VCam trong `CharacterCameraProvider`
- [ ] Tạo 3 UI prefabs: HealthBar (Slider + HealthBarModule), StaminaBar (Slider + StaminaBarModule), Crosshair (Image + CrosshairModule)
- [ ] Gán prefabs trong `CharacterHUDProvider`
- [ ] Swap `GameBootstrapper._initialPossessable` → Character GO mới
- [ ] Swap `PossessionTester._character` → Character GO mới

---

## Phase 4 — Interaction System ⬜

> **Điều kiện bắt đầu:** Phase 2.  
> **Có thể làm song song với Phase 3** (không phụ thuộc Locomotion chi tiết).

### Checklist

- [ ] `InteractionDetector` component (overlap/raycast tìm IInteractable gần nhất)
- [ ] Character implement `IInteractor`
- [ ] `PickItemInteractable`
- [ ] `PushObjectInteractable` (LocksLocomotion = true)
- [ ] `SitInteractable` (LocksLocomotion = true)
- [ ] `EnterVehicleInteractable` (dùng lại cho Phase 5)
- [ ] `ExitVehicleInteractable`
- [ ] Cờ `LocksLocomotion` trên Character AbilitySystem cơ bản

---

## Phase 5 — Vehicle thật đầu tiên: Motorcycle ⬜

> **Điều kiện bắt đầu:** Phase 3 + Phase 4.

### Checklist

- [ ] `VehicleControllerBase` abstract class (Rigidbody, EnterExitAnchorPoint, IsOccupied)
- [ ] `MotorcycleController` implement IPossessable + VehicleControllerBase
- [ ] Motorcycle physics (Lean, Wheelie, Drift cơ bản)
- [ ] Input map `"Vehicle_Motorcycle"` + `MotorcycleInputAdapter` + `MotorcycleMoveCommand`
- [ ] `MotorcycleCameraRig` (Cinemachine Virtual Camera riêng)
- [ ] Motorcycle HUD Set (Speed, RPM)
- [ ] `EnterVehicleInteractable` trên Motorcycle prefab
- [ ] Kiểm tra: không có file nào trong asmdef khác bị sửa khi thêm Motorcycle

### Tiêu chí pass
- Thêm MotorcycleController **không sửa** PossessionManager, CameraManager, HUDManager, InputManager.
- `Vehicles.Motorcycle` asmdef chỉ reference `Vehicles.Common` + `Core`.

---

## Phase 6 — Car + Airplane (song song) ⬜

> **Điều kiện bắt đầu:** Phase 5.  
> **Có thể giao 2 người làm đồng thời.**

### Car ⬜
- [ ] `CarController` implement IPossessable + VehicleControllerBase
- [ ] Car physics (Gear enum: Drive/Reverse/Neutral — không cần FSM object)
- [ ] Input map `"Vehicle_Car"` + adapter + command
- [ ] Car CameraRig riêng
- [ ] Car HUD Set (Speed, Gear indicator)

### Airplane ⬜
- [ ] `AirplaneController` implement IPossessable + VehicleControllerBase
- [ ] Airplane physics (pitch/yaw/roll)
- [ ] Input map `"Vehicle_Airplane"` + adapter + command
- [ ] Airplane CameraRig riêng
- [ ] Airplane HUD Set (Speed, Altitude, Heading)

### HUD Infrastructure nâng cấp ⬜
- [ ] `ScriptableObject` `HUDModuleSet` (Designer cấu hình qua Inspector)
- [ ] Chuyển HUD prefab sang Addressables

---

## Phase 7 — Ability System + Save/Load (song song) ⬜

> **Điều kiện bắt đầu:** Phase 3 (Track D), Phase 5 (Track E).  
> **Track D và E độc lập nhau.**

### Track D — Ability System ⬜
- [ ] `ICharacterAbility` interface (CanActivate, Activate, Cancel, LocksLocomotion)
- [ ] `AbilitySystem` component tách biệt với LocomotionStateMachine
- [ ] Ability: Crouch (tích hợp lại từ Locomotion)
- [ ] Ability: Interact (nâng cấp từ Phase 4)
- [ ] Ability: PushObject (LocksLocomotion = true)
- [ ] Ability: Sit (LocksLocomotion = true)
- [ ] LocomotionStateMachine hỏi qua interface "có Ability nào đang khoá không" — không biết Ability cụ thể

### Track E — Save/Load ⬜
- [ ] `ISaveable` hoạt động (CaptureState, RestoreState)
- [ ] `SaveService` gọi qua interface, không biết chi tiết từng Possessable
- [ ] Version field `int saveVersion` ngay từ đầu
- [ ] Serialize Character: vị trí, health/stamina, lastCameraMode
- [ ] Serialize Motorcycle: vị trí, vận tốc, damage state
- [ ] World state: IInteractable đã thay đổi (dùng GUID, không dùng index)
- [ ] Không serialize: Component reference, state machine object, HUD state, blend runtime

---

## Phase 8+ — Mở rộng theo roadmap ⬜

> Các hạng mục này chưa được thiết kế chi tiết. Mỗi mục cần một vòng thiết kế riêng khi bắt đầu.

- [ ] Swim / Dive (có thể cần refactor LocomotionStateMachine sang HFSM)
- [ ] ClimbLadder / Vault / Prone
- [ ] Boat / Tank / Helicopter (lặp quy trình Phase 5-6)
- [ ] `GameFlowStateMachine` (Boot → MainMenu → Loading → Gameplay → Paused → GameOver)
- [ ] Inventory / Equipment / Damage System
- [ ] Photo Mode
- [ ] Replay System (dựa trên Command struct từ Phase 1)
- [ ] Accessibility Options
- [ ] Co-op / Split-screen support

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
