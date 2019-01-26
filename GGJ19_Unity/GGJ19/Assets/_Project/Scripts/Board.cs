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

        public TileData()
        {
            player = -1;
            connected = false;
            roomType = ROOM_TYPE.EMPTY;
        }
    }

    [System.Serializable]
    public class Tile
    {
        public int pos_x;
        public int pos_y;
        public int block_id;
        public int room_id;
        public TileData data;
        public GameObject gFloor; //floor
        public GameObject gWallLeft = null; 
        public GameObject gWallRight = null;
        public GameObject gWallTop = null; 
        public GameObject gWallBot = null; 

        public void Clear()
        {
            pos_x = -1;
            pos_y = -1;
            block_id = -1;
            data.connected = false;
            data.player = -1;
            data.roomType = ROOM_TYPE.EMPTY;
            //GameObject.Destroy( this.gObject );
            //GameObject.Destroy( this.gTopWallObject );
            //GameObject.Destroy( this.gLeftWallObject );
        }

        public void ClearVisuals()
        {
            GameObject.Destroy(gFloor);
            GameObject.Destroy(gWallLeft);
            GameObject.Destroy(gWallRight);
            GameObject.Destroy(gWallTop);
            GameObject.Destroy(gWallBot);
        }
    }

    public Tile[,] tiles;

    public bool initialized = false;

    public int boardWidth = 20;
    public int boardHeight = 20;

    Vector2[] offsets;

    public void InitBoard(int width = 20, int height = 20)
    {
        boardWidth = width;
        boardHeight = height;

        offsets = new Vector2[4];
        offsets[0].Set(-1, 0);
        offsets[1].Set(+1, 0);
        offsets[2].Set(0, -1);
        offsets[3].Set(0, +1);

        tiles = new Tile[width, height];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                Tile tile = new Tile();
                tiles[i, j] = tile;
                tile.pos_x = i;
                tile.pos_y = j;

                TileData t = new TileData();
                tiles[i, j].data = t;

                if (i == 0 || i == width - 1) SetTileState(i, j, ROOM_TYPE.WALL);
                else if (j == 0 || j == height - 1) SetTileState(i, j, ROOM_TYPE.WALL);
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
        return (ROOM_TYPE)Random.Range(0, n - 3) + 3;
    }

    public int ComputePlayerScore( int player_id )
    {
        int score = 0;

        int room_id = 0;

        //reset sectors
        for (int i = 0; i < boardWidth; i++)
            for (int j = 0; j < boardHeight; j++)
            {
                Tile tile = GetTile(i, j);
                if (tile.data.player != player_id)
                    continue;
                tile.room_id = -1;
            }

        //search player sectors
        List<Tile> pending = new List<Tile>();
        int[] used_blocks = new int[ GameManager.Instance.last_block_id ];
        for (int i = 0; i < GameManager.Instance.last_block_id; ++i)
            used_blocks[i] = 0;

        for (int i = 1; i < boardWidth - 1; i++)
            for (int j = 1; j < boardHeight - 1; j++)
            {
                Tile tile = GetTile(i, j);

                //needs to expand
                if (tile.data.player != player_id || tile.room_id != -1)
                    continue;

                int room_size = 1; //num tiles per room
                int num_blocks = 1; //num blocks per room

                tile.room_id = room_id;
                pending.Clear();
                pending.Add(GetTile(i - 1, j));
                pending.Add(GetTile(i + 1, j));
                pending.Add(GetTile(i, j - 1));
                pending.Add(GetTile(i, j + 1));

                //compute sector size
                while( pending.Count > 0 )
                {
                    Tile current = pending[pending.Count - 1];
                    pending.RemoveAt(pending.Count - 1);

                    if (current.room_id != -1 || 
                        current.data.player != player_id || 
                        current.data.roomType == ROOM_TYPE.EMPTY ||
                        current.data.roomType == ROOM_TYPE.WALL ||
                        current.data.roomType == ROOM_TYPE.START )
                        continue;

                    current.room_id = room_id;

                    //sector_size
                    room_size++;
                    if(used_blocks[current.block_id] == 0)
                    {
                        num_blocks++;
                        used_blocks[current.block_id] = 1; //mark as used
                    }

                    if (room_size > 1024)
                    {
                        Debug.Log("ERROR IN SCORE");
                        return -1;
                    }

                    if ( current.pos_x > 1)
                        pending.Add(GetTile(current.pos_x - 1, current.pos_y));
                    if (current.pos_x < boardWidth - 1)
                        pending.Add(GetTile(current.pos_x + 1, current.pos_y));
                    if (current.pos_y > 1)
                        pending.Add(GetTile(current.pos_x, current.pos_y - 1));
                    if (current.pos_y < boardHeight - 1)
                        pending.Add(GetTile(current.pos_x, current.pos_y + 1));
                }

                //room score found
                score += room_size * num_blocks;
                room_id++;
            }

        return score;
    }

}