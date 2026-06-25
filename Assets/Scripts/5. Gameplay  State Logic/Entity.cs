using System;
using UnityEngine;

namespace OctoTest.GameplayEntity
{
    public class Entity : MonoBehaviour, IGameplayEntity
    {
        public event Action<IGameplayEntity> OnActiveStateChanged;
        public bool IsActive { get; private set; }

        private IEntityRegistry _registry;

        //[Inject]
        public void Construct(IEntityRegistry registry)
        {
            _registry = registry;
        }

        private void OnEnable()
        {
            _registry.AddEntity(this);
        }

        private void OnDisable()
        {
            _registry.RemoveEntity(this);
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            OnActiveStateChanged?.Invoke(this);
        }
    }
}
