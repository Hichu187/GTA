using UnityEngine;
using UnityEngine.UI;
using Game.Core.HUD;
using Game.Gameplay.Character.Stats;

namespace Game.Gameplay.Character.HUD
{
    [RequireComponent(typeof(Slider))]
    public class HealthBarModule : MonoBehaviour, IHUDModule
    {
        private Slider         _slider;
        private ICharacterStats _stats;

        private void Awake() => _slider = GetComponent<Slider>();

        public void Bind(object dataSource) => _stats = dataSource as ICharacterStats;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            if (_stats != null && _stats.MaxHealth > 0f)
                _slider.value = _stats.Health / _stats.MaxHealth;
        }
    }
}
