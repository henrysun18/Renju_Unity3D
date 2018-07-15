using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class RenjuBoard : MonoBehaviour
{
    public Camera computerPlayerCamera;
    

    public static bool IsDebugModeWithOnlyBlackPieces = false;
    public static bool IsOnlineGame = true;
    public static bool IsGameOver = false;
    public static bool IsBlacksTurn = true;
    public static readonly int BOARD_SIZE = 15;

    private static OccupancyState[,] Board = new OccupancyState[BOARD_SIZE, BOARD_SIZE];
    private static Stack<Stone> MovesHistory = new Stack<Stone>();

    private static GameObject BlackStone;
    private static GameObject WhiteStone;
    private static GameObject BlackWinMessage;
    private static GameObject WhiteWinMessage;

    // Use this for initialization
    void Start () {
        BlackStone = Resources.Load<GameObject>(GameConstants.BLACK_STONE);
        WhiteStone = Resources.Load<GameObject>(GameConstants.WHITE_STONE);
        GameObject.Find(GameConstants.UNDO_BUTTON_GAMEOBJECT).GetComponent<Button>().onClick.AddListener(OnUndoButtonPress);

        if (IsOnlineGame)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore, //need to be able to update some properties of RoomDto without overwriting others
            };
            InvokeRepeating("SyncGameWithDB", 0.2f, 0.5f); //0.2s delay, repeat every 0.5s
        }
    }

    void SyncGameWithDB()
    {
        StartCoroutine(FirebaseDao.GetRoomInfo());

        if (FirebaseDao.IsUndoButtonUnacknowledged())
        {
            OnUndoButtonPress();
            StartCoroutine(FirebaseDao.ConfirmUndoRequest());
        }
        else if (FirebaseDao.IsOpponentDoneChoosingAMove())
        {
            AttemptToPlaceStone(FirebaseDao.GetOpponentsLastMove());
        }
        

        GameObject.Find(GameConstants.P1_LABEL_GAMEOBJECT).GetComponent<Text>().text = "P1: " + FirebaseDao.OnlineRoomInfo.Player1;
        GameObject.Find(GameConstants.P2_LABEL_GAMEOBJECT).GetComponent<Text>().text = "P2: " + FirebaseDao.OnlineRoomInfo.Player2;
    }

    void OnMouseDown()
    {
        if (IsGameOver)
        {
            ResetGameState();
        }
        else if (IsOnlineGame)
        {
            //ONLINE MULTIPLAYER
            if (FirebaseDao.IsMyTurn())
            {
                Ray ray = computerPlayerCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 50))
                {
                    Point myMove = Point.At((int) Math.Round(hit.point.x), (int) Math.Round(hit.point.z));
                    bool isMovePlacedSuccessfully = AttemptToPlaceStone(myMove);
                    if (isMovePlacedSuccessfully)
                    {
                        StartCoroutine(FirebaseDao.SetTurnOverAfterMyMove(myMove));
                    }
                }
            }
        }
        else
        {
            //LOCAL
            Ray ray = computerPlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 50))
            {
                Point myMove = Point.At((int)Math.Round(hit.point.x), (int)Math.Round(hit.point.z));
                AttemptToPlaceStone(myMove);
            }
        }
    }

    void OnUndoButtonPress()
    {
        if (IsOnlineGame && !FirebaseDao.IsUndoButtonUnacknowledged())
        {
            StartCoroutine(FirebaseDao.PressUndoButton());
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

    bool AttemptToPlaceStone(Point gridPoint)
    {

        if (GetPointOnBoardOccupancyState(gridPoint) == OccupancyState.None)
        {
            if (IsBlacksTurn || IsDebugModeWithOnlyBlackPieces)
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

            if (IsBlacksTurn || IsDebugModeWithOnlyBlackPieces) IllegalMovesController.ShowIllegalMoves();
            else IllegalMovesController.DestroyIllegalMoveWarnings();

            return true; //successfully placed stone
        }

        return false;
    }

    public static OccupancyState GetPointOnBoardOccupancyState(Point point)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= BOARD_SIZE || point.Y >= BOARD_SIZE)
        {
            return OccupancyState.OutsideOfBoard;
        }
        return Board[point.X, point.Y];
    }

    public static void SetPointOnBoardOccupancyState(Point point, OccupancyState state)
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
            GameObject WinMessage = Resources.Load<GameObject>(GameConstants.BLACK_WIN_MESSAGE);
            if (IsOnlineGame)
            {
                WinMessage.GetComponent<Text>().text = FirebaseDao.OnlineRoomInfo.Player1 + " Wins!";
            }

            BlackWinMessage = Instantiate(WinMessage);
        }
        else
        {
            GameObject WinMessage = Resources.Load<GameObject>(GameConstants.WHITE_WIN_MESSAGE);
            if (IsOnlineGame)
            {
                WinMessage.GetComponent<Text>().text = FirebaseDao.OnlineRoomInfo.Player2 + " Wins!";
            }

            WhiteWinMessage = Instantiate(WinMessage);
        }

        IsGameOver = true;
    }

    void ResetGameState()
    {
        IllegalMovesController.DestroyIllegalMoveWarnings();
        Board = new OccupancyState[15,15];
        FirebaseDao.OnlineRoomInfo = null;

        while (MovesHistory.Count > 0) //empty the board of stones
        {
            OnUndoButtonPress();
        }

        if (BlackWinMessage != null)
        {
            Destroy(BlackWinMessage);
        }
        if (WhiteWinMessage != null)
        {
            Destroy(WhiteWinMessage);
        }

        IsGameOver = false;
        IsBlacksTurn = true;
        PlayerEntryForm.CreateNewOnlineGame();
    }
}
