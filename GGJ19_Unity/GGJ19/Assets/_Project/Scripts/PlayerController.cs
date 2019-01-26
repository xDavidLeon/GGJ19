using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInput
    {
        public string horizontal, vertical, accept, rotate;
        public float posX = 0;
        public float posZ = 0;
        public float placementX = 0;
        public float placementZ = 0;
        public Transform pointer;
    };

    [Header("Block Selection")]
    public Block selectedBlock;
    public PlayBlock playBlock;

    [Header("Input")]
    public float joySpeed = 15.0f;
    public float offsetX = -2.5f;
    public float offsetY = -2.5f;
    public List<PlayerInput> playerInputs;

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
        playBlock.SetData(GameManager.Instance.blockDatabase.GetRandomBlock(), Board.GetRandomRoomType(), 0);
        playBlock.Populate();
    }

    void Update()
    {
        if (!cam || !playBlock)
            return;

        for(int i = 0; i < GameManager.Instance.numPlayers; i++)
            UpdatePlayerInput(playerInputs[i]);
    }

    public void UpdatePlayerInput(PlayerInput pInput)
    {
        pInput.placementX = pInput.posX;
        pInput.placementZ = pInput.posZ;

        int pIndex = playerInputs.IndexOf(pInput);
        bool myTurn = pIndex == GameManager.Instance.currentPlayer; 
        //if (pIndex == 0)
        //{
        //    float mouseMag = Mathf.Abs(Input.GetAxis("Mouse X")) + Mathf.Abs(Input.GetAxis("Mouse Y"));
        //    Ray ray = cam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        //    if(Physics.Raycast(ray, out hit, 100))
        //    {
        //        Vector3 hitPoint = hit.point;
        //        Debug.DrawRay(ray.origin, hitPoint - ray.origin, Color.green);

        //        pInput.posX = Mathf.Clamp(Mathf.Round(hitPoint.x - 0.5f), 0.0f, GameManager.Instance.boardWidth - 1) + 0.5f + offsetX;
        //        pInput.posZ = Mathf.Clamp(Mathf.Round(hitPoint.z - 0.5f), 0.0f, GameManager.Instance.boardHeight - 1) + 0.5f + offsetY;

        //        pInput.placementX = pInput.posX;
        //        pInput.placementZ = pInput.posZ;
        //    }
        //    else
        //        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        //}
        
        {
            float joyH = Input.GetAxis(pInput.horizontal);
            float joyV = Input.GetAxis(pInput.vertical);
            pInput.posX += joyH * Time.deltaTime * joySpeed;
            pInput.posZ += joyV * Time.deltaTime * joySpeed;

            pInput.placementX = Mathf.Clamp(Mathf.Round(pInput.posX - 0.5f), 0.0f, GameManager.Instance.boardWidth - 1) + 0.5f + offsetX;
            pInput.placementZ = Mathf.Clamp(Mathf.Round(pInput.posZ - 0.5f), 0.0f, GameManager.Instance.boardHeight - 1) + 0.5f + offsetY;
        }

        pInput.pointer.position = Vector3.Lerp(pInput.pointer.position, new Vector3(pInput.placementX + 2, Constants.boardPlayblockHeight + 0.5f, pInput.placementZ + 2), Time.deltaTime * 4);

        // Move the playBlock to the target position
        if(myTurn)
        {
            playBlock.transform.position = new Vector3(pInput.placementX, Constants.boardPlayblockHeight, pInput.placementZ);

            if(Input.GetMouseButtonDown(0) || Input.GetButtonDown(pInput.accept))
            {
                Vector3 placementPosition = new Vector3(pInput.placementX, Constants.boardPlayblockHeight, pInput.placementZ);
                playBlock.transform.position = placementPosition;

                Debug.DrawLine(placementPosition, placementPosition + Vector3.up, Color.blue);

                if(GameManager.Instance.PlacePlayBlock(playBlock))
                {
                    // Get new block
                    playBlock.SetData(GameManager.Instance.blockDatabase.GetRandomBlock(), Board.GetRandomRoomType(), GameManager.Instance.currentPlayer);
                    playBlock.Populate();

                    // TODO if it was a valid placement, check if game is over. If not, give control to next player
                    GameManager.Instance.NextPlayer();
                }
            }
            else if(Input.GetMouseButtonDown(1) || Input.GetButtonDown(pInput.rotate))
            {
                playBlock.Rotate(1);
            }
        }
        
    }
}
