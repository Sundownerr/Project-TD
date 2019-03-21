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
}

[Serializable]
public class EnemyCreationRequest
{
    [SerializeField] public EnemyData Data;
    [SerializeField] public Vector3 Position;
}


[Serializable]
public struct SpiritCreationRequestData
{
    [SerializeField] public int[] ID;
    [SerializeField] public int ElementID;
    [SerializeField] public int RarityID;
    [SerializeField] public Vector3 Position;

    public SpiritCreationRequestData(int[] id, int elementID, int rarityID, Vector3 position)
    {
        ID = id;
        ElementID = elementID;
        RarityID = rarityID;
        Position = position;
    }
}

public static class NetworkRequest
{
    public static event EventHandler<SpiritSystem> SpiritCreatingRequestDone = delegate { };
    public static event EventHandler<EnemySystem> EnemyCreatingRequestDone = delegate { };

    #region Spirit Creation Request

    public static void Send(SpiritCreationRequest request)
    {
        var id = request.Data.Id.ToArray();
     
        CmdCreateSpirit(id, (int)request.Data.Element, (int)request.Data.Rarity, request.Position);
    }

    [Command]
    private static void CmdCreateSpirit(int[] id, int elementID, int rarityID, Vector3 position) => RpcCreateSpirit(id, elementID, rarityID, position);

    [ClientRpc]
    private static void RpcCreateSpirit(int[] id, int elementID, int rarityID, Vector3 position)
    {
        var listId = new List<int>(id);
        var spiritData = ReferenceHolder.Get.SpiritDataBase.Spirits.Elements[elementID].Rarities[rarityID].Spirits.Find(x => x.CompareId(listId));
        var choosedCell = ReferenceHolder.Get.Player.CellControlSystem.Cells.Find(x => x.transform.position == position);

        var newSpirit = choosedCell != null ?
            StaticMethods.CreateSpirit(spiritData, choosedCell.GetComponent<Cell>(), ReferenceHolder.Get.Player) :
            StaticMethods.CreateSpirit(spiritData, position, ReferenceHolder.Get.Player);

        SpiritCreatingRequestDone?.Invoke(null, newSpirit);
    }

    #endregion

    #region Enemy Creation Request

    public static void Send(EnemyCreationRequest request)
    {
        var id = request.Data.Id.ToArray();

        CmdCreateEnemy(id, (int)request.Data.Race, request.Position);
    }

    [Command]
    private static void CmdCreateEnemy(int[] id, int race, Vector3 spawnPosition) => RpcCreateEnemy(id, race, spawnPosition);

    [ClientRpc]
    private static void RpcCreateEnemy(int[] id, int race, Vector3 spawnPosition)
    {
        var listID = new List<int>(id);
        var enemyData = ReferenceHolder.Get.EnemyDataBase.Races[race].Enemies.Find(x => x.CompareId(listID));
        var newEnemy = StaticMethods.CreateEnemy(enemyData, ReferenceHolder.Get.Player);

        EnemyCreatingRequestDone?.Invoke(null, newEnemy);
    }

    #endregion
    
}