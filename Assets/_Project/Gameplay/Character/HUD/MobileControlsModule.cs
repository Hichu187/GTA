using Game.Core.HUD;
using Game.Core.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.Character.HUD
{
    public class MobileControlsModule : MonoBehaviour, IHUDModule
    {
        private void Awake()
        {
            var lookPad = transform.Find("LookPad");
            if (lookPad != null && lookPad.GetComponent<LookDragHandler>() == null)
                lookPad.gameObject.AddComponent<LookDragHandler>();
        }

        public void Bind(object dataSource)
        {
            if (dataSource is ILookInjectable lookInjectable)
                GetComponentInChildren<LookDragHandler>()?.Initialize(lookInjectable);

            if (dataSource is IAimToggleable aimToggleable)
            {
                var aimBtn = transform.Find("AimButton")?.GetComponent<Button>();
                if (aimBtn != null)
                    aimBtn.onClick.AddListener(aimToggleable.ToggleAim);
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
