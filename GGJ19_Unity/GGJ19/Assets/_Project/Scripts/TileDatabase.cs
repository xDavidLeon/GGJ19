using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileMaterialDictionary : SerializableDictionaryBase<Board.ROOM_TYPE, Material> { }
[System.Serializable]
public class WallPropDictionary : SerializableDictionaryBase<Board.ROOM_TYPE, PrefabList> { }

[CreateAssetMenu(fileName = "TileDatabase", menuName = "GGJ19/Create Tile Database", order = 1)]
public class TileDatabase : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject prefabTileFloor;
    public GameObject prefabWall;
    public GameObject prefabDoor;
    public WallPropDictionary prefabWallProps;

    [Header("Settings")]
    public Vector3 tileScale = Vector3.one;
    //public Vector3 propScale = Vector3.one;
    public float boardHeight = 0.5215f;
    public float boardPropHeight = 0.5215f;
    public float boardPlayblockHeight = 0.75f;

    [Header("Materials")]
    public TileMaterialDictionary tileMaterials;

    public GameObject RandomWallProp(Board.ROOM_TYPE roomType)
    {
        if(prefabWallProps.Count <= 0) return null;
        if(prefabWallProps.ContainsKey(roomType) == false) return null;

        List<GameObject> props = prefabWallProps[roomType].prefabs;
        if(props.Count == 0) return null;

        if(Random.Range(0.0f, 100.0f) > prefabWallProps[roomType].chance) return null;

        return props[Random.Range(0, props.Count)];
    }

}

[System.Serializable]
public class PrefabList
{
    public float chance = 100.0f;
    public List<GameObject> prefabs;
}