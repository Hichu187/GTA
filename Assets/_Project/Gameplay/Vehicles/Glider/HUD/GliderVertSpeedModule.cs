using UnityEngine;
using TMPro;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Glider
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class GliderVertSpeedModule : MonoBehaviour, IHUDModule
    {
        private TextMeshProUGUI _text;
        private IGliderStats    _stats;

        private void Awake() => _text = GetComponent<TextMeshProUGUI>();

        public void Bind(object dataSource) => _stats = dataSource as IGliderStats;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            if (_stats != null)
                _text.text = $"VS {_stats.VerticalSpeedMs:+0.0;-0.0} m/s";
        }
    }
}
