using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Board board;
    public TileDatabase tileDatabase;
    public BlockDatabase blockDatabase;

    public int boardWidth = 20;
    public int boardHeight = 20;
    public Transform boardContainer;

    public List<PlayerController> players;

    public int numPlayers = 2;
    public int currentPlayerId = 0;
    Vector2[] offsets;

    public int turn = 0;
    public int last_block_id = 0;
    public bool force_corridors = true;

    public PlayerController CurrentPlayer
    {
        get
        {
            return players[currentPlayerId];
        }
    }

    void Start()
    {
        offsets = new Vector2[4];
        offsets[0].Set(-1, 0);
        offsets[1].Set(+1, 0);
        offsets[2].Set(0, -1);
        offsets[3].Set(0, +1);

        last_block_id = 0;

        board.InitBoard(boardWidth, boardHeight);
        for(int i = 0; i < numPlayers; ++i)
            SetPlayerStartTiles(i);

        board.GetTile( (int)(board.boardWidth * 0.25f), (int)(board.boardHeight * 0.25f) ).data.roomType = Board.ROOM_TYPE.WALL;
        board.GetTile((int)(board.boardWidth * 0.25f), (int)(board.boardHeight * 0.75f)).data.roomType = Board.ROOM_TYPE.WALL;
        board.GetTile((int)(board.boardWidth * 0.75f), (int)(board.boardHeight * 0.25f)).data.roomType = Board.ROOM_TYPE.WALL;
        board.GetTile((int)(board.boardWidth * 0.75f), (int)(board.boardHeight * 0.75f)).data.roomType = Board.ROOM_TYPE.WALL;

        UpdateBoardTileAssets();
    }

    void Update()
    {

    }

    void SetPlayerStartTiles(int player_id)
    {
        int start_x = 0;
        int start_y = 0;

        if(player_id == 0 || player_id == 1)
            start_y = (int)(boardHeight * 0.5);
        if(player_id == 2 || player_id == 3)
            start_x = (int)(boardWidth * 0.5);
        if(player_id == 1)
            start_x = boardWidth - 1;
        if(player_id == 3)
            start_y = boardHeight - 1;

        Board.Tile tile = board.GetTile(start_x, start_y);
        tile.data.player = player_id;
        tile.data.roomType = Board.ROOM_TYPE.START;
    }

    /// <summary>
    /// Check if a PlayBlock could be placed in the board
    /// Called by the PlayerController to place all the tiles from a new block.
    /// </summary>
    /// <param name="playBlock"></param>
    /// <returns></returns>
    public bool CheckPlacePlayBlock(PlayBlock playBlock)
    {
        int startX = (int)playBlock.transform.position.x;
        int startY = (int)playBlock.transform.position.z;

        bool touching_player = false;
        bool is_corridor = playBlock.roomType == Board.ROOM_TYPE.CORRIDOR;

        //check if placeable
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                int x = startX + i;
                int y = startY + j;
                if(x < 0 || x >= board.boardWidth || y < 0 || y >= board.boardHeight)
                    return false;
                int has_cell = playBlock.block.GetValue(i, j);
                if(has_cell != 0 && board.GetTileState(x, y) != Board.ROOM_TYPE.EMPTY)
                {
                    Debug.Log("not empty");
                    return false;
                }
                //check if player id near
                if(!touching_player && has_cell == 1)
                    for(int k = 0; k < 4; ++k)
                    {
                        Vector2 offset = offsets[k];
                        int x2 = x + (int)offset.x;
                        int y2 = y + (int)offset.y;
                        if(x2 < 0 || x2 >= board.boardWidth || y2 < 0 || y2 >= boardHeight)
                            continue; //could happen
                        Board.Tile tile = board.GetTile(x2, y2);
                        if(tile.data.player == currentPlayerId)
                        {
                            //if not touching a corridor
                            if (force_corridors && tile.data.roomType != playBlock.roomType && (
                                (is_corridor && tile.data.roomType == Board.ROOM_TYPE.CORRIDOR) ||
                                (!is_corridor && tile.data.roomType != Board.ROOM_TYPE.CORRIDOR)))
                            {
                                Debug.Log("wrong corridor connection");
                                continue;
                            }
                            touching_player = true;
                            break;
                        }
                    }

            }

        if(!touching_player) //add tip in GUI about not close to player
        {
            Debug.Log("far from player");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Places a block if it fits
    /// </summary>
    /// <returns><c>true</c>, if play block was placed, <c>false</c> otherwise.</returns>
    /// <param name="playBlock">Play block.</param>
    public bool PlacePlayBlock(PlayBlock playBlock)
    {
        if(CheckPlacePlayBlock(playBlock) == false)
            return false;

        int block_id = last_block_id++;
        int startX = (int)playBlock.transform.position.x;
        int startY = (int)playBlock.transform.position.z;
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                if(playBlock.block.GetValue(i, j) != 0)
                    PlaceTile(startX + i, startY + j, playBlock.roomType, currentPlayerId, block_id);
            }
        return true;
    }

    /// <summary>
    /// Places a Tile by setting the board logical state, and placing the visual representation.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tileState"></param>
    public void PlaceTile(int x, int y, Board.ROOM_TYPE roomState, int player_id, int block_id)
    {
        Board.Tile t = board.GetTile(x, y);
        t.data.roomType = roomState;
        t.data.player = player_id;
        t.block_id = block_id;
        PlaceTileGameObject(x, y, roomState);
    }

    public void NextTurn()
    {
        CurrentPlayer.score = board.ComputePlayerScore(CurrentPlayer.playerId);

        // Get new block
        CurrentPlayer.RandomBlock();

        UpdateBoardTileAssets();

        currentPlayerId = (currentPlayerId + 1) % numPlayers;
        if(currentPlayerId == 0) turn++;
    }

    #region VISUALS

    /// <summary>
    /// Updates the full board assets (GameObjects) by reading the tiles roomType
    /// </summary>
    public void UpdateBoardTileAssets()
    {
        if(board == null || board.initialized == false) return;

        for(int i = 0; i < boardWidth; i++)
            for(int j = 0; j < boardHeight; j++)
            {
                Board.Tile tile = board.tiles[i, j];
                tile.ClearVisuals();
                PlaceTileGameObject(i, j, tile.data.roomType);
                PlaceWalls(tile);
            }
    }

    private void PlaceWalls(Board.Tile tile)
    {
        PlaceWall(tile, Constants.Direction.Left);
        PlaceWall(tile, Constants.Direction.Right);
        PlaceWall(tile, Constants.Direction.Top);
        PlaceWall(tile, Constants.Direction.Bot);
    }

    /// <summary>
    /// Sets the visual representation of the Tile according to the roomType
    /// Called by InitBoard and PlaceTile.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="roomType"></param>
    public GameObject PlaceTileGameObject(int x, int y, Board.ROOM_TYPE roomType)
    {
        Board.Tile tile = board.tiles[x, y];

        if(board.tiles[x, y].gFloor != null)
            GameObject.Destroy(board.tiles[x, y].gFloor);

        GameObject g = CreateTileGameObject(roomType);
        g.name = "Tile_X" + x + "Y" + y;
        g.SetActive(roomType != Board.ROOM_TYPE.EMPTY);
        g.transform.parent = boardContainer;
        g.transform.localPosition = new Vector3(x, Constants.boardHeight, y);

        board.tiles[x, y].gFloor = g;

        return g;
    }

    public GameObject PlaceWall(Board.Tile tile, Constants.Direction direction)
    {
        if(tile.data.roomType == Board.ROOM_TYPE.EMPTY) return null;
        if(tile.data.roomType == Board.ROOM_TYPE.WALL) return null;

        // Out of bounds checks
        if(direction == Constants.Direction.Left && tile.pos_x <= 0) return null;
        else if(direction == Constants.Direction.Right && tile.pos_x >= boardWidth - 1) return null;
        else if(direction == Constants.Direction.Bot && tile.pos_y <= 0.0f) return null;
        else if(direction == Constants.Direction.Top && tile.pos_y >= boardHeight - 1) return null;

        // Where will we store the gameObject?
        GameObject targetGO = null;
        Board.Tile targetTile = tile;
        int x = tile.pos_x;
        int y = tile.pos_y;

        if(direction == Constants.Direction.Bot)
        {
            targetTile = board.GetTile(x, y - 1);
            targetGO = tile.gWallBot;
        }
        else if(direction == Constants.Direction.Top)
        {
            targetTile = board.GetTile(x, y + 1);
            targetGO = tile.gWallTop;
        }
        else if(direction == Constants.Direction.Left)
        {
            targetTile = board.GetTile(x - 1, y);
            targetGO = tile.gWallLeft;
        }
        else if(direction == Constants.Direction.Right)
        {
            targetTile = board.GetTile(x + 1, y);
            targetGO = tile.gWallRight;
        }

        if(targetGO != null)
            GameObject.Destroy(targetGO);

        GameObject gPrefab = tileDatabase.prefabWall;
        if(targetTile.data.roomType == Board.ROOM_TYPE.WALL || targetTile.data.roomType == Board.ROOM_TYPE.EMPTY) gPrefab = tileDatabase.prefabWall;
        else if(targetTile.data.roomType != tile.data.roomType) gPrefab = tileDatabase.prefabDoor;
        else return null;

        GameObject g = CreateWallGameObject(tile.data.roomType, gPrefab);
        g.transform.parent = boardContainer;

        if(direction == Constants.Direction.Bot)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallBot-" + gPrefab.name;
            g.transform.localPosition = new Vector3(x, Constants.boardHeight, y);
            tile.gWallBot = g;
        }
        else if(direction == Constants.Direction.Top)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallTop-" + gPrefab.name;
            g.transform.localPosition = new Vector3(x, Constants.boardHeight, y + 1.0f);
            tile.gWallTop = g;
        }
        else if(direction == Constants.Direction.Left)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallLeft-" + gPrefab.name;
            g.transform.localRotation = Quaternion.Euler(0, -90, 0);
            g.transform.localPosition = new Vector3(x, Constants.boardHeight, y);
            tile.gWallLeft = g;
        }
        else if(direction == Constants.Direction.Right)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallRight-" + gPrefab.name;
            g.transform.localRotation = Quaternion.Euler(0, -90, 0);
            g.transform.localPosition = new Vector3(x + 1.0f, Constants.boardHeight, y);
            tile.gWallRight = g;
        }

        return g;
    }

    /// <summary>
    /// Create the GameObject Visual representation to be used by other methods (board or playblocks)
    /// </summary>
    /// <param name="roomType"></param>
    /// <returns></returns>
    public GameObject CreateTileGameObject(Board.ROOM_TYPE roomType)
    {
        GameObject g = GameObject.Instantiate(tileDatabase.prefabTileFloor, new Vector3(0, Constants.boardHeight, 0), Quaternion.identity);
        g.transform.localScale = new Vector3(tileDatabase.tileScale, tileDatabase.tileScale, tileDatabase.tileScale);
        g.GetComponent<MeshRenderer>().material = tileDatabase.tileMaterials[roomType];
        return g;
    }

    public GameObject CreateWallGameObject(Board.ROOM_TYPE roomType, GameObject gPrefab)
    {
        GameObject g = GameObject.Instantiate(gPrefab, new Vector3(0, Constants.boardHeight, 0), Quaternion.identity);
        //g.transform.localScale = new Vector3(tileDatabase.tileScale, tileDatabase.tileScale, tileDatabase.tileScale);
        g.GetComponent<MeshRenderer>().material = tileDatabase.tileMaterials[roomType];
        return g;
    }

    #endregion
}
