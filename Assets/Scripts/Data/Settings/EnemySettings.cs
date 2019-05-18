using Game.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data.Settings
{
    [CreateAssetMenu(fileName = "EnemySettings", menuName = "Data/Settings/Enemy Gold and Exp settings")]

    public class EnemySettings : ScriptableObject
    {
        public int SmallGold = 1;
        public int NormalGold = 2;
        public int CommanderGold = 4;
        public int FlyingGold = 4;
        public int BossGold = 20;

        public int SmallExp = 1;
        public int NormalExp = 2;
        public int ChampionExp = 4;
        public int FlyingExp = 4;
        public int BossExp = 20;

        //    Mass: 1 gold* (level/8 + 1)
        //Normal: 2 gold* (level/8 + 1)
        //Champion: 4 gold* (level/8 + 1)
        //Air: 4 gold* (level/8 + 1)
        //Boss: 20 gold* (level/8 + 1)
    }
}