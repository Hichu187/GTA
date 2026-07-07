using System.Collections.Generic;
using UnityEngine;
using Game.Core.Abilities;

namespace Game.Gameplay.Character.Abilities
{
    public class AbilitySystem : MonoBehaviour
    {
        private readonly List<ICharacterAbility> _abilities = new();

        public bool IsLocomotionLocked
        {
            get
            {
                foreach (var a in _abilities)
                    if (a.IsActive && a.LocksLocomotion) return true;
                return false;
            }
        }

        public void Register(ICharacterAbility ability)   => _abilities.Add(ability);
        public void Unregister(ICharacterAbility ability) => _abilities.Remove(ability);

        public void CancelAllLocking()
        {
            foreach (var a in _abilities)
                if (a.IsActive && a.LocksLocomotion) a.Cancel();
        }
    }
}
