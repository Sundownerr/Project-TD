using System;
using System.Collections.Generic;
using Game.Data.EnemyEntity.Internal;
using Game.Data.SpiritEntity.Internal;
using Game.Wrappers;
using UnityEngine;

namespace Game.Data.Settings
{
    [CreateAssetMenu(fileName = "DamageToArmorSettings", menuName = "Data/Settings/Damage to armor settings")]

    [Serializable]
    public class DamageToArmorSettings : ScriptableObject
    {
        [SerializeField]
        public List<DamageToArmor> DamageToArmorList;

        void Awake()
        {
            if (DamageToArmorList == null)
            {
                var damageTypes = Enum.GetValues(typeof(DamageType));
                var armorTypes = Enum.GetValues(typeof(ArmorType));

                DamageToArmorList = new List<DamageToArmor>(damageTypes.Length);

                for (int i = 0; i < damageTypes.Length; i++)
                {
                    DamageToArmorList.Add(new DamageToArmor()
                    {
                        Type = (DamageType)damageTypes.GetValue(i),
                        Percents = new List<double>(armorTypes.Length)
                    });

                    for (int j = 0; j < armorTypes.Length; j++)
                    {
                        DamageToArmorList[i].Percents.Add(100);
                    }
                }
            }
        }
    }
}