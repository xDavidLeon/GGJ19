using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Board board;
    public float boardHeight = 0.5f;

    public GameObject prefabTileFloor;

    public Material[] matTileFloor;

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
                SetTileGameObject(i, j, tile.data.state);
            }
    }

    public void PlaceBlock(Block block, int startX, int startY)
    {
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                if (block.blockBoard[j*4 + i] != 0)
                    PlaceTile(startX + i, startY + j, Board.TILE_STATE.KITCHEN);
            }
    }

    public void PlaceTile(int x, int y, Board.TILE_STATE tileState)
    {
        board.SetTileState(x, y, tileState);
        SetTileGameObject(x, y, tileState);
    }

    public void SetTileGameObject(int x, int y, Board.TILE_STATE tileState)
    {
        Board.Tile tile = board.tiles[x, y];

        if(board.tiles[x, y].gObject != null)
            GameObject.Destroy(board.tiles[x, y].gObject);

        GameObject g = GameObject.Instantiate(prefabTileFloor, new Vector3(x, boardHeight, y), Quaternion.identity);
        g.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        g.GetComponent<MeshRenderer>().material = matTileFloor[(int)tile.data.state];
        board.tiles[x, y].gObject = g;
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
