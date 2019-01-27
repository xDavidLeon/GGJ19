using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public enum GAME_MODE
    {
        CONQUEST = 1,
        HOME = 2
    };

    public GAME_MODE mode = GAME_MODE.CONQUEST;
    public Board board;
    public TileDatabase tileDatabase;
    public BlockDatabase blockDatabase;

    public int boardWidth = 20;
    public int boardHeight = 20;
    public Transform boardContainer;

    public List<PlayerController> players;

    public int numPlayers = 2;
    public int level = 3;
    public int currentPlayerId = 0;
    Vector2[] offsets;

    public int turn = 0;
    public int last_block_id = 0;
    public bool force_corridors = true;
    public bool force_connectivity = true;
    public bool placeBotWalls = false;

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

        //obstacles
        board.SetLevel( level );

        UpdateBoardTileAssets();
    }

    void Update()
    {

    }

    void SetPlayerStartTiles(int player_id)
    {
        int start_x = 0;
        int start_y = 0;

        PlayerController pc = players[player_id];

        if(player_id == 0 || player_id == 1)
            start_y = (int)(boardHeight * 0.5);
        if(player_id == 2 || player_id == 3)
            start_x = (int)(boardWidth * 0.5);
        if(player_id == 1)
            start_x = boardWidth - 1;
        if(player_id == 3)
            start_y = boardHeight - 1;

        pc.startX = start_x;
        pc.startY = start_y;

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
                            if(force_corridors && tile.data.roomType != playBlock.roomType && (
                                (is_corridor && tile.data.roomType == Board.ROOM_TYPE.CORRIDOR) ||
                                (!is_corridor && tile.data.roomType != Board.ROOM_TYPE.CORRIDOR)))
                            {
                                Debug.Log("wrong corridor connection");
                                continue;
                            }

                            if ( force_connectivity && !tile.data.connected && tile.data.roomType != Board.ROOM_TYPE.START )
                            {
                                Debug.Log("not connected");
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
    public bool PlacePlayBlock(PlayBlock playBlock, bool erase = false)
    {
        if(CheckPlacePlayBlock(playBlock) == false && !erase)
            return false;

        int block_id = last_block_id++;
        int startX = (int)playBlock.transform.position.x;
        int startY = (int)playBlock.transform.position.z;
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                if(playBlock.block.GetValue(i, j) == 0)
                    continue;

                Board.ROOM_TYPE type = playBlock.roomType;
                if (erase)
                    type = Board.ROOM_TYPE.EMPTY;

                PlaceTile(startX + i, startY + j, type, currentPlayerId, block_id);

                //conquer neightbours
                if(mode == GAME_MODE.CONQUEST && !erase)
                {
                    for(int k = 0; k < 4; ++k)
                    {
                        Vector2 offset = offsets[k];
                        Board.Tile next = board.GetTile(startX + i + (int)offset.x, startY + j + (int)offset.y);
                        if(next == null)
                            continue;
                        if(next.data.roomType != Board.ROOM_TYPE.WALL &&
                            next.data.roomType != Board.ROOM_TYPE.EMPTY &&
                            next.data.roomType != Board.ROOM_TYPE.START)
                        {
                            next.data.player = currentPlayerId;
                            next.data.roomType = type;
                        }
                    }
                }
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
        Board.Tile tile = board.GetTile(x, y);
        tile.data.roomType = roomState;
        tile.data.player = player_id;
        tile.block_id = block_id;
        PlaceTileGameObject(tile);
    }

    public void NextTurn()
    {
        //mark tiles not connected
        for(int i = 0; i < numPlayers; ++i)
        {
            PlayerController player = players[i];
            if (mode == GAME_MODE.CONQUEST)
                board.ComputeConnectivity(player.playerId);
            player.score = board.ComputePlayerScore(player.playerId);
        }

        // Get new block
        CurrentPlayer.NewBlock();

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
                //if( tile.data.connected )
                PlaceTileGameObject(tile);
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
    /// <param name="tile"></param>
    public GameObject PlaceTileGameObject(Board.Tile tile)
    {
        int x = tile.pos_x;
        int y = tile.pos_y;

        if (tile.gFloor != null)
            GameObject.Destroy(tile.gFloor);

        GameObject g = CreateTileGameObject( tile.data.roomType, tile.data.connected );
        g.name = "Tile_X" + x + "Y" + y;
        g.SetActive( tile.data.roomType != Board.ROOM_TYPE.EMPTY);
        g.transform.parent = boardContainer;
        g.transform.localPosition = new Vector3(x + 0.5f, tileDatabase.boardHeight, y + 0.5f);

        tile.gFloor = g;

        return g;
    }

    public GameObject PlaceWall(Board.Tile tile, Constants.Direction direction)
    {
        if(tile.data.roomType == Board.ROOM_TYPE.EMPTY) return null;
        if(tile.data.roomType == Board.ROOM_TYPE.WALL) return null;
        if(tile.data.roomType == Board.ROOM_TYPE.START) return null;

        // Out of bounds checks
        //if(direction == Constants.Direction.Left && tile.pos_x <= 0) return null;
        //else if(direction == Constants.Direction.Right && tile.pos_x >= boardWidth - 1) return null;
        //else if(direction == Constants.Direction.Bot && tile.pos_y <= 0.0f) return null;
        //else if(direction == Constants.Direction.Top && tile.pos_y >= boardHeight - 1) return null;

        // Where will we store the gameObject?
        GameObject targetGO = null;
        Board.Tile targetTile = null;
        int x = tile.pos_x;
        int y = tile.pos_y;

        if(direction == Constants.Direction.Bot)
        {
            if(y > 0)
                targetTile = board.GetTile(x, y - 1);
            targetGO = tile.gWallBot;
        }
        else if(direction == Constants.Direction.Top)
        {
            if(y < boardHeight - 1)
                targetTile = board.GetTile(x, y + 1);
            targetGO = tile.gWallTop;
        }
        else if(direction == Constants.Direction.Left)
        {
            if(x > 0)
                targetTile = board.GetTile(x - 1, y);
            targetGO = tile.gWallLeft;
        }
        else if(direction == Constants.Direction.Right)
        {
            if(x < boardWidth - 1)
                targetTile = board.GetTile(x + 1, y);
            targetGO = tile.gWallRight;
        }

        if(targetGO != null)
            GameObject.Destroy(targetGO);

        GameObject gPrefab = tileDatabase.prefabWall;
        bool isTargetRoom = targetTile != null && targetTile.data.roomType != Board.ROOM_TYPE.WALL && targetTile.data.roomType != Board.ROOM_TYPE.EMPTY;
        if(!isTargetRoom)
        {
            PlaceWallProp(tile, direction, tileDatabase.RandomWallProp(tile.data.roomType), true);
            if(placeBotWalls == false && direction == Constants.Direction.Bot)
            {
                return null;
            }
            return PlaceWallProp(tile, direction, tileDatabase.prefabWall);
        }
        else if(targetTile.data.player != tile.data.player) return PlaceWallProp(tile, direction, tileDatabase.prefabWall);
        else if(targetTile.data.roomType != tile.data.roomType) return PlaceWallProp(tile, direction, tileDatabase.prefabDoor);


        return null;
    }

    private GameObject PlaceWallProp(Board.Tile tile, Constants.Direction direction, GameObject gPrefab, bool isProp = false)
    {
        if(gPrefab == null) return null;

        GameObject g = CreateWallGameObject(tile.data.roomType, gPrefab);
        g.transform.parent = boardContainer;
        if(isProp)
        {
            GameObject.Destroy(tile.gProp);
            tile.gProp = g;
        }

        if (isProp && !tile.data.connected)
            return null;

        int x = tile.pos_x;
        int y = tile.pos_y;

        if(direction == Constants.Direction.Bot)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallBot-" + gPrefab.name;
            g.transform.localPosition = new Vector3(x + 0.5f, tileDatabase.boardPropHeight, y + 0.5f);
            if(!isProp) tile.gWallBot = g;
        }
        else if(direction == Constants.Direction.Top)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallTop-" + gPrefab.name;
            g.transform.localRotation = Quaternion.Euler(0, 180, 0);
            g.transform.localPosition = new Vector3(x + 0.5f, tileDatabase.boardPropHeight, y + 0.5f);
            if(!isProp) tile.gWallTop = g;
        }
        else if(direction == Constants.Direction.Left)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallLeft-" + gPrefab.name;
            g.transform.localRotation = Quaternion.Euler(0, 90, 0);
            g.transform.localPosition = new Vector3(x + 0.5f, tileDatabase.boardPropHeight, y + 0.5f);
            if(!isProp) tile.gWallLeft = g;
        }
        else if(direction == Constants.Direction.Right)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallRight-" + gPrefab.name;
            g.transform.localRotation = Quaternion.Euler(0, -90, 0);
            g.transform.localPosition = new Vector3(x + 0.5f, tileDatabase.boardPropHeight, y + 0.5f);
            if(!isProp) tile.gWallRight = g;
        }
        g.transform.parent = boardContainer;
        return g;
    }

    /// <summary>
    /// Create the GameObject Visual representation to be used by other methods (board or playblocks)
    /// </summary>
    /// <param name="roomType"></param>
    /// <returns></returns>
    public GameObject CreateTileGameObject(Board.ROOM_TYPE room_type, bool connected = true)
    {
        GameObject g = GameObject.Instantiate(tileDatabase.prefabTileFloor, new Vector3(0, tileDatabase.boardHeight, 0), Quaternion.identity);
        g.transform.localScale = tileDatabase.tileScale;
        Material mat;
        if(connected)
            mat = tileDatabase.tileMaterials[room_type];
        else
            mat = tileDatabase.tileMaterialsDisconnected[room_type];
        g.GetComponent<MeshRenderer>().material = mat;
        return g;
    }

    public GameObject CreateWallGameObject(Board.ROOM_TYPE roomType, GameObject gPrefab)
    {
        GameObject g = GameObject.Instantiate(gPrefab, new Vector3(0, tileDatabase.boardPropHeight, 0), Quaternion.identity);
        //g.transform.localScale = tileDatabase.propScale;
        return g;
    }

    #endregion
}
