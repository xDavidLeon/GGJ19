using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockDictionary : SerializableDictionaryBase<BLOCK_TYPE, Block> { }

[CreateAssetMenu(fileName = "BlockDatabase", menuName = "GGJ19/Create Block Database", order = 1)]
public class BlockDatabase : ScriptableObject
{
    public BlockDictionary blocks;

    public Block GetRandomBlock()
    {
        int n = System.Enum.GetNames(typeof(BLOCK_TYPE)).Length;
        int r = Random.Range(0, n);
        return blocks[(BLOCK_TYPE)r];
    }

}

public enum BLOCK_TYPE
{
    T,
    S,
    B,
    I,
    L,
    S2,
    L2
}
