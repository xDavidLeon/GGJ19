using UnityEngine;

[CreateAssetMenu(fileName = "Block", menuName = "GGJ19/Create Block", order = 1)]
public class Block : ScriptableObject
{
    public int[] blockBoard = new int[4 * 4];

    public void SetValue( int x, int y, int v )
    {
        blockBoard[y * 4 + x] = v;
    }

    public int GetValue(int x, int y)
    {
        return blockBoard[y * 4 + x];
    }

    public void CopyFrom( Block b )
    {
        for (int i = 0; i < 16; i++)
            blockBoard[i] = b.blockBoard[i];
    }

    public void Rotate(int dir)
    {
        dir = dir % 4;
        if (dir == 0)
            return;
        if (dir < 0)
            dir = 4 + dir;

        Block temp = new Block();
        temp.CopyFrom(this);

        //rotate
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
            {
                int v = 0;
                if (dir == 1)
                    v = temp.GetValue( 3 - j, i );
                else if (dir == 2)
                    v = temp.GetValue( 3-i , 3-j );
                else if (dir == 3)
                    v = temp.GetValue( j, 3 - i );
                SetValue(i, j, v);
            }
    }
}


