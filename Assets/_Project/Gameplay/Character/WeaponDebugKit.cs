using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core.Weapons;

namespace Game.Gameplay.Character
{
    // DEBUG ONLY — drop on Character GO, assign weapon prefabs in Inspector.
    // Press 1 / 2 to instantly equip without pickup interaction.
    // Remove this component before shipping.
    public class WeaponDebugKit : MonoBehaviour
    {
        [SerializeField] private GameObject _weapon1Prefab;
        [SerializeField] private GameObject _weapon2Prefab;

        private IWeaponHolder _holder;

        private void Awake() => _holder = GetComponent<IWeaponHolder>();

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame) QuickEquip(_weapon1Prefab);
            if (kb.digit2Key.wasPressedThisFrame) QuickEquip(_weapon2Prefab);
        }

        private void QuickEquip(GameObject prefab)
        {
            if (prefab == null || _holder == null) return;

            if (prefab.GetComponent<IWeapon>() == null)
            {
                Debug.LogWarning($"[WeaponDebugKit] {prefab.name} không có IWeapon component.");
                return;
            }

            _holder.ClearAll();
            var weapon = Instantiate(prefab).GetComponent<IWeapon>();
            _holder.PickUp(weapon);
        }
    }
}
