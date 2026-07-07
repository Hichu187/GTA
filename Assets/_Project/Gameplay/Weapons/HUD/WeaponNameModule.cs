using UnityEngine;
using TMPro;
using Game.Core.HUD;
using Game.Core.Weapons;

namespace Game.Gameplay.Weapons
{
    /// <summary>Displays the name of the currently equipped weapon.</summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class WeaponNameModule : MonoBehaviour, IHUDModule
    {
        private TextMeshProUGUI _text;
        private IWeaponHolder   _holder;

        private void Awake() => _text = GetComponent<TextMeshProUGUI>();

        public void Bind(object dataSource) => _holder = dataSource as IWeaponHolder;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            var weapon = _holder?.CurrentWeapon;
            _text.text = weapon != null ? weapon.WeaponName : string.Empty;
        }
    }
}
