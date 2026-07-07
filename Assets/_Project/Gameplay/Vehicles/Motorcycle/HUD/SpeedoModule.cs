using UnityEngine;
using TMPro;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Motorcycle
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SpeedoModule : MonoBehaviour, IHUDModule
    {
        private TextMeshProUGUI  _text;
        private IMotorcycleStats _stats;

        private void Awake() => _text = GetComponent<TextMeshProUGUI>();

        public void Bind(object dataSource) => _stats = dataSource as IMotorcycleStats;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            if (_stats != null)
                _text.text = $"{Mathf.Abs(_stats.SpeedKmh):0} km/h";
        }
    }
}
