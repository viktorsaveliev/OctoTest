using System.Collections.Generic;
using UnityEngine;

namespace OctoTest.GameplayEntity
{
    public interface IEntityRegistry
    {
        public IReadOnlyCollection<IGameplayEntity> GetActiveEntities();
        public void AddEntity(IGameplayEntity entity);
        public void RemoveEntity(IGameplayEntity entity);
    }
}
