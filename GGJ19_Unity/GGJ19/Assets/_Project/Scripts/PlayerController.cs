using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Block Selection")]
    public Block selectedBlock;

    [Header("Input")]
    public float joySpeed = 15.0f;
    public float offsetX = -2.5f;
    public float offsetY = -2.5f;

    private Plane plane;

    private Camera cam;
    private RaycastHit hit;

    private bool usingMouse = false;

    private void Awake()
    {
        cam = Camera.main;
        plane = new Plane(Vector3.up, 0);
    }

    void Start()
    {

    }

    void Update()
    {
        if (!cam)
            return;

        for(int i = 0; i < GameManager.Instance.numPlayers; i++)
            UpdatePlayerInput(GameManager.Instance.players[i] );
    }

    public void UpdatePlayerInput(GameManager.PlayerData pData)
    {
        pData.placementX = pData.posX;
        pData.placementZ = pData.posZ;

        int pIndex = GameManager.Instance.players.IndexOf(pData);
        bool myTurn = pIndex == GameManager.Instance.currentPlayer; 
        if (pIndex == 0)
        {
            float mouseMag = Mathf.Abs(Input.GetAxis("Mouse X")) + Mathf.Abs(Input.GetAxis("Mouse Y"));
            Ray ray = cam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            if(Physics.Raycast(ray, out hit, 100))
            {
                Vector3 hitPoint = hit.point;
                Debug.DrawRay(ray.origin, hitPoint - ray.origin, Color.green);

                pData.posX = Mathf.Clamp(Mathf.Round(hitPoint.x - 0.5f), 0.0f, GameManager.Instance.boardWidth - 1) + 0.5f + offsetX;
                pData.posZ = Mathf.Clamp(Mathf.Round(hitPoint.z - 0.5f), 0.0f, GameManager.Instance.boardHeight - 1) + 0.5f + offsetY;

                pData.placementX = pData.posX;
                pData.placementZ = pData.posZ;
            }
            else
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        }
        else
        {
            float joyH = Input.GetAxis(pData.horizontal);
            float joyV = Input.GetAxis(pData.vertical);
            pData.posX += joyH * Time.deltaTime * joySpeed;
            pData.posZ += joyV * Time.deltaTime * joySpeed;

            pData.placementX = Mathf.Clamp(Mathf.Round(pData.posX - 0.5f), 0.0f, GameManager.Instance.boardWidth - 1) + 0.5f + offsetX;
            pData.placementZ = Mathf.Clamp(Mathf.Round(pData.posZ - 0.5f), 0.0f, GameManager.Instance.boardHeight - 1) + 0.5f + offsetY;
        }

        pData.pointer.position = Vector3.Lerp(pData.pointer.position, new Vector3(pData.placementX + 2, Constants.boardPlayblockHeight + 0.5f, pData.placementZ + 2), Time.deltaTime * 4);

        // Move the playBlock to the target position
        if(myTurn)
        {
            pData.playBlock.transform.position = new Vector3(pData.placementX, Constants.boardPlayblockHeight, pData.placementZ);

            if(Input.GetMouseButtonDown(0) || Input.GetButtonDown(pData.accept))
            {
                Vector3 placementPosition = new Vector3(pData.placementX, Constants.boardPlayblockHeight, pData.placementZ);
                pData.playBlock.transform.position = placementPosition;

                Debug.DrawLine(placementPosition, placementPosition + Vector3.up, Color.blue);

                if(GameManager.Instance.PlacePlayBlock(pData.playBlock))
                {
                    // Get new block
                    pData.playBlock.SetData(GameManager.Instance.blockDatabase.GetRandomBlock(), Board.GetRandomRoomType(), GameManager.Instance.currentPlayer);
                    pData.playBlock.Populate();

                    GameManager.Instance.onEndPlayerTurn();
                }
            }
            else if(Input.GetMouseButtonDown(1) || Input.GetButtonDown(pData.rotate))
            {
                pData.playBlock.Rotate(1);
            }
        }
        
    }
}
