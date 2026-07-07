using TMPro;
using UnityEngine;
using Game.Core.HUD;

namespace Game.Systems.HUD
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DummyHUDModule : MonoBehaviour, IHUDModule
    {
        private TextMeshProUGUI _text;

        private void Awake() => _text = GetComponent<TextMeshProUGUI>();

        public void Bind(object dataSource)
        {
            if (dataSource is string label)
                _text.text = label;
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
