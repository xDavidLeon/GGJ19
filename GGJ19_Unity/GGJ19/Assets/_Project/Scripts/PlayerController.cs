using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Camera cam = null;

    public Block selectedBlock;
    public PlayBlock playBlock;

    public float offsetX = -2.5f;
    public float offsetY = -2.5f;

    Plane plane;

    // Start is called before the first frame update
    void Start()
    {
        playBlock.SetData(selectedBlock, Board.ROOM_TYPE.KITCHEN, 0);
        playBlock.Populate();

        plane = new Plane(Vector3.up, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!cam || !playBlock)
            return;

        Ray ray = cam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        float enter = 0.0f;

        if (plane.Raycast(ray, out enter))
        {
            //Get the point that is clicked
            Vector3 hitPoint = ray.GetPoint(enter);

            int board_width = 20;
            int board_height = 20;

            hitPoint.x = Mathf.Clamp( Mathf.Round(hitPoint.x - 0.5f), 0, board_width-1) + 0.5f + offsetX;
            hitPoint.z = Mathf.Clamp( Mathf.Round(hitPoint.z - 0.5f), 0, board_height-1) + 0.5f + offsetY;
            hitPoint.y = Constants.boardPlayblockHeight;

            //Move your cube GameObject to the point where you clicked
            playBlock.transform.position = hitPoint;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.Instance.PlacePlayBlock(playBlock))
            {
                // Get new block
                playBlock.SetData(GameManager.Instance.blockDatabase.GetRandomBlock(), Board.GetRandomRoomType(), 0);
                playBlock.Populate();
            }
            
        }
    }

}
