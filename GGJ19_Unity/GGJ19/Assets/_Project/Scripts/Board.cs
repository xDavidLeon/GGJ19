using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Board", menuName = "GGJ19/Create Board", order = 1)]
public class Board : ScriptableObject
{
    [System.Serializable]
    public enum ROOM_TYPE
    {
        EMPTY = 0,
        WALL,
        START,
        KITCHEN,
        CORRIDOR,
        DORM,
        BATH,
        LIVING
    }

    [System.Serializable]
    public class TileData
    {
        public ROOM_TYPE roomType;
        public int player;
        public bool connected;
        public int sector; //to which room sector belongs

        public TileData()
        {
            player = -1;
            sector = -1;
            connected = false;
            roomType = ROOM_TYPE.EMPTY;    
        }
    }

    [System.Serializable]
    public class Tile
    {
        public TileData data;
        public GameObject gObject; //floor
        public GameObject gTopWallObject = null; //topwall
        public GameObject gLeftWallObject = null; //leftwall

        public void Clear()
        {
            data.connected = false;
            data.player = -1;
            data.roomType = ROOM_TYPE.EMPTY;
            //GameObject.Destroy( this.gObject );
            //GameObject.Destroy( this.gTopWallObject );
            //GameObject.Destroy( this.gLeftWallObject );
        }
    }

    public Tile[,] tiles;

    public bool initialized = false;

    public int boardWidth = 20;
    public int boardHeight = 20;

    public void InitBoard(int width = 20, int height = 20)
    {
        boardWidth = width;
        boardHeight = height;

        tiles = new Tile[width,height];
        for(int i = 0; i < width; i++)
            for(int j = 0; j < height; j++)
            {
                Tile tile = new Tile();
                tiles[i, j] = tile;

                TileData t =  new TileData();
                tiles[i, j].data = t;

                if(i == 0 || i == width - 1) SetTileState(i,j, ROOM_TYPE.WALL);
                else if(j == 0 || j == height - 1) SetTileState(i, j, ROOM_TYPE.WALL);
                else SetTileState(i, j, ROOM_TYPE.EMPTY);
            }

        initialized = true;
    }

    public Tile GetTile(int x, int y)
    {
        return tiles[x, y];
    }

    public void SetTile(int x, int y, Tile t)
    {
        tiles[x, y].Clear();
        tiles[x, y].data = t.data;
        //populate?
    }

    public ROOM_TYPE GetTileState(int x, int y)
    {
        return tiles[x, y].data.roomType;
    }

    public void SetTileState(int x, int y, ROOM_TYPE state)
    {
        tiles[x, y].data.roomType = state;
    }

    public void SetTilePlayer(int x, int y, int player)
    {
        tiles[x, y].data.player = player;
    }

    public void SetTileConnected(int x, int y, bool connected)
    {
        tiles[x, y].data.connected = connected;
    }

    public static ROOM_TYPE GetRandomRoomType()
    {
        int n = System.Enum.GetNames(typeof(ROOM_TYPE)).Length;
        return (ROOM_TYPE) Random.Range(0, n - 3) + 3;
    }
}
