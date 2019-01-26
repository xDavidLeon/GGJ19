using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Board
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
    public struct Tile
    {
        public TILE_STATE state;
        public int player;
        public bool connected;
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
                Tile t =  new Tile();
                if(i == 0 || i == width - 1) t.state = TILE_STATE.WALL;
                else if(j == 0 || j == height - 1) t.state = TILE_STATE.WALL;
                else t.state = TILE_STATE.EMPTY;
                tiles[i, j] = t;
            }

        initialized = true;
    }

}
