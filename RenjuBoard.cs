using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenjuBoard : MonoBehaviour
{
    public Camera computerPlayerCamera;
    public GameObject BlackWinMessage;
    public GameObject WhiteWinMessage;

    public static bool IsGameOver;
    public static bool IsBlacksTurn;

    private FirebaseDao FirebaseDao;
    private IllegalMovesCalculator IllegalMovesCalculator;
    private IllegalMovesController IllegalMovesController;

    private OccupancyState[,] Board = new OccupancyState[GameConfiguration.BOARD_SIZE, GameConfiguration.BOARD_SIZE];
    private Stack<Stone> MovesHistory = new Stack<Stone>();

    private static GameObject BlackStone;
    private static GameObject WhiteStone;
    private GameObject WinMessage;

    // Use this for initialization
    void Start ()
    {
        IsBlacksTurn = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (GameConfiguration.IsOnlineGame)
        {
            FirebaseDao = gameObject.AddComponent<FirebaseDao>(); //adds multiplayer script if necessary
        }

        IllegalMovesCalculator = new IllegalMovesCalculator(this);
        IllegalMovesController = new IllegalMovesController(this, IllegalMovesCalculator);

        BlackStone = Resources.Load<GameObject>(GameConstants.BLACK_STONE);
        WhiteStone = Resources.Load<GameObject>(GameConstants.WHITE_STONE);
        GameObject.Find(GameConstants.UNDO_BUTTON_GAMEOBJECT).GetComponent<Button>().onClick.AddListener(OnUndoButtonPress);
    }

    void OnMouseDown()
    {
        if (IsGameOver)
        {
            ResetGameState();
            return;
        }

        Ray ray = computerPlayerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 50))
        {
            Point myMove = Point.At((int) Math.Round(hit.point.x), (int) Math.Round(hit.point.z));

            if (!GameConfiguration.IsOnlineGame) //LOCAL
            {
                AttemptToPlaceStone(myMove); 
            }
            else if (FirebaseDao.IsMyTurn() && AttemptToPlaceStone(myMove)) //ONLINE MULTIPLAYER
            {
                StartCoroutine(FirebaseDao.SetTurnOverAfterMyMove(myMove));
            }
            
        }
    }

    public void OnUndoButtonPress()
    {
        if (MovesHistory.Count == 0)
        {
            return;
        }

        if (GameConfiguration.IsOnlineGame)
        {
            if (!IsGameOver && FirebaseDao.IsUndoButtonAvailable())
            {
                StartCoroutine(FirebaseDao.PressUndoButton());
            }
            else
            {
                return; //disable undo button until undo button is acknowledged
            }
        }

        UndoOneMove();
    }

    public void UndoOneMove()
    {
        if (MovesHistory.Count == 0)
        {
            return;
        }

        Stone stoneToUndo = MovesHistory.Pop();

        Destroy(stoneToUndo.stone);
        SetPointOnBoardOccupancyState(stoneToUndo.point, OccupancyState.None);
        IsBlacksTurn = !IsBlacksTurn;

        if (IsBlacksTurn)
        {
            IllegalMovesController.DestroyIllegalMoveWarnings();
            IllegalMovesController.ShowIllegalMoves();
        }
    }

    public bool AttemptToPlaceStone(Point gridPoint)
    {

        if (GetPointOnBoardOccupancyState(gridPoint) == OccupancyState.None)
        {
            if (IsBlacksTurn || GameConfiguration.IsDebugModeWithOnlyBlackPieces)
            {
                SetPointOnBoardOccupancyState(gridPoint, OccupancyState.Black);
                if (IllegalMovesCalculator.MoveProducesFiveToWin(gridPoint, OccupancyState.Black))
                {
                    SetWinner(PlayerColour.Black);
                }
            }
            else
            {
                SetPointOnBoardOccupancyState(gridPoint, OccupancyState.White);
                if (IllegalMovesCalculator.MoveProducesFiveToWin(gridPoint, OccupancyState.White))
                {
                    SetWinner(PlayerColour.White);
                }
            }

            IsBlacksTurn = !IsBlacksTurn; //next guy's turn

            if (IsBlacksTurn || GameConfiguration.IsDebugModeWithOnlyBlackPieces) IllegalMovesController.ShowIllegalMoves();
            else IllegalMovesController.DestroyIllegalMoveWarnings();

            return true; //successfully placed stone
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
            GameObject stone = Instantiate(BlackStone, worldVector, Quaternion.identity);
            MovesHistory.Push(Stone.newStoneWithPointAndObjectReference(point, stone));
        }
        else if (state == OccupancyState.White)
        {
            GameObject stone = Instantiate(WhiteStone, worldVector, Quaternion.identity);
            MovesHistory.Push(Stone.newStoneWithPointAndObjectReference(point, stone));
        }

        Board[point.X, point.Y] = state;
    }

    void SetWinner(PlayerColour pc)
    {
        if (pc == PlayerColour.Black)
        {
            WinMessage = BlackWinMessage;
            if (GameConfiguration.IsOnlineGame)
            {
                WinMessage.GetComponent<TextMesh>().text = FirebaseDao.OnlineRoomInfo.PlayerInfo.Player1 + " Wins!";
            }
        }
        else
        {
            WinMessage = WhiteWinMessage;
            if (GameConfiguration.IsOnlineGame)
            {
                WinMessage.GetComponent<TextMesh>().text = FirebaseDao.OnlineRoomInfo.PlayerInfo.Player2 + " Wins!";
            }
        }

        WinMessage = Instantiate(WinMessage);
        IsGameOver = true;
    }

    void ResetGameState()
    {
        IllegalMovesController.DestroyIllegalMoveWarnings();
        Board = new OccupancyState[15,15];

        while (MovesHistory.Count > 0) //empty the board of stones
        {
            OnUndoButtonPress();
        }

        if (WinMessage != null)
        {
            Destroy(WinMessage);
        }

        IsGameOver = false;
        IsBlacksTurn = true;
        if (GameConfiguration.IsOnlineGame)
        {
            PlayerEntryForm.CreateNewOnlineGame();
        }
    }
}
