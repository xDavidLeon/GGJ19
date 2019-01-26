using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Board", menuName = "GGJ19/Create Board", order = 1)]
public class Board : ScriptableObject
{
    public enum TILE_STATE
    {
        EMPTY = 0,
        WALL,
        START,
        KITCHEN,
        CORRIDOR,
        DORM,
        BATH
    }

    [System.Serializable]
    public struct TileData
    {
        public TILE_STATE state;
        public int player;
        public bool connected;
    }

    [System.Serializable]
    public class Tile
    {
        public TileData data;
        public GameObject gObject;
    }

    public int width = 20;
    public int height = 20;

    public Tile[,] tiles;

    public bool initialized = false;

    public void InitBoard()
    {
        tiles = new Tile[width,height];
        for(int i = 0; i < width; i++)
            for(int j = 0; j < height; j++)
            {
                Tile tile = new Tile();
                tiles[i, j] = tile;

                TileData t =  new TileData();
                tiles[i, j].data = t;

                if(i == 0 || i == width - 1) SetTileState(i,j, TILE_STATE.WALL);
                else if(j == 0 || j == height - 1) SetTileState(i, j, TILE_STATE.WALL);
                else SetTileState(i, j, TILE_STATE.EMPTY);
            }

        initialized = true;
    }

    public void SetTileState(int x, int y, TILE_STATE state)
    {
        tiles[x, y].data.state = state;
    }

    public void SetTilePlayer(int x, int y, int player)
    {
        tiles[x, y].data.player = player;
    }

    public void SetTileConnected(int x, int y, bool connected)
    {
        tiles[x, y].data.connected = connected;
    }
}
