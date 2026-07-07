using UnityEngine;
using TMPro;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Airplane
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class HeadingModule : MonoBehaviour, IHUDModule
    {
        private TextMeshProUGUI _text;
        private IAirplaneStats  _stats;

        private void Awake() => _text = GetComponent<TextMeshProUGUI>();

        public void Bind(object dataSource) => _stats = dataSource as IAirplaneStats;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            if (_stats != null)
                _text.text = $"{_stats.HeadingDeg:000}°";
        }
    }
}
