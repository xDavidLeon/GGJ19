using UnityEngine;

[CreateAssetMenu(fileName = "Block", menuName = "GGJ19/Create Block", order = 1)]
public class Block : ScriptableObject
{
    public int[] blockBoard = new int[4 * 4];
}