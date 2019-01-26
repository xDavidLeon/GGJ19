using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileMaterialDictionary : SerializableDictionaryBase<Board.ROOM_TYPE, Material> { }

[CreateAssetMenu(fileName = "TileDatabase", menuName = "GGJ19/Create Tile Database", order = 1)]
public class TileDatabase : ScriptableObject
{
    public GameObject prefabTileFloor;

    public TileMaterialDictionary tileMaterials;

}
