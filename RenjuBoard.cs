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
    private LinkedList<Stone> MovesHistory = new LinkedList<Stone>();

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
        GameObject.Find(GameConstants.UNDO_BUTTON_BLACK).GetComponent<Button>().onClick.AddListener(OnUndoButtonPress);
        GameObject.Find(GameConstants.UNDO_BUTTON_WHITE).GetComponent<Button>().onClick.AddListener(OnUndoButtonPress);
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
        if (MovesHistory.Count == 0 || IsGameOver)
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

        Stone stoneToUndo = MovesHistory.Last.Value;
        MovesHistory.RemoveLast();
        Destroy(stoneToUndo.stoneObj);
        SetPointOnBoardOccupancyState(stoneToUndo.point, OccupancyState.None);

        if (MovesHistory.Count > 0)
        {
            DisableHaloFromPreviousStoneAndEnableOnThisStone(MovesHistory.Last.Value.stoneObj);
        }

        if (IsBlacksTurn)
        {
            IllegalMovesController.DestroyIllegalMoveWarnings();
            IllegalMovesController.ShowIllegalMoves();
        }

        IsBlacksTurn = !IsBlacksTurn;
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
                    return true; //don't show illegal moves
                }
            }
            else
            {
                SetPointOnBoardOccupancyState(gridPoint, OccupancyState.White);
                if (IllegalMovesCalculator.MoveProducesFiveToWin(gridPoint, OccupancyState.White))
                {
                    SetWinner(PlayerColour.White);
                    return true;
                }
            }

            IsBlacksTurn = !IsBlacksTurn; //next guy's turn

            if (IsBlacksTurn || GameConfiguration.IsDebugModeWithOnlyBlackPieces) IllegalMovesController.ShowIllegalMoves();
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
            Component previousStoneHalo = previousStoneObj.GetComponent("Halo");
            previousStoneHalo.GetType().GetProperty("enabled").SetValue(previousStoneHalo, false, null);
        }

        Component stoneHalo = stoneObj.GetComponent("Halo");
        stoneHalo.GetType().GetProperty("enabled").SetValue(stoneHalo, true, null);
    }

    void SetWinner(PlayerColour pc)
    {
        IsGameOver = true;

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

        if (GameConfiguration.IsAndroidGame) //rotate towards winner if game is played in portrait mode
        {
            if (pc == PlayerColour.Black)
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
            foreach (Transform child in move.stoneObj.transform) //find the MoveNumberText child object
            {
                if (child.tag == "Text")
                {
                    TextMesh textMesh = child.gameObject.GetComponent<TextMesh>();
                    textMesh.text = moveNumber.ToString();
                    if (moveNumber % 2 == 0)
                    {
                        textMesh.color = Color.black; //set to opposite colour as the piece (white / even number move shows black)
                    }

                    if (GameConfiguration.IsAndroidGame)
                    {
                        child.rotation = GameConstants.QuaternionTowardsBlack;
                    }
                }
            }
            moveNumber++;
        }
    }

    void ResetGameState()
    {
        IllegalMovesController.DestroyIllegalMoveWarnings();
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
        if (GameConfiguration.IsOnlineGame)
        {
            PlayerEntryForm.CreateNewOnlineGame();
        }
    }
}
