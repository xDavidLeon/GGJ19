using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileMaterialDictionary : SerializableDictionaryBase<Board.ROOM_TYPE, Material> { }

[CreateAssetMenu(fileName = "TileDatabase", menuName = "GGJ19/Create Tile Database", order = 1)]
public class TileDatabase : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject prefabTileFloor;
    public GameObject prefabWall;
    public GameObject prefabDoor;

    [Header("Settings")]
    public float tileScale = 1.0f;

    [Header("Materials")]
    public TileMaterialDictionary tileMaterials;

}
