using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
    public int currentPlayer = 0;
    Vector2[] offsets;

    public int turn = 0;
    public int last_block_id = 0;

    void Start()
    {
        offsets = new Vector2[4];
        offsets[0].Set(-1, 0);
        offsets[1].Set(+1, 0);
        offsets[2].Set(0, -1);
        offsets[3].Set(0, +1);

        last_block_id = 0;

        board.InitBoard( boardWidth, boardHeight );
        for(int i = 0; i < numPlayers; ++i)
            SetPlayerStartTiles(i);
        UpdateBoardTileAssets();
    }

    void Update()
    {

    }

    void SetPlayerStartTiles( int player_id )
    {
        int start_x = 0;
        int start_y = 0;

        if (player_id == 0 || player_id == 1)
            start_y = (int)(boardHeight * 0.5);
        if (player_id == 2 || player_id == 3)
            start_x = (int)(boardWidth * 0.5);
        if (player_id == 1)
            start_x = boardWidth - 1;
        if (player_id == 3)
            start_y = boardHeight - 1;

        Board.Tile tile = board.GetTile( start_x, start_y );
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
        int startX = (int) playBlock.transform.position.x;
        int startY = (int) playBlock.transform.position.z;

        bool touching_player = false;

        //check if placeable
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                int x = startX + i;
                int y = startY + j;
                if (x < 0 || x >= board.boardWidth || y < 0 || y >= board.boardHeight)
                    return false;
                int has_cell = playBlock.block.GetValue(i, j);
                if (has_cell != 0 && board.GetTileState(x, y) != Board.ROOM_TYPE.EMPTY)
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
                        if (x2 < 0 || x2 >= board.boardWidth || y2 < 0 || y2 >= boardHeight)
                            continue; //could happen
                        Board.Tile tile = board.GetTile( x2,y2 );
                        if (tile.data.player == currentPlayer)
                        {
                            touching_player = true;
                            break;
                        }
                    }

            }

        if (!touching_player) //add tip in GUI about not close to player
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
        if (CheckPlacePlayBlock(playBlock) == false)
            return false;

        int block_id = last_block_id++;
        int startX = (int)playBlock.transform.position.x;
        int startY = (int)playBlock.transform.position.z;
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
            {
                if (playBlock.block.GetValue(i, j) != 0)
                    PlaceTile(startX + i, startY + j, playBlock.roomType, currentPlayer, block_id);
            }
        return true;
    }

    /// <summary>
    /// Places a Tile by setting the board logical state, and placing the visual representation.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tileState"></param>
    public void PlaceTile(int x, int y, Board.ROOM_TYPE roomState, int player_id, int block_id )
    {
        Board.Tile t = board.GetTile(x, y);
        t.data.roomType = roomState;
        t.data.player = player_id;
        t.block_id = block_id;
        PlaceTileGameObject( x, y, roomState );
    }

    public void onEndPlayerTurn()
    {
        // TODO if it was a valid placement, check if game is over. If not, give control to next player
        PlayerData p = players[currentPlayer];
        currentPlayer = (currentPlayer + 1) % numPlayers;


        if (currentPlayer == 0)
        {
            turn++;
        }
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
                PlaceTileGameObject(i, j, tile.data.roomType);
            }
    }


    /// <summary>
    /// Sets the visual representation of the Tile according to the roomType
    /// Called by InitBoard and PlaceTile.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="roomType"></param>
    public void PlaceTileGameObject(int x, int y, Board.ROOM_TYPE roomType)
    {
        Board.Tile tile = board.tiles[x, y];

        if(board.tiles[x, y].gObject != null)
            GameObject.Destroy( board.tiles[x, y].gObject );

        GameObject g = CreateTileGameObject(roomType);

        g.SetActive( roomType != Board.ROOM_TYPE.EMPTY );
        g.transform.parent = boardContainer;
        g.transform.localPosition = new Vector3(x, Constants.boardHeight, y);

        board.tiles[x, y].gObject = g;
    }

    /// <summary>
    /// Given a board region, repopulates prefabs for the walls
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="w">The width.</param>
    /// <param name="h">The height.</param>
    public void updateWallsGameObjects()
    {
        for (int i = 1; i < board.boardWidth - 1; ++i)
            for (int j = 1; j < board.boardHeight - 1; ++j)
            {
                Board.Tile top_tile = board.tiles[i - 1, j];
                Board.Tile left_tile = board.tiles[i, j - 1];
                Board.Tile tile = board.tiles[ i, j ];

                /*
                if (board.tiles[x, y].gObject != null)
                    GameObject.Destroy(board.tiles[x, y].gObject);

                GameObject g = CreateTileGameObject(roomType);

                g.SetActive(roomType != Board.ROOM_TYPE.EMPTY);
                g.transform.parent = boardContainer;
                g.transform.localPosition = new Vector3(x, Constants.boardHeight, y);

                board.tiles[x, y].gObject = g;
                */               
            }
    }


    /// <summary>
    /// Create the GameObject Visual representation to be used by other methods (board or playblocks)
    /// </summary>
    /// <param name="roomType"></param>
    /// <returns></returns>
    public GameObject CreateTileGameObject(Board.ROOM_TYPE roomType)
    {
        GameObject g = GameObject.Instantiate( tileDatabase.prefabTileFloor, new Vector3(0, Constants.boardHeight, 0), Quaternion.identity);
        g.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        g.GetComponent<MeshRenderer>().material = tileDatabase.tileMaterials[roomType];
        return g;
    }

    #endregion
}
