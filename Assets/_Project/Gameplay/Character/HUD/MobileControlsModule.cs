using Game.Core.HUD;
using Game.Core.Input;
using UnityEngine;

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
            if (dataSource is ILookInjectable injectable)
                GetComponentInChildren<LookDragHandler>()?.Initialize(injectable);
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
