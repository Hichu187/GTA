using UnityEngine;
using UnityEngine.UI;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Tank
{
    // Slider fill = 1 when cannon is ready, 0 when reloading.
    [RequireComponent(typeof(Slider))]
    public class TankFireCooldownModule : MonoBehaviour, IHUDModule
    {
        private Slider     _slider;
        private ITankStats _stats;

        private void Awake() => _slider = GetComponent<Slider>();

        public void Bind(object dataSource) => _stats = dataSource as ITankStats;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            if (_stats == null) return;
            _slider.value = 1f - _stats.FireCooldownRatio;
        }
    }
}
