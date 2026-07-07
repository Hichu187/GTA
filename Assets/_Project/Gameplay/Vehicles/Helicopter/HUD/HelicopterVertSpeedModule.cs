using UnityEngine;
using TMPro;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Helicopter
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class HelicopterVertSpeedModule : MonoBehaviour, IHUDModule
    {
        private TextMeshProUGUI  _text;
        private IHelicopterStats _stats;

        private void Awake() => _text = GetComponent<TextMeshProUGUI>();

        public void Bind(object dataSource) => _stats = dataSource as IHelicopterStats;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            if (_stats != null)
                _text.text = $"VS {_stats.VerticalSpeedMs:+0.0;-0.0} m/s";
        }
    }
}
