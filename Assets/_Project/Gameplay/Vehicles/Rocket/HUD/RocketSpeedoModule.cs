using UnityEngine;
using TMPro;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Rocket
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class RocketSpeedoModule : MonoBehaviour, IHUDModule
    {
        private TextMeshProUGUI _text;
        private IRocketStats    _stats;

        private void Awake() => _text = GetComponent<TextMeshProUGUI>();

        public void Bind(object dataSource) => _stats = dataSource as IRocketStats;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            if (_stats != null)
                _text.text = $"{_stats.SpeedKmh:0} km/h";
        }
    }
}
