using System.Collections.Generic;
using UnityEngine;

namespace OctoTest.GameplayEntity
{
    public class EntityRegistry : IEntityRegistry
    {
        private readonly HashSet<IGameplayEntity> _entities = new();
        private readonly HashSet<IGameplayEntity> _activeEntities = new();

        public void AddEntity(IGameplayEntity entity)
        {
            _entities.Add(entity);
            entity.OnActiveStateChanged += OnEntityStateChanged;
        }

        public IReadOnlyCollection<IGameplayEntity> GetActiveEntities() => _activeEntities;

        public void RemoveEntity(IGameplayEntity entity)
        {
            entity.OnActiveStateChanged -= OnEntityStateChanged;
            _entities.Remove(entity);
        }

        private void OnEntityStateChanged(IGameplayEntity entity)
        {
            if (entity.IsActive)
            {
                _activeEntities.Add(entity);
            }
            else
            {
                _activeEntities.Remove(entity);
            }
        }
    }
}
