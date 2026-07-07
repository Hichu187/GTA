using TMPro;
using Game.Core.HUD;
using UnityEngine;

namespace Game.Gameplay.Vehicles.Car
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class GearModule : MonoBehaviour, IHUDModule
    {
        private TextMeshProUGUI _text;
        private ICarStats       _stats;

        private void Awake() => _text = GetComponent<TextMeshProUGUI>();

        public void Bind(object dataSource) => _stats = dataSource as ICarStats;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            if (_stats == null) return;
            _text.text = _stats.CurrentGear switch
            {
                GearState.Drive   => "D",
                GearState.Neutral => "N",
                GearState.Reverse => "R",
                _                 => "N",
            };
        }
    }
}
