using UnityEngine;
using Game.Core.HUD;

namespace Game.Gameplay.Character.HUD
{
    public class CrosshairModule : MonoBehaviour, IHUDModule
    {
        public void Bind(object dataSource) { }
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
