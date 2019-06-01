using Game.Systems;
using UnityEngine;
using Game.Data.Databases;

#if UNITY_EDITOR
using NaughtyAttributes;
using UnityEditor;
#endif

namespace Game.Data.Traits
{
    public abstract class Trait : Entity
    {
        public abstract ITraitSystem GetSystem(ITraitComponent owner);

#if UNITY_EDITOR
        [Button("Add to DataBase")]
        public void AddToDataBase()
        {
            if (DataControlSystem.LoadDatabase<TraitDatabase>() is TraitDatabase dataBase)
            {
                if (dataBase.Data.Find(entity => entity.Index == Index) == null)
                {
                    Index = dataBase.Data.Count;

                    dataBase.Data.Add(this);
                    EditorUtility.SetDirty(this);
                    DataControlSystem.Save(dataBase);
                }
                else
                {
                    Debug.LogWarning($"{this} already in data base");
                }
            }
            else
            {
                Debug.LogError($"{typeof(TraitDatabase)} not found");
            }
        }
#endif
    }
}
