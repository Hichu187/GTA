using UnityEngine;
using Game.Core.Interaction;
using Game.Core.Persistence;
using Game.Core.Weapons;
using Game.Services;

namespace Game.Gameplay.Weapons
{
    /// <summary>Attach to a weapon world-pickup GO. When the Character interacts,
    /// the weapon is added to the Character's WeaponHolder.
    /// If a PersistentGUID is present, the pickup is registered as consumed so it
    /// does not re-appear after save/load.</summary>
    public class WeaponPickupInteractable : MonoBehaviour, IInteractable
    {
        [Tooltip("The WeaponBase component on this or a child GO.")]
        [SerializeField] private MonoBehaviour _weapon;

        private IWeapon        _weaponInterface;
        private PersistentGUID _guid;

        private void Awake()
        {
            _weaponInterface = _weapon as IWeapon
                            ?? _weapon?.GetComponent<IWeapon>();
            _guid = GetComponent<PersistentGUID>();
        }

        public bool CanInteract(IInteractor actor)
        {
            if (_weaponInterface == null) return false;
            return actor is Component c && c.GetComponent<IWeaponHolder>() != null;
        }

        public void Interact(IInteractor actor)
        {
            if (_weaponInterface == null) return;
            if (actor is not Component c) return;

            var holder = c.GetComponent<IWeaponHolder>();
            if (holder == null) return;

            if (holder.PickUp(_weaponInterface))
            {
                // Mark consumed so WorldStateTracker suppresses this on next load
                if (_guid != null && _guid.HasId)
                    GameplayServiceLocator.Current?.WorldStateTracker?.MarkConsumed(_guid.Id);

                // Detach weapon GO from pickup — it will re-parent under the grip
                transform.SetParent(null);
                // Destroy the pickup container but NOT the weapon GO (weapon manages itself)
                Destroy(gameObject);
            }
        }
    }
}
