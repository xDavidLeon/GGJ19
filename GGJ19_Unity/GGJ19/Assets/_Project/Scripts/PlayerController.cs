using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int playerId;

    [Header("UI")]
    public int score = 0;
    public TMPro.TextMeshProUGUI textScore;
    public CanvasGroup uiCanvasGroup;

    [Header("Input")]
    public float moveSpeed = 15.0f;
    public float offsetX = -2.5f;
    public float offsetY = -2.5f;

    private float posX = 0;
    private float posZ = 0;
    private float placementX = 0;
    private float placementZ = 0;
    private Rewired.Player playerInput;

    public int startX = -1;
    public int startY = -1;

    [Header("Block Management")]
    public Transform pointer;
    public PlayBlock playBlock;

    private Camera cam;
    private RaycastHit hit;

    public bool MyTurn
    {
        get
        {
            return playerId == GameManager.Instance.currentPlayerId;
        }
    }

    private void Awake()
    {
        cam = Camera.main;
        playerId = GameManager.Instance.players.IndexOf(this);
    }

    void Start()
    {
        playerInput = ReInput.players.GetPlayer(playerId);

        NewBlock();

        /*
        playBlock.SetData(GameManager.Instance.blockDatabase.GetRandomBlock(), Board.ROOM_TYPE.CORRIDOR, playerId);
        playBlock.Populate();
        */
    }

    void Update()
    {
        if (!cam)
            return;

        UpdatePlayerInput();

        textScore.text = score.ToString();
        if(MyTurn) uiCanvasGroup.alpha = 1.0f;
        else uiCanvasGroup.alpha = 0.5f;

    }

    public void UpdatePlayerInput()
    {
        placementX = posX;
        placementZ = posZ;

        Controller controller = playerInput.controllers.GetLastActiveController();
        if(controller == null) return;
        if (controller.type == ControllerType.Mouse)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            if(Physics.Raycast(ray, out hit, 100))
            {
                Vector3 hitPoint = hit.point;
                Debug.DrawRay(ray.origin, hitPoint - ray.origin, Color.green);

                posX = Mathf.Clamp(Mathf.Round(hitPoint.x - 0.5f), 0.0f, GameManager.Instance.boardWidth - 1) + 0.5f + offsetX;
                posZ = Mathf.Clamp(Mathf.Round(hitPoint.z - 0.5f), 0.0f, GameManager.Instance.boardHeight - 1) + 0.5f + offsetY;

                placementX = posX;
                placementZ = posZ;
            }
            else
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        }
        else
        {
            float joyH = playerInput.GetAxis("Horizontal");
            float joyV = playerInput.GetAxis("Vertical");
            posX += joyH * Time.deltaTime * moveSpeed;
            posZ += joyV * Time.deltaTime * moveSpeed;

            placementX = Mathf.Clamp(Mathf.Round(posX - 0.5f), 0.0f, GameManager.Instance.boardWidth - 1) + 0.5f + offsetX;
            placementZ = Mathf.Clamp(Mathf.Round(posZ - 0.5f), 0.0f, GameManager.Instance.boardHeight - 1) + 0.5f + offsetY;
        }

        if(playerInput.GetButtonDown("Rotate"))
            playBlock.Rotate(1);

        pointer.position = Vector3.Lerp(pointer.position, new Vector3(placementX + 2, GameManager.Instance.tileDatabase.boardPlayblockHeight + 0.5f, placementZ + 2), Time.deltaTime * 4);
        playBlock.transform.position = new Vector3(placementX, GameManager.Instance.tileDatabase.boardPlayblockHeight, placementZ);

        // Move the playBlock to the target position
        if(MyTurn)
        {
            if(playerInput.GetButtonDown("Select"))
            {
                Vector3 placementPosition = new Vector3(placementX, GameManager.Instance.tileDatabase.boardPlayblockHeight, placementZ);
                playBlock.transform.position = placementPosition;

                Debug.DrawLine(placementPosition, placementPosition + Vector3.up, Color.blue);

                if(GameManager.Instance.PlacePlayBlock(playBlock)) GameManager.Instance.NextTurn();
            }
        }

    }

    public void NewBlock()
    {
        Board.ROOM_TYPE roomType = Board.GetRandomRoomType();

        if (GameManager.Instance.mode == GameManager.GAME_MODE.CONQUEST)
            roomType = (Board.ROOM_TYPE)(playerId + (int)Board.ROOM_TYPE.KITCHEN);
        else if (GameManager.Instance.mode == GameManager.GAME_MODE.HOME && GameManager.Instance.turn == 0)
            roomType = Board.ROOM_TYPE.CORRIDOR;
        playBlock.SetData(GameManager.Instance.blockDatabase.GetRandomBlock(), roomType, GameManager.Instance.currentPlayerId);
        playBlock.Populate();
    }
}
