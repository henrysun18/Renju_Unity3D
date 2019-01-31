using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenjuBoard : MonoBehaviour
{
    private Camera MainCamera;
    public GameObject BlackStone;
    public GameObject WhiteStone;
    public Button SoloUndoButton;
    public Button BlackUndoButton;
    public Button WhiteUndoButton;
    public GameObject BlackWinMessage;
    public GameObject WhiteWinMessage;

    public static bool IsGameOver;
    public static bool IsBlacksTurn;

    private OnlineMultiplayerClient _onlineMultiplayerClient;
    private IllegalMovesCalculator IllegalMovesCalculator;
    private IllegalMovesController IllegalMovesController;

    private OccupancyState[,] Board = new OccupancyState[GameConfiguration.BOARD_SIZE, GameConfiguration.BOARD_SIZE];
    private LinkedList<Stone> MovesHistory = new LinkedList<Stone>();

    private GameObject WinMessage;

    // Use this for initialization
    void Start ()
    {
        if (GameConfiguration.IsOnlineGame)
        {
            _onlineMultiplayerClient = gameObject.AddComponent<OnlineMultiplayerClient>(); //adds multiplayer script if necessary
        }

        IsBlacksTurn = true;
        IllegalMovesCalculator = new IllegalMovesCalculator(this, MovesHistory);
        IllegalMovesController = new IllegalMovesController(this, IllegalMovesCalculator);

        MainCamera = GameConfiguration.OrientCameraBasedOnPlatform();

        BlackUndoButton.onClick.AddListener(OnUndoButtonPress);
        WhiteUndoButton.onClick.AddListener(OnUndoButtonPress);
        SoloUndoButton.onClick.AddListener(OnUndoButtonPress);
    }

    void OnMouseDown()
    {
        if (IsGameOver)
        {
            ResetGameState();
            return;
        }

        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 50))
        {
            if (hit.collider.gameObject.tag == GameConstants.PROP)
            {
                return; //don't try placing stone if we hit a prop
            }

            Point myMove = Point.At((int) Math.Round(hit.point.x), (int) Math.Round(hit.point.z));

            if (!GameConfiguration.IsOnlineGame) //LOCAL
            {
                AttemptToPlaceStone(myMove); 
            }
            /*else if (!GameConfiguration.IsInOnlineLobby && _onlineMultiplayerClient.IsMyTurn()) //ONLINE MULTIPLAYER
            {
                if (AttemptToPlaceStone(myMove))
                {
                    StartCoroutine(_onlineMultiplayerClient.SetTurnOverAfterMyMove(myMove));
                }
            }*/
        }
    }

    public void OnUndoButtonPress()
    {
        if (MovesHistory.Count == 0 || IsGameOver)
        {
            return;
        }

        if (GameConfiguration.IsOnlineGame)
        {

        }

        UndoOneMove();
    }

    public void UndoOneMove()
    {
        if (MovesHistory.Count == 0)
        {
            return;
        }

        Stone stoneToUndo = MovesHistory.Last.Value;
        MovesHistory.RemoveLast();
        Destroy(stoneToUndo.stoneObj);
        SetPointOnBoardOccupancyState(stoneToUndo.point, OccupancyState.None);

        if (MovesHistory.Count > 0)
        {
            DisableHaloFromPreviousStoneAndEnableOnThisStone(MovesHistory.Last.Value.stoneObj);
        }

        IsBlacksTurn = !IsBlacksTurn;

        //after undo, if it's blacks turn then redraw warnings, otherwise just remove
        IllegalMovesController.DestroyIllegalMoveWarnings();
        if (IsBlacksTurn)
        {
            IllegalMovesController.ShowIllegalMoves();
        }
    }

    public bool AttemptToPlaceStone(Point gridPoint)
    {
        if (GetPointOnBoardOccupancyState(gridPoint) == OccupancyState.None)
        {
            if (IsBlacksTurn)
            {
                SetPointOnBoardOccupancyState(gridPoint, OccupancyState.Black);
                if (IllegalMovesCalculator.MoveProducesFiveToWin(gridPoint, OccupancyState.Black))
                {
                    SetWinner(PlayerNumber.One);
                }
            }
            else
            {
                SetPointOnBoardOccupancyState(gridPoint, OccupancyState.White);
                if (IllegalMovesCalculator.MoveProducesFiveToWin(gridPoint, OccupancyState.White))
                {
                    SetWinner(PlayerNumber.Two);
                }
            }

            if (!GameConfiguration.IsDebugModeWithSamePieceUntilUndo) //don't handle IsBlacksTurn when placing stone, only do so in Undo button
            {
                IsBlacksTurn = !IsBlacksTurn; //next guy's turn
            }

            if (IsBlacksTurn && !IsGameOver) IllegalMovesController.ShowIllegalMoves();
            else IllegalMovesController.DestroyIllegalMoveWarnings();

            return true; //successfully placed stoneObj
        }

        return false;
    }

    public OccupancyState GetPointOnBoardOccupancyState(Point point)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= GameConfiguration.BOARD_SIZE || point.Y >= GameConfiguration.BOARD_SIZE)
        {
            return OccupancyState.OutsideOfBoard;
        }
        return Board[point.X, point.Y];
    }

    public void SetPointOnBoardOccupancyState(Point point, OccupancyState state)
    {
        Vector3 worldVector = new Vector3(point.X, 0, point.Y);

        if (state == OccupancyState.Black)
        {
            GameObject stoneObj = Instantiate(BlackStone, worldVector, Quaternion.identity);
            DisableHaloFromPreviousStoneAndEnableOnThisStone(stoneObj);
            
            MovesHistory.AddLast(Stone.newStoneWithPointAndObjectReference(point, stoneObj));
        }
        else if (state == OccupancyState.White)
        {
            GameObject stoneObj = Instantiate(WhiteStone, worldVector, Quaternion.identity);
            DisableHaloFromPreviousStoneAndEnableOnThisStone(stoneObj);

            MovesHistory.AddLast(Stone.newStoneWithPointAndObjectReference(point, stoneObj));
        }

        Board[point.X, point.Y] = state;
    }

    private void DisableHaloFromPreviousStoneAndEnableOnThisStone(GameObject stoneObj)
    {
        if (MovesHistory.Count > 0)
        {
            GameObject previousStoneObj = MovesHistory.Last.Value.stoneObj;
            Component previousStoneHalo = previousStoneObj.GetComponent("Halo"); //hacky but only way to enable/disable halos through scripting
            previousStoneHalo.GetType().GetProperty("enabled").SetValue(previousStoneHalo, false, null);
        }

        Component stoneHalo = stoneObj.GetComponent("Halo");
        stoneHalo.GetType().GetProperty("enabled").SetValue(stoneHalo, true, null);
    }

    void SetWinner(int playerNumber)
    {
        IsGameOver = true;

        if (playerNumber == PlayerNumber.One)
        {
            WinMessage = BlackWinMessage;
            if (GameConfiguration.IsOnlineGame)
            {
                WinMessage.GetComponent<TextMesh>().text = OnlineMultiplayerClient.OnlineRoomInfo.RoomSummary.P1 + " Wins!";
            }
        }
        else
        {
            WinMessage = WhiteWinMessage;
            if (GameConfiguration.IsOnlineGame)
            {
                WinMessage.GetComponent<TextMesh>().text = OnlineMultiplayerClient.OnlineRoomInfo.RoomSummary.P2 + " Wins!";
            }
        }

        if (GameConfiguration.IsAndroidGame) //rotate towards winner if game is played in portrait mode
        {
            if (playerNumber == PlayerNumber.One)
            {
                WinMessage = Instantiate(WinMessage, WinMessage.transform.position, GameConstants.QuaternionTowardsBlack);
            }
            else
            {
                WinMessage = Instantiate(WinMessage, WinMessage.transform.position, GameConstants.QuaternionTowardsWhite);
            }
        }
        else
        {
            WinMessage = Instantiate(WinMessage);
        }

        ShowMoveHistory();
    }

    private void ShowMoveHistory()
    {
        int moveNumber = 1;
        foreach (Stone move in MovesHistory)
        {
            Transform moveNumberTransform = move.stoneObj.transform.Find(GameConstants.MOVE_NUMBER_TEXT);
            TextMesh textMesh = moveNumberTransform.GetComponent<TextMesh>();
            textMesh.text = moveNumber.ToString();

            if (moveNumber % 2 == 0)
            {
                textMesh.color = Color.black; //set to opposite colour as the piece (white / even number move shows black)
            }

            if (GameConfiguration.IsAndroidGame)
            {
                moveNumberTransform.rotation = GameConstants.QuaternionTowardsBlack;
            }

            moveNumber++;
        }
    }

    void ResetGameState()
    {
        IllegalMovesController.DestroyIllegalMoveWarnings();
        GameObject.Find(GameConstants.OFFICE_PROPS).GetComponent<OfficeProps>().ResetAllProps();
        Board = new OccupancyState[15,15];

        while (MovesHistory.Count > 0) //empty the board of stones
        {
            UndoOneMove();
        }

        if (WinMessage != null)
        {
            Destroy(WinMessage);
        }

        IsGameOver = false;
        IsBlacksTurn = true;
    }
}
