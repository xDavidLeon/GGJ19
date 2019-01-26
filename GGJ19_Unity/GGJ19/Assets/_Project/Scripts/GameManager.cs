using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Board board;
    public TileDatabase tileDatabase;
    public BlockDatabase blockDatabase;

    public Transform boardContainer;

    void Start()
    {
        board.InitBoard();

        UpdateBoardTileAssets();
    }

    void Update()
    {

    }

    /// <summary>
    /// Called by the PlayerController to place all the tiles from a new block.
    /// </summary>
    /// <param name="playBlock"></param>
    /// <returns></returns>
    public bool PlacePlayBlock(PlayBlock playBlock)
    {
        int startX = (int) playBlock.transform.position.x;
        int startY = (int) playBlock.transform.position.z;
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                // @TODO Check that the tile doesn's access out of bounds

                if (playBlock.block.blockBoard[j*4 + i] != 0)
                    PlaceTile(startX + i, startY + j, playBlock.roomType);
            }
        return true;
    }

    /// <summary>
    /// Places a Tile by setting the board logical state, and placing the visual representation.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tileState"></param>
    public void PlaceTile(int x, int y, Board.ROOM_TYPE tileState)
    {
        board.SetTileState(x, y, tileState);
        PlaceTileGameObject(x, y, tileState);
    }

    #region VISUALS

    /// <summary>
    /// Updates the full board assets (GameObjects) by reading the tiles roomType
    /// </summary>
    public void UpdateBoardTileAssets()
    {
        if(board == null || board.initialized == false) return;
        for(int i = 0; i < board.width; i++)
            for(int j = 0; j < board.height; j++)
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
            GameObject.Destroy(board.tiles[x, y].gObject);

        GameObject g = CreateTileGameObject(roomType);

        g.transform.parent = boardContainer;
        g.transform.localPosition = new Vector3(x, Constants.boardHeight, y);

        board.tiles[x, y].gObject = g;
    }

    /// <summary>
    /// Create the GameObject Visual representation to be used by other methods (board or playblocks)
    /// </summary>
    /// <param name="roomType"></param>
    /// <returns></returns>
    public GameObject CreateTileGameObject(Board.ROOM_TYPE roomType)
    {
        GameObject g = GameObject.Instantiate(tileDatabase.prefabTileFloor, new Vector3(0, Constants.boardHeight, 0), Quaternion.identity);
        g.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        g.GetComponent<MeshRenderer>().material = tileDatabase.tileMaterials[roomType];
        return g;
    }

    #endregion
}
