using Rewired;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public enum GAME_MODE
    {
        CONQUEST = 1,
        HOME = 2
    };

    public enum GAME_STATE
    {
        INTRO,
        PLAYER_SELECTION,
        GAME,
        GAME_OVER,
        TUTORIAL
    }

    [Header("Game Settings")]
    public GAME_MODE mode = GAME_MODE.CONQUEST;
    public GAME_STATE gameState = GAME_STATE.PLAYER_SELECTION;
    public Board board;
    public TileDatabase tileDatabase;
    public BlockDatabase blockDatabase;
    public int boardWidth = 20;
    public int boardHeight = 20;
    public Transform boardContainer;
    public float selectionCountdownTime = 5.0f;
    public float turnMaxTime = 10.0f;
    public float introDuration = 5.0f;
    public float gameOverDuration = 5.0f;
    private float lastTimePlayerAdded = 0.0f;
    private float lastTimeTurnStarted = 0.0f;
    private float gameOverStartTime = 0.0f;
    private float introStartTime = 0.0f;
    private int skippedTurns = 0;

    [Header("Special Settings")]
    public int level = 3;
    public bool force_corridors = true;
    public bool force_connectivity = true;
    public bool placeBotWalls = false;
    public Transform boardGuide;
    public int turn = 0;
    public int last_block_id = 0;
    private Vector2[] offsets;

    [Header("Player Management")]
    public List<PlayerController> players;
    public int maxPlayers = 4;
    public int activePlayers = 0;
    public int currentPlayerId = 0;

    [Header("UI")]
    public CanvasGroup canvasGroupIntro;
    public CanvasGroup canvasGroupTitle;
    public CanvasGroup canvasGroupPlayerSelection;
    public CanvasGroup canvasGroupGameOver;
    public TMPro.TextMeshProUGUI txtPlayerSelectionCooldown;
    public CanvasGroup canvasGroupGame;
    public TMPro.TextMeshProUGUI txtPlayerTimerTitle;
    public UnityEngine.UI.Image txtPlayerTimerImage;
    public UnityEngine.UI.Image imgIntro;
    public UnityEngine.UI.Image imgTutorial;

    [Header("Audio")]
    public AudioClip clipFall;

    public PlayerController CurrentPlayer
    {
        get
        {
            return players[currentPlayerId];
        }
    }

    private void Awake()
    {
        offsets = new Vector2[4];
        offsets[0].Set(-1, 0);
        offsets[1].Set(+1, 0);
        offsets[2].Set(0, -1);
        offsets[3].Set(0, +1);
        activePlayers = 0;

        canvasGroupPlayerSelection.alpha = 0.0f;
        canvasGroupGame.alpha = 0.0f;
        canvasGroupIntro.alpha = 1.0f;
        if (canvasGroupGameOver != null) canvasGroupGameOver.alpha = 0.0f;
    }

    void Start()
    {
        SetGameState(GAME_STATE.INTRO);
        SceneManager.LoadScene("ArtScene", LoadSceneMode.Additive);
    }

    public void InitGame()
    {
        canvasGroupPlayerSelection.DOFade(0.0f, 1.0f);
        canvasGroupGame.DOFade(1.0f, 1.0f);

        last_block_id = 0;
        currentPlayerId = 0;
        lastTimeTurnStarted = Time.time;
        skippedTurns = 0;

        board.InitBoard(boardWidth, boardHeight);
        boardGuide.transform.localScale = new Vector3(boardWidth, 1.0f, boardHeight);
        boardGuide.transform.localPosition = new Vector3(boardWidth / 2.0f, -0.475f, boardHeight / 2.0f);
        for(int i = 0; i < activePlayers; ++i)
        {
            PlayerController player = players[i];
            player.Reset();
        }

        for(int i = 0; i < activePlayers; ++i)
            SetPlayerStartTiles(i);

        board.SetLevel(Random.Range(0, 5));

        //obstacles
        UpdateBoardTileAssets();
    }

    public void SetGameState(GAME_STATE state)
    {
        GAME_STATE prevState = state;
        gameState = state;
        switch(state)
        {
            case GAME_STATE.INTRO:
                introStartTime = Time.time;
                imgIntro.DOFade(1.0f, 0.0f);
                imgTutorial.DOFade(0.0f, 0.0f);

                //canvasGroupIntro.DOFade(1.0f, 0.5f);
                break;
            case GAME_STATE.TUTORIAL:
                imgIntro.DOFade(0.0f, 0.5f);
                imgTutorial.DOFade(1.0f, 0.5f);
                //canvasGroupIntro.DOFade(1.0f, 0.5f);
                break;
            case GAME_STATE.PLAYER_SELECTION:
                canvasGroupIntro.DOFade(0.0f, 0.5f);
                canvasGroupPlayerSelection.DOFade(1.0f, 0.5f);
                lastTimePlayerAdded = Time.time;
                break;
            case GAME_STATE.GAME:
                if(canvasGroupGameOver != null) canvasGroupGameOver.alpha = 0.0f;
                if (prevState == GAME_STATE.GAME_OVER)
                    if(canvasGroupGameOver != null) canvasGroupGameOver.DOFade(0.0f, 0.5f);
                InitGame();
                break;
            case GAME_STATE.GAME_OVER:
                if(canvasGroupGameOver != null) canvasGroupGameOver.DOFade(1.0f, 0.5f);
                gameOverStartTime = Time.time;
                break;
        }
    }

    void Update()
    {
        switch(gameState)
        {
            case GAME_STATE.INTRO:
                if(Time.time - introStartTime > introDuration / 2.0f) SetGameState(GAME_STATE.TUTORIAL);
                break;
            case GAME_STATE.TUTORIAL:
                if(Time.time - introStartTime > introDuration) SetGameState(GAME_STATE.PLAYER_SELECTION);
                break;
            case GAME_STATE.PLAYER_SELECTION:
                // Watch for JoinGame action in each Player
                for(int i = 0; i < ReInput.players.playerCount; i++)
                {
                    if(ReInput.players.GetPlayer(i).GetButtonDown("Select"))
                    {
                        AssignNextPlayer(i);
                    }
                }
                int timeSelection = (int)(selectionCountdownTime - (Time.time - lastTimePlayerAdded));
                timeSelection = Mathf.Clamp(timeSelection, 0, (int)selectionCountdownTime);
                if(activePlayers >= 1)
                    txtPlayerSelectionCooldown.text = timeSelection.ToString();
                else
                    txtPlayerSelectionCooldown.text = "";
                if(activePlayers >= maxPlayers || (Time.time - lastTimePlayerAdded > selectionCountdownTime && activePlayers >= 1)) SetGameState(GAME_STATE.GAME);

                break;
            case GAME_STATE.GAME:
                txtPlayerTimerTitle.text = "Player " + (currentPlayerId + 1) + " Turn";
                txtPlayerTimerTitle.color = CurrentPlayer.playerColor;
                txtPlayerTimerImage.color = CurrentPlayer.playerColor;
                float timeTurn = (int)(turnMaxTime - (Time.time - lastTimeTurnStarted));
                timeTurn = Mathf.Clamp(timeTurn, 0.0f, turnMaxTime);
                txtPlayerTimerImage.fillAmount = timeTurn / turnMaxTime;

                if(Time.time - lastTimeTurnStarted > turnMaxTime) NextTurn();

                break;
            case GAME_STATE.GAME_OVER:
                if(Time.time - gameOverStartTime > gameOverDuration) SetGameState(GAME_STATE.GAME);
                break;
        }
    }

    void AssignNextPlayer(int rewiredPlayerId)
    {
        if(activePlayers >= maxPlayers)
        {
            Debug.LogError("Max player limit already reached!");
            return;
        }

        Player rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerId);
        players[activePlayers].Init(rewiredPlayerId, rewiredPlayer);

        // Disable the Assignment map category in Player so no more JoinGame Actions return
        //rewiredPlayer.controllers.maps.SetMapsEnabled(false, "Assignment");

        // Enable UI control for this Player now that he has joined
        //rewiredPlayer.controllers.maps.SetMapsEnabled(true, "UI");
        Debug.Log("Added Rewired Player id " + rewiredPlayerId + " to game player " + activePlayers);
        activePlayers++;

        lastTimePlayerAdded = Time.time;
    }

    void SetPlayerStartTiles(int player_id)
    {
        int start_x = 0;
        int start_y = 0;

        PlayerController pc = players[player_id];

        if(player_id == 0 || player_id == 1)
            start_y = (int)(boardHeight * 0.5);
        if(player_id == 2 || player_id == 3)
            start_x = (int)(boardWidth * 0.5);
        if(player_id == 1)
            start_x = boardWidth - 1;
        if(player_id == 3)
            start_y = boardHeight - 1;

        pc.startX = start_x;
        pc.startY = start_y;

        Board.Tile tile = board.GetTile(start_x, start_y);
        tile.data.player = player_id;
        tile.data.roomType = Board.ROOM_TYPE.START;
    }

    /// <summary>
    /// Check if a PlayBlock could be placed in the board
    /// Called by the PlayerController to place all the tiles from a new block.
    /// </summary>
    /// <param name="playBlock"></param>
    /// <returns></returns>
    public bool CheckPlacePlayBlock(PlayBlock playBlock)
    {
        int startX = (int)playBlock.transform.position.x;
        int startY = (int)playBlock.transform.position.z;

        bool touching_player = false;
        bool is_corridor = playBlock.roomType == Board.ROOM_TYPE.CORRIDOR;

        //check if placeable
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                int x = startX + i;
                int y = startY + j;
                if(x < 0 || x >= board.boardWidth || y < 0 || y >= board.boardHeight)
                    return false;
                int has_cell = playBlock.block.GetValue(i, j);
                if(has_cell != 0 && board.GetTileState(x, y) != Board.ROOM_TYPE.EMPTY)
                {
                    Debug.Log("not empty");
                    return false;
                }
                //check if player id near
                if(!touching_player && has_cell == 1)
                    for(int k = 0; k < 4; ++k)
                    {
                        Vector2 offset = offsets[k];
                        int x2 = x + (int)offset.x;
                        int y2 = y + (int)offset.y;
                        if(x2 < 0 || x2 >= board.boardWidth || y2 < 0 || y2 >= boardHeight)
                            continue; //could happen
                        Board.Tile tile = board.GetTile(x2, y2);
                        if(tile.data.player == currentPlayerId)
                        {
                            //if not touching a corridor
                            if(force_corridors && tile.data.roomType != playBlock.roomType && (
                                (is_corridor && tile.data.roomType == Board.ROOM_TYPE.CORRIDOR) ||
                                (!is_corridor && tile.data.roomType != Board.ROOM_TYPE.CORRIDOR)))
                            {
                                Debug.Log("wrong corridor connection");
                                continue;
                            }

                            if(force_connectivity && !tile.data.connected && tile.data.roomType != Board.ROOM_TYPE.START)
                            {
                                Debug.Log("not connected");
                                continue;
                            }

                            touching_player = true;
                            break;
                        }
                    }

            }

        if(!touching_player) //add tip in GUI about not close to player
        {
            Debug.Log("far from player");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Places a block if it fits
    /// </summary>
    /// <returns><c>true</c>, if play block was placed, <c>false</c> otherwise.</returns>
    /// <param name="playBlock">Play block.</param>
    public bool PlacePlayBlock(PlayBlock playBlock, bool erase = false)
    {
        if(CheckPlacePlayBlock(playBlock) == false && !erase)
            return false;

        int block_id = last_block_id++;
        int startX = (int)playBlock.transform.position.x;
        int startY = (int)playBlock.transform.position.z;
        for(int i = 0; i < 4; i++)
            for(int j = 0; j < 4; j++)
            {
                if(playBlock.block.GetValue(i, j) == 0)
                    continue;

                Board.ROOM_TYPE type = playBlock.roomType;
                if(erase)
                    type = Board.ROOM_TYPE.EMPTY;

                PlaceTile(startX + i, startY + j, type, currentPlayerId, block_id);

                //conquer neightbours
                if(mode == GAME_MODE.CONQUEST && !erase)
                {
                    for(int k = 0; k < 4; ++k)
                    {
                        Vector2 offset = offsets[k];
                        Board.Tile next = board.GetTile(startX + i + (int)offset.x, startY + j + (int)offset.y);
                        if(next == null)
                            continue;
                        if(next.data.roomType != Board.ROOM_TYPE.WALL &&
                            next.data.roomType != Board.ROOM_TYPE.EMPTY &&
                            next.data.roomType != Board.ROOM_TYPE.START)
                        {
                            next.data.player = currentPlayerId;
                            next.data.roomType = type;
                        }
                    }
                }
            }

        GetComponent<AudioSource>().PlayOneShot(clipFall);

        return true;
    }

    /// <summary>
    /// Places a Tile by setting the board logical state, and placing the visual representation.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tileState"></param>
    public void PlaceTile(int x, int y, Board.ROOM_TYPE roomState, int player_id, int block_id)
    {
        Board.Tile tile = board.GetTile(x, y);
        tile.data.roomType = roomState;
        tile.data.player = player_id;
        tile.block_id = block_id;
        PlaceTileGameObject(tile);
    }

    public void NextTurn(bool skip = false)
    {
        //mark tiles not connected
        for(int i = 0; i < activePlayers; ++i)
        {
            PlayerController player = players[i];
            if(mode == GAME_MODE.CONQUEST)
                board.ComputeConnectivity(player.playerId);
            player.score = board.ComputePlayerScore(player.playerId);
        }

        if(skip)
        {
            CurrentPlayer.skippedTurns++;
            if (CurrentPlayer.skippedTurns >= 2)
            {
                CurrentPlayer.isPlaying = false;
            }
        }
        else
        {
            CurrentPlayer.skippedTurns = 0;
        }

        skippedTurns = 0;
        for(int i = 0; i < activePlayers; i++)
        {
            PlayerController pl = players[i];
            if(pl.skippedTurns > 0) skippedTurns++;
        }

        if (skippedTurns >= activePlayers)
        {
            SetGameState(GAME_STATE.GAME_OVER);
            return;
        }

        // Get new block

        UpdateBoardTileAssets();

        NextPlayer();

        CurrentPlayer.NewBlock();

        lastTimeTurnStarted = Time.time;
    }

    void NextPlayer()
    {
        int nextPlayer = (currentPlayerId + 1) % activePlayers;
        while (players[nextPlayer].isPlaying == false)
            nextPlayer = (nextPlayer + 1) % activePlayers;

        currentPlayerId = nextPlayer;
        if(currentPlayerId == 0) turn++;
    }

    #region VISUALS

    /// <summary>
    /// Updates the full board assets (GameObjects) by reading the tiles roomType
    /// </summary>
    public void UpdateBoardTileAssets()
    {
        if(board == null || board.initialized == false) return;

        for(int i = 0; i < boardWidth; i++)
            for(int j = 0; j < boardHeight; j++)
            {
                Board.Tile tile = board.tiles[i, j];
                tile.ClearVisuals();
                //if( tile.data.connected )
                PlaceTileGameObject(tile);
                PlaceWalls(tile);
            }
    }

    private void PlaceWalls(Board.Tile tile)
    {
        PlaceWall(tile, Constants.Direction.Left);
        PlaceWall(tile, Constants.Direction.Right);
        PlaceWall(tile, Constants.Direction.Top);
        PlaceWall(tile, Constants.Direction.Bot);
    }

    /// <summary>
    /// Sets the visual representation of the Tile according to the roomType
    /// Called by InitBoard and PlaceTile.
    /// </summary>
    /// <param name="tile"></param>
    public GameObject PlaceTileGameObject(Board.Tile tile)
    {
        int x = tile.pos_x;
        int y = tile.pos_y;

        if(tile.gFloor != null)
            GameObject.Destroy(tile.gFloor);

        GameObject g = CreateTileGameObject(tile.data.roomType, tile.data.connected);
        g.name = "Tile_X" + x + "Y" + y;
        g.SetActive(tile.data.roomType != Board.ROOM_TYPE.EMPTY);
        g.transform.parent = boardContainer;
        g.transform.localPosition = new Vector3(x + 0.5f, tileDatabase.boardHeight, y + 0.5f);

        tile.gFloor = g;

        return g;
    }

    public GameObject PlaceWall(Board.Tile tile, Constants.Direction direction)
    {
        if(tile.data.roomType == Board.ROOM_TYPE.EMPTY) return null;
        if(tile.data.roomType == Board.ROOM_TYPE.WALL) return null;
        if(tile.data.roomType == Board.ROOM_TYPE.START) return null;

        // Out of bounds checks
        //if(direction == Constants.Direction.Left && tile.pos_x <= 0) return null;
        //else if(direction == Constants.Direction.Right && tile.pos_x >= boardWidth - 1) return null;
        //else if(direction == Constants.Direction.Bot && tile.pos_y <= 0.0f) return null;
        //else if(direction == Constants.Direction.Top && tile.pos_y >= boardHeight - 1) return null;

        // Where will we store the gameObject?
        GameObject targetGO = null;
        Board.Tile targetTile = null;
        int x = tile.pos_x;
        int y = tile.pos_y;

        if(direction == Constants.Direction.Bot)
        {
            if(y > 0)
                targetTile = board.GetTile(x, y - 1);
            targetGO = tile.gWallBot;
        }
        else if(direction == Constants.Direction.Top)
        {
            if(y < boardHeight - 1)
                targetTile = board.GetTile(x, y + 1);
            targetGO = tile.gWallTop;
        }
        else if(direction == Constants.Direction.Left)
        {
            if(x > 0)
                targetTile = board.GetTile(x - 1, y);
            targetGO = tile.gWallLeft;
        }
        else if(direction == Constants.Direction.Right)
        {
            if(x < boardWidth - 1)
                targetTile = board.GetTile(x + 1, y);
            targetGO = tile.gWallRight;
        }

        if(targetGO != null)
            GameObject.Destroy(targetGO);

        GameObject gPrefab = tileDatabase.prefabWall;
        bool isTargetRoom = targetTile != null && targetTile.data.roomType != Board.ROOM_TYPE.WALL && targetTile.data.roomType != Board.ROOM_TYPE.EMPTY;
        if(!isTargetRoom)
        {
            PlaceWallProp(tile, direction, tileDatabase.RandomWallProp(tile.data.roomType), true);
            if(placeBotWalls == false && direction == Constants.Direction.Bot)
            {
                return null;
            }
            return PlaceWallProp(tile, direction, tileDatabase.prefabWall);
        }
        else if(targetTile.data.player != tile.data.player) return PlaceWallProp(tile, direction, tileDatabase.prefabWall);
        else if(targetTile.data.roomType != tile.data.roomType) return PlaceWallProp(tile, direction, tileDatabase.prefabDoor);


        return null;
    }

    private GameObject PlaceWallProp(Board.Tile tile, Constants.Direction direction, GameObject gPrefab, bool isProp = false)
    {
        if(gPrefab == null) return null;

        GameObject g = CreateWallGameObject(tile.data.roomType, gPrefab);
        g.transform.parent = boardContainer;
        if(isProp)
        {
            GameObject.Destroy(tile.gProp);
            tile.gProp = g;
        }

        if(isProp && !tile.data.connected)
            return null;

        int x = tile.pos_x;
        int y = tile.pos_y;

        if(direction == Constants.Direction.Bot)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallBot-" + gPrefab.name;
            g.transform.localPosition = new Vector3(x + 0.5f, tileDatabase.boardPropHeight, y + 0.5f);
            if(!isProp) tile.gWallBot = g;
        }
        else if(direction == Constants.Direction.Top)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallTop-" + gPrefab.name;
            g.transform.localRotation = Quaternion.Euler(0, 180, 0);
            g.transform.localPosition = new Vector3(x + 0.5f, tileDatabase.boardPropHeight, y + 0.5f);
            if(!isProp) tile.gWallTop = g;
        }
        else if(direction == Constants.Direction.Left)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallLeft-" + gPrefab.name;
            g.transform.localRotation = Quaternion.Euler(0, 90, 0);
            g.transform.localPosition = new Vector3(x + 0.5f, tileDatabase.boardPropHeight, y + 0.5f);
            if(!isProp) tile.gWallLeft = g;
        }
        else if(direction == Constants.Direction.Right)
        {
            g.name = "Tile_X" + x + "Y" + y + "WallRight-" + gPrefab.name;
            g.transform.localRotation = Quaternion.Euler(0, -90, 0);
            g.transform.localPosition = new Vector3(x + 0.5f, tileDatabase.boardPropHeight, y + 0.5f);
            if(!isProp) tile.gWallRight = g;
        }
        g.transform.parent = boardContainer;
        return g;
    }

    /// <summary>
    /// Create the GameObject Visual representation to be used by other methods (board or playblocks)
    /// </summary>
    /// <param name="roomType"></param>
    /// <returns></returns>
    public GameObject CreateTileGameObject(Board.ROOM_TYPE room_type, bool connected = true)
    {
        GameObject g = GameObject.Instantiate(tileDatabase.prefabTileFloor, new Vector3(0, tileDatabase.boardHeight, 0), Quaternion.identity);
        g.transform.localScale = tileDatabase.tileScale;
        Material mat;
        if(connected)
            mat = tileDatabase.tileMaterials[room_type];
        else
            mat = tileDatabase.tileMaterialsDisconnected[room_type];
        g.GetComponent<MeshRenderer>().material = mat;
        return g;
    }

    public GameObject CreateWallGameObject(Board.ROOM_TYPE roomType, GameObject gPrefab)
    {
        GameObject g = GameObject.Instantiate(gPrefab, new Vector3(0, tileDatabase.boardPropHeight, 0), Quaternion.identity);
        //g.transform.localScale = tileDatabase.propScale;
        return g;
    }

    #endregion
}
