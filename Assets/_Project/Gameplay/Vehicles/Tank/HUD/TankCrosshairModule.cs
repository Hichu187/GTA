using UnityEngine;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Tank
{
    public class TankCrosshairModule : MonoBehaviour, IHUDModule
    {
        public void Bind(object dataSource) { }
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
