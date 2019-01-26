using UnityEngine;

public class PlayBlock : MonoBehaviour
{
    public TileDatabase tileDatabase;

    public Block block;
    public Board.ROOM_TYPE roomType;
    public int player = 0;

    public void SetData(Block b, Board.ROOM_TYPE s, int p)
    {
        block = b;
        roomType = s;
        player = p;
    }

    [ContextMenu("Populate")]
    public void Populate()
    {
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                if(block.blockBoard[j * 4 + i] != 0)
                {
                    GameObject g = CreateTileGameObject(block, roomType);
                    g.transform.SetParent(this.transform);
                    g.transform.localPosition = new Vector3(i, 0.0f, j);
                }
            }
    }

    public GameObject CreateTileGameObject(Block block, Board.ROOM_TYPE tileState)
    {
        GameObject g = GameObject.Instantiate(tileDatabase.prefabTileFloor, new Vector3(0, Constants.boardHeight, 0), Quaternion.identity);
        g.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        g.GetComponent<MeshRenderer>().material = tileDatabase.tileMaterials[tileState];
        return g;
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        foreach(Transform child in this.transform) GameObject.Destroy(child.gameObject);
    }
}
