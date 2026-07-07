using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Persistence;
using Game.Core.Weapons;
using Game.Services;

namespace Game.Gameplay.Weapons
{
    /// <summary>Add this component to the Character GO alongside WeaponBase prefabs.
    /// Character.Update calls Tick() each frame with the current WeaponCommand.</summary>
    public class WeaponHolder : MonoBehaviour, IWeaponHolder, ISaveable
    {
        [Tooltip("Hand bone or grip point transform where weapons attach.")]
        [SerializeField] private Transform _gripPoint;

        [SerializeField] private int            _maxSlots = 5;
        [SerializeField] private WeaponRegistry _registry;

        private readonly List<IWeapon> _slots = new();
        private int _activeIndex = -1;

        public IWeapon CurrentWeapon =>
            (_activeIndex >= 0 && _activeIndex < _slots.Count) ? _slots[_activeIndex] : null;

        public int SlotCount => _slots.Count;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Start()
        {
            GameplayServiceLocator.Current?.SaveService?.Register(this);
        }

        private void OnDestroy()
        {
            GameplayServiceLocator.Current?.SaveService?.Unregister(this);
        }

        // ── IWeaponHolder ─────────────────────────────────────────────────────

        public void Tick(WeaponCommand cmd)
        {
            var weapon = CurrentWeapon;
            if (weapon == null) return;

            if (cmd.FirePressed)
            {
                weapon.UsePrimary();
                if (weapon.IsConsumed) RemoveCurrent();
            }

            if (cmd.AimHeld) weapon.UseSecondary();
            else             weapon.StopSecondary();

            if (cmd.ReloadPressed) weapon.Reload();

            if (_slots.Count > 1)
            {
                if (cmd.SwitchDelta > 0.1f)
                    SwitchTo((_activeIndex + 1) % _slots.Count);
                else if (cmd.SwitchDelta < -0.1f)
                    SwitchTo((_activeIndex - 1 + _slots.Count) % _slots.Count);
            }

            if (cmd.ThrowPressed)
            {
                weapon.UsePrimary();
                if (weapon.IsConsumed) RemoveCurrent();
            }
        }

        public bool PickUp(IWeapon weapon)
        {
            if (weapon == null || _slots.Count >= _maxSlots) return false;

            _slots.Add(weapon);

            if (_slots.Count == 1) SwitchTo(0);

            return true;
        }

        public void Drop()
        {
            if (CurrentWeapon == null) return;
            CurrentWeapon.Unequip();
            RemoveCurrent();
        }

        public void SwitchTo(int index)
        {
            if (index < 0 || index >= _slots.Count) return;

            CurrentWeapon?.Unequip();
            _activeIndex = index;
            _slots[_activeIndex].Equip(_gripPoint);
        }

        /// <summary>Destroys all held weapon GameObjects and resets slot state.</summary>
        public void ClearAll()
        {
            CurrentWeapon?.Unequip();

            foreach (var w in _slots)
            {
                if (w is MonoBehaviour mb && mb != null)
                    Destroy(mb.gameObject);
            }
            _slots.Clear();
            _activeIndex = -1;
        }

        // ── ISaveable ─────────────────────────────────────────────────────────

        public string SaveKey => "WeaponHolder_" + gameObject.name;

        public string CaptureState()
        {
            var slotData = new List<WeaponSlotData>(_slots.Count);
            foreach (var w in _slots)
            {
                slotData.Add(new WeaponSlotData
                {
                    typeName    = w.GetType().Name,
                    currentAmmo = w.CurrentAmmo,
                    reserveAmmo = w.ReserveAmmo,
                });
            }
            return JsonUtility.ToJson(new WeaponHolderData
            {
                slots       = slotData,
                activeIndex = _activeIndex,
            });
        }

        public void RestoreState(string json)
        {
            if (_registry == null)
            {
                Debug.LogWarning("[WeaponHolder] WeaponRegistry not assigned — cannot restore weapons.");
                return;
            }

            ClearAll();

            var data = JsonUtility.FromJson<WeaponHolderData>(json);
            if (data?.slots == null) return;

            foreach (var slot in data.slots)
            {
                if (!_registry.TryGetPrefab(slot.typeName, out var prefab)) continue;

                var go     = Instantiate(prefab);
                var weapon = go.GetComponent<IWeapon>();
                if (weapon == null) { Destroy(go); continue; }

                if (weapon is GunBase gun)
                    gun.SetAmmo(slot.currentAmmo, slot.reserveAmmo);

                _slots.Add(weapon);
            }

            if (_slots.Count > 0)
            {
                int idx = Mathf.Clamp(data.activeIndex, 0, _slots.Count - 1);
                SwitchTo(idx);
            }
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void RemoveCurrent()
        {
            if (_activeIndex < 0 || _activeIndex >= _slots.Count) return;

            _slots.RemoveAt(_activeIndex);

            if (_slots.Count == 0)
            {
                _activeIndex = -1;
                return;
            }

            _activeIndex = Mathf.Clamp(_activeIndex, 0, _slots.Count - 1);
            SwitchTo(_activeIndex);
        }

        // ── Serialization helpers ─────────────────────────────────────────────

        [Serializable]
        private class WeaponSlotData
        {
            public string typeName;
            public int    currentAmmo;
            public int    reserveAmmo;
        }

        [Serializable]
        private class WeaponHolderData
        {
            public List<WeaponSlotData> slots;
            public int                  activeIndex;
        }
    }
}
