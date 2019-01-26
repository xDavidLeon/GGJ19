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

        InitBoardAssets();
    }

    void Update()
    {

    }

    public void InitBoardAssets()
    {
        if(board == null || board.initialized == false) return;
        for(int i = 0; i < board.width; i++)
            for(int j = 0; j < board.height; j++)
            {
                Board.Tile tile = board.tiles[i, j];
                SetTileGameObject(i, j, tile.data.roomType);
            }
    }

    public bool PlacePlayBlock(PlayBlock playBlock)
    {
        int startX = (int) playBlock.transform.position.x;
        int startY = (int) playBlock.transform.position.z;
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                if (playBlock.block.blockBoard[j*4 + i] != 0)
                    PlaceTile(startX + i, startY + j, playBlock.roomType);
            }
        return true;
    }

    public void PlaceTile(int x, int y, Board.ROOM_TYPE tileState)
    {
        board.SetTileState(x, y, tileState);
        SetTileGameObject(x, y, tileState);
    }

    public void SetTileGameObject(int x, int y, Board.ROOM_TYPE roomType)
    {
        Board.Tile tile = board.tiles[x, y];

        if(board.tiles[x, y].gObject != null)
            GameObject.Destroy(board.tiles[x, y].gObject);

        GameObject g = CreateTileGameObject(roomType);

        g.transform.parent = boardContainer;
        g.transform.localPosition = new Vector3(x, Constants.boardHeight, y);

        board.tiles[x, y].gObject = g;
    }

    public GameObject CreateTileGameObject(Board.ROOM_TYPE roomType)
    {
        GameObject g = GameObject.Instantiate(tileDatabase.prefabTileFloor, new Vector3(0, Constants.boardHeight, 0), Quaternion.identity);
        g.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        g.GetComponent<MeshRenderer>().material = tileDatabase.tileMaterials[roomType];
        return g;
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
