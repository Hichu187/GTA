using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core.Input;

namespace Game.Gameplay.Character.HUD
{
    // Trackpad-style look input cho mobile.
    // Bám sát UILookDragArea pattern: pointer-ID isolation, dead zone, delta clamp, LateUpdate zero.
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class LookDragHandler : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Tuning")]
        [SerializeField] private float _sensitivity      = 0.05f;
        [SerializeField] private bool  _invertX          = false;
        [SerializeField] private bool  _invertY          = true;
        [SerializeField] private float _deadZone         = 3f;    // pixels
        [SerializeField] private float _maxDeltaPerFrame = 80f;   // pixels, 0 = no clamp

        [Header("Platform")]
        [Tooltip("Disable drag look trong Editor — để mouse look của Input System hoạt động bình thường.")]
        [SerializeField] private bool _disableInEditor = false;

        private ILookInjectable _adapter;

        private bool    _dragging;
        private int     _pointerId    = int.MinValue;
        private Vector2 _lastPos;
        private int     _lastDragFrame = -1;

        public void Initialize(ILookInjectable adapter) => _adapter = adapter;

        // ── Pointer Down — ghi nhớ ngón tay đầu tiên ────────────────────────

        public void OnPointerDown(PointerEventData e)
        {
#if UNITY_EDITOR
            if (_disableInEditor) return;
#endif
            if (_dragging) return;   // đã có ngón tay khác

            _dragging      = true;
            _pointerId     = e.pointerId;
            _lastPos       = e.position;
            _lastDragFrame = -1;
        }

        // ── Drag — tính delta từ position thực (không dùng e.delta vì EventSystem accumulate) ──

        public void OnDrag(PointerEventData e)
        {
#if UNITY_EDITOR
            if (_disableInEditor) return;
#endif
            if (!_dragging || e.pointerId != _pointerId) return;

            _lastDragFrame = Time.frameCount;

            Vector2 delta = e.position - _lastPos;
            _lastPos = e.position;

            // Dead zone — lọc jitter ngón tay
            if (_deadZone > 0f && delta.sqrMagnitude < _deadZone * _deadZone)
            {
                _adapter?.InjectLook(Vector2.zero);
                return;
            }

            // Clamp swipe quá nhanh
            if (_maxDeltaPerFrame > 0f)
                delta = Vector2.ClampMagnitude(delta, _maxDeltaPerFrame);

            if (_invertX) delta.x = -delta.x;
            if (_invertY) delta.y = -delta.y;

            _adapter?.InjectLook(delta * _sensitivity);
        }

        // ── LateUpdate — zero look nếu frame này không có OnDrag ─────────────

        private void LateUpdate()
        {
            if (!_dragging) return;
            if (_lastDragFrame != Time.frameCount)
                _adapter?.InjectLook(Vector2.zero);
        }

        // ── Pointer Up ───────────────────────────────────────────────────────

        public void OnPointerUp(PointerEventData e)
        {
#if UNITY_EDITOR
            if (_disableInEditor) return;
#endif
            if (!_dragging || e.pointerId != _pointerId) return;
            Release();
        }

        // ── OnDisable — clean up nếu bị disable giữa chừng ──────────────────

        private void OnDisable()
        {
            if (_dragging)
                Release();
            else
                _adapter?.InjectLook(Vector2.zero);
        }

        private void Release()
        {
            _dragging      = false;
            _pointerId     = int.MinValue;
            _lastDragFrame = -1;
            _adapter?.InjectLook(Vector2.zero);
        }
    }
}
