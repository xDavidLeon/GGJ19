using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Board board;

    public GameObject prefabTileFloor;

    public Material[] matTileFloor;

    void Start()
    {
        board.InitBoard();

        InitBoardAssets();
    }

    void Update()
    {
        
    }

    void InitBoardAssets()
    {
        if(board == null || board.initialized == false) return;
        for(int i = 0; i < board.width; i++)
            for(int j = 0; j < board.height; j++)
            {
                Board.Tile tile = board.tiles[i, j];
                GameObject g = GameObject.Instantiate(prefabTileFloor, new Vector3(i, 0, j), Quaternion.identity);
                g.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
                g.GetComponent<MeshRenderer>().material = matTileFloor[(int)tile.state];
            }
    }

    private void OnDrawGizmosSelected()
    {
        //if(board == null || board.initialized == false) return;
        //for(int i = 0; i < board.width; i++)
        //    for(int j = 0; j < board.height; j++)
        //    {
        //        Board.Tile tile = board.tiles[i, j];
        //        Gizmos.color = Color.blue;
        //        Gizmos.DrawCube(new Vector3(i, 0, j), new Vector3(1.0f, 0.1f, 1.0f));
        //    }
    }
}
