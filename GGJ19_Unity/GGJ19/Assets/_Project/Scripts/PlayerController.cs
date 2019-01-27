using DG.Tweening;
using Rewired;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int playerId = -1;
    public bool initialized = false;
    public Color playerColor = Color.white;

    [Header("UI")]
    public int score = 0;
    public TMPro.TextMeshProUGUI textScore;
    public CanvasGroup uiCanvasGroup;
    public UnityEngine.UI.Image imgPrompt;

    [Header("Input")]
    public float moveSpeed = 15.0f;
    public float offsetX = -2.5f;
    public float offsetY = -2.5f;
    public LayerMask raycastLayer;

    private float posX = 0;
    private float posZ = 0;
    private float placementX = 0;
    private float placementZ = 0;
    public Rewired.Player playerInput;

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
    }

    void Start()
    {
        imgPrompt.rectTransform.DOScale(2.0f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    void Update()
    {
        if(!cam)
            return;

        if(GameManager.Instance.gameState == GameManager.GAME_STATE.PLAYER_SELECTION && initialized == false)
        {
            imgPrompt.gameObject.SetActive(true);
        }
        else
            imgPrompt.gameObject.SetActive(false);

        //if(GameManager.Instance.gamePlayerIdCounter <= playerId) return;
        if(initialized == false || playerInput == null)
        {
            DisablePlayer();
            return;
        }

        //if(GameManager.Instance.gameState != GameManager.GAME_STATE.GAME)
        //{
        //    playBlock.gameObject.SetActive(false);
        //}
        //else
        //    playBlock.gameObject.SetActive(true);

        UpdatePlayerInput();

        textScore.text = score.ToString();
        if(MyTurn) uiCanvasGroup.alpha = 1.0f;
        else uiCanvasGroup.alpha = 0.5f;
    }

    void DisablePlayer()
    {
        playerId = -1;
        uiCanvasGroup.alpha = 0.0f;
        playBlock.gameObject.SetActive(false);
    }

    public void Init(int id, Rewired.Player rewiredPlayer)
    {
        playerId = id;
        playerInput = rewiredPlayer;
        initialized = true;
        playBlock.gameObject.SetActive(true);
        posX = 10;
        posZ = 10;

        NewBlock();
    }

    public void UpdatePlayerInput()
    {

        placementX = posX;
        placementZ = posZ;

        Controller controller = playerInput.controllers.GetLastActiveController();
        if(controller == null)
            return;
        if(controller.type == ControllerType.Mouse)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            if(Physics.Raycast(ray, out hit, 100, raycastLayer))
            {
                Vector3 hitPoint = hit.point;
                Debug.DrawRay(ray.origin, hitPoint - ray.origin, Color.green);

                posX = Mathf.Clamp(Mathf.Round(hitPoint.x - 0.5f), 0.0f, GameManager.Instance.boardWidth - 1) + 0.5f;
                posZ = Mathf.Clamp(Mathf.Round(hitPoint.z - 0.5f), 0.0f, GameManager.Instance.boardHeight - 1) + 0.5f;

                placementX = posX + offsetX;
                placementZ = posZ + offsetY;
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
        Vector3 placement = new Vector3(placementX, GameManager.Instance.tileDatabase.boardHeight, placementZ);
        Debug.DrawLine(placement, placement + Vector3.up * 10.0f, Color.blue);

        if(GameManager.Instance.gameState != GameManager.GAME_STATE.GAME) return;

        // Move the playBlock to the target position
        if(MyTurn)
        {
            if(playerInput.GetButtonDown("Select") || Input.GetKeyDown(KeyCode.Y))
            {
                if(GameManager.Instance.PlacePlayBlock(playBlock, Input.GetKeyDown(KeyCode.Y)))
                    GameManager.Instance.NextTurn();
            }
            else if(playerInput.GetButtonDown("Skip"))
            {
                GameManager.Instance.NextTurn(true);
            }
        }

    }

    public void NewBlock()
    {
        Board.ROOM_TYPE roomType = Board.GetRandomRoomType();

        if(GameManager.Instance.mode == GameManager.GAME_MODE.CONQUEST)
            roomType = (Board.ROOM_TYPE)(playerId + (int)Board.ROOM_TYPE.KITCHEN);
        else if(GameManager.Instance.mode == GameManager.GAME_MODE.HOME && GameManager.Instance.turn == 0)
            roomType = Board.ROOM_TYPE.CORRIDOR;
        playBlock.SetData(GameManager.Instance.blockDatabase.GetRandomBlock(), roomType, GameManager.Instance.currentPlayerId);
        playBlock.Populate();
    }

}
