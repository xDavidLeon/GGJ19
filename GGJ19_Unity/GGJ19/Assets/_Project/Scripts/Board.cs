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
        CORRIDOR,
        KITCHEN,
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
        public bool interior;

        public TileData()
        {
            player = -1;
            connected = false;
            interior = false;
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
        public GameObject gProp = null;

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

        public bool SameRoom( Tile a )
        {
            if (data.roomType == a.data.roomType &&
                data.player == a.data.player)
                return true;
            return false;
        }

        public void ClearVisuals()
        {
            GameObject.Destroy(gFloor);
            GameObject.Destroy(gWallLeft);
            GameObject.Destroy(gWallRight);
            GameObject.Destroy(gWallTop);
            GameObject.Destroy(gWallBot);
            GameObject.Destroy(gProp);
        }
    }

    public Tile[,] tiles;

    public bool initialized = false;

    public int boardWidth = 20;
    public int boardHeight = 20;

    Vector2[] offsets;

    public void ClearBoard()
    {
        if(tiles == null) return;
        for(int i = 0; i < boardWidth; i++)
            for(int j = 0; j < boardHeight; j++)
            {
                Tile tile = GetTile(i, j);
                if(tile == null) continue;
                tile.Clear();
                tile.ClearVisuals();
            }
    }

    public void InitBoard(int width = 20, int height = 20)
    {
        ClearBoard();

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

    public void SetLevel(int level)
    {
        if (level == 1)
        {
            GetTile((int)(boardWidth * 0.25f), (int)(boardHeight * 0.25f)).data.roomType = Board.ROOM_TYPE.WALL;
            GetTile((int)(boardWidth * 0.25f), (int)(boardHeight * 0.75f)).data.roomType = Board.ROOM_TYPE.WALL;
            GetTile((int)(boardWidth * 0.75f), (int)(boardHeight * 0.25f)).data.roomType = Board.ROOM_TYPE.WALL;
            GetTile((int)(boardWidth * 0.75f), (int)(boardHeight * 0.75f)).data.roomType = Board.ROOM_TYPE.WALL;
        }
        else if (level == 2)
        {
            for (int i = (int)(boardWidth*0.5f) - 2; i <= (int)(boardWidth * 0.5f) + 2; ++i)
                for (int j = (int)(boardHeight * 0.5f) - 2; j <= (int)(boardHeight * 0.5f) + 2; ++j)
                {
                    GetTile(i, j).data.roomType = Board.ROOM_TYPE.WALL;
                }
        }
        else if (level == 3)
        {
            int startx = (int)Mathf.Floor(boardWidth * 0.25f);
            int endx = (int)Mathf.Ceil(boardWidth * 0.7f);
            int starty = (int)Mathf.Floor(boardHeight * 0.25f);
            int endy = (int)Mathf.Ceil(boardHeight * 0.7f);
            for (int i = startx; i <= endx; ++i)
            {
                if (i == (int)Mathf.Floor(boardWidth * 0.49f) || i == (int)Mathf.Floor(boardWidth * 0.51f))
                    continue;
                GetTile(i, starty).data.roomType = Board.ROOM_TYPE.WALL;
                GetTile(i, endy).data.roomType = Board.ROOM_TYPE.WALL;
            }
            for (int j = starty; j <= endy; ++j)
            {
                //if (j == (int)Mathf.Floor(boardHeight * 0.49f) || j == (int)Mathf.Floor(boardHeight * 0.51f))
                //    continue;
                GetTile(startx, j).data.roomType = Board.ROOM_TYPE.WALL;
                GetTile(endx, j).data.roomType = Board.ROOM_TYPE.WALL;
            }
        }
        else if (level == 4)
        {
            for (int i = 0; i < boardWidth; ++i)
            {
                int x = Random.Range(3, boardWidth - 3);
                int y = Random.Range(3, boardHeight - 3);
                GetTile(x,y).data.roomType = Board.ROOM_TYPE.WALL;
            }
        }

    }

    public Tile GetTile(int x, int y)
    {
        if(tiles == null) return null;
        if (x < 0 || x >= boardWidth || y < 0 || y >= boardHeight)
            return null;
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

    public void ComputeConnectivity(int player_id)
    {
        PlayerController pc = GameManager.Instance.players[player_id];

        //clear
        for (int i = 0; i < boardWidth; i++)
            for (int j = 0; j < boardHeight; j++)
            {
                Tile tile = GetTile(i, j);
                if (tile.data.player != player_id)
                    continue;
                tile.data.connected = false;
            }

        //compute connectivity
        Tile start = GetTile(pc.startX, pc.startY);

        List<Tile> pending = new List<Tile>();
        pending.Add(start);

        while (pending.Count > 0)
        {
            Tile current = pending[pending.Count - 1];
            pending.RemoveAt(pending.Count - 1);

            if (current.data.player != player_id)
                continue;

            if(current.data.roomType == ROOM_TYPE.EMPTY ||
                current.data.roomType == ROOM_TYPE.WALL )
                continue;

            if (current.data.roomType != ROOM_TYPE.START &&
                current.data.connected)
                continue;

            current.data.connected = true;
            current.data.interior = false;

            int valid_neightbours = 0;
            for (int k = 0; k < 4; ++k)
            {
                Vector2 offset = offsets[k];
                int x2 = current.pos_x + (int)offset.x;
                int y2 = current.pos_y + (int)offset.y;

                if (x2 < 1 || x2 >= boardWidth - 1 || y2 < 1 || y2 >= boardHeight - 1)
                    continue;
                Tile neighbour = GetTile(x2, y2);
                if (neighbour.SameRoom(current))
                {
                    valid_neightbours++;
                }
                pending.Add(neighbour);
            }

            if (valid_neightbours == 4)
                current.data.interior = true;
        }
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

                int num_tiles = 0; //num tiles per room
                int num_blocks = 1; //num blocks per room
                int num_internal_tiles = 0; //num tiles surrounded

                pending.Clear();
                pending.Add(tile);

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
                    current.data.interior = false;

                    //sector_size
                    num_tiles++;
                    if(used_blocks[current.block_id] == 0)
                    {
                        num_blocks++;
                        used_blocks[current.block_id] = 1; //mark as used
                    }

                    if (num_tiles > 1024)
                    {
                        Debug.Log("INFINITE ERROR IN SCORE");
                        return -1;
                    }

                    int valid_neightbours = 0;
                    for (int k = 0; k < 4; ++k)
                    {
                        Vector2 offset = offsets[k];
                        int x2 = current.pos_x + (int)offset.x;
                        int y2 = current.pos_y + (int)offset.y;

                        if (x2 < 1 || x2 >= boardWidth - 1 || y2 < 1 || y2 >= boardHeight - 1)
                            continue;
                        Tile neighbour = GetTile(x2,y2);
                        if( neighbour.SameRoom(current))
                        {
                            valid_neightbours++;
                            pending.Add( neighbour );
                        }
                    }

                    if (valid_neightbours == 4)
                    {
                        num_internal_tiles++;
                        current.data.interior = true;
                    }
                }

                //room score found
                //score += num_tiles * num_blocks;
                score += num_tiles + num_internal_tiles;
                room_id++;
            }

        return score;
    }

}