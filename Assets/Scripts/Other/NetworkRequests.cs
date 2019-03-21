using Game.Data;
using Game.Enemy;
using Game.Spirit.Data;
using Game.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Game.Cells;
using Game.Spirit;

[Serializable]
public class SpiritCreationRequest
{
    [SerializeField] public SpiritData Data;
    [SerializeField] public Vector3 Position;

    public SpiritCreationRequest(SpiritData data, Vector3 position)
    {
        Data = data;
        Position = position;
    }
}

[Serializable]
public class EnemyCreationRequest
{
    [SerializeField] public EnemyData Data;
    [SerializeField] public Vector3 Position;

    public EnemyCreationRequest(EnemyData data, Vector3 position)
    {
        Data = data;
        Position = position;
    }
}
