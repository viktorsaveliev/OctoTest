using System;
using UnityEngine;

namespace OctoTest.GameplayEntity
{
    public interface IGameplayEntity
    {
        public event Action<IGameplayEntity> OnActiveStateChanged;

        public bool IsActive { get; }
    }
}
