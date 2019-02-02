using UnityEngine;

public class PlayBlock : MonoBehaviour
{
    public TileDatabase tileDatabase;

    public Block blockBlueprint;
    public Block block;

    public Board.ROOM_TYPE roomType;
    public int player = 0;

    void Awake()
    {
        block = ScriptableObject.CreateInstance<Block>();
    }

    public void SetData(Block b, Board.ROOM_TYPE s, int p)
    {
        blockBlueprint = b;
        for (int i = 0; i < 16; ++i)
            block.blockBoard[i] = blockBlueprint.blockBoard[i];
        roomType = s;
        player = p;
    }

    [ContextMenu("Populate")]
    public void Populate(int playerId)
    {
        Clear();
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                if (block.GetValue(i,j) == 0)
                    continue;
                GameObject g = GameManager.Instance.CreateTileGameObject( roomType, true, playerId );
                g.transform.SetParent(this.transform);
                g.transform.localPosition = new Vector3(i, 0.0f, j);
            }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        //clear tiles
        foreach(Transform child in this.transform) GameObject.Destroy(child.gameObject);
    }

    public void Rotate(int dir)
    {
        block.Rotate( dir ); //rotate internal block
        //repopulate
        Populate(player);
    }
}
