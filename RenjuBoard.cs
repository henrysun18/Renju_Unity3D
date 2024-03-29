﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenjuBoard : MonoBehaviour
{
    public GameObject BlackStone;
    public GameObject WhiteStone;
    public Button SoloUndoButton;
    public Button BlackUndoButton;
    public Button WhiteUndoButton;
    public Button SoloRespawnPropsButton;
    public Button BlackRespawnPropsButton;
    public Button WhiteRespawnPropsButton;
    public GameObject BlackWinMessage;
    public GameObject WhiteWinMessage;

    public static bool IsGameOver;
    public static bool IsGameOverMouseDowned; // hack to ensure we only reset game state after a full mousedown = mouseup (to avoid duplicate button presses e.g. re-joining a room)
    public static bool IsBlacksTurn;
    public static GameObject WinMessage;

    private Camera MainCamera;
    private IllegalMovesCalculator IllegalMovesCalculator;
    private IllegalMovesController IllegalMovesController;
    private OnlineRoomSelection OnlineRoomSelection;
    private OnlineMultiplayerClient OnlineMultiplayerClient;

    private OccupancyState[,] Board = new OccupancyState[GameConfiguration.BOARD_SIZE, GameConfiguration.BOARD_SIZE];
    private LinkedList<Stone> MovesHistory = new LinkedList<Stone>();


    // Use this for initialization
    void Start ()
    {
        IsBlacksTurn = true;
        IllegalMovesCalculator = new IllegalMovesCalculator(this, MovesHistory);
        IllegalMovesController = new IllegalMovesController(this, IllegalMovesCalculator);
        OnlineRoomSelection = GetComponent<OnlineRoomSelection>();
        OnlineMultiplayerClient = GetComponent<OnlineMultiplayerClient>();
        OnlineRoomSelection.Init(this);
        OnlineMultiplayerClient.Init(this);

        MainCamera = GameConfiguration.OrientCameraBasedOnPlatform();

        InitializeUndoAndOfficePropsButtons();
    }

    void OnMouseDown()
    {
        if (IsGameOver)
        {
            IsGameOverMouseDowned = true;
            return;
        }

        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 50))
        {
            if (hit.collider.gameObject.tag == GameConstants.PROP)
            {
                return; //don't try placing stone if we hit a prop (or undo modal tagged as prop)
            }

            Point myMove = Point.At((int) Math.Round(hit.point.x), (int) Math.Round(hit.point.z));

            if (!GameConfiguration.IsOnlineGame) //LOCAL
            {
                AttemptToPlaceStone(myMove); 
            }
            else if (GameConfiguration.IsOnlineGame && OnlineMultiplayerClient.IsMyTurn()) //ONLINE MULTIPLAYER
            {
                Debug.Log("attemping to place stone " + myMove.X + "," + myMove.Y);
                if (AttemptToPlaceStone(myMove))
                {
                    StartCoroutine(OnlineMultiplayerClient.MakeMove(myMove));
                }
            }
        }
    }

    private void OnMouseUp()
    {
        if (IsGameOverMouseDowned)
        {
            ResetGameState();
            return;
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
            // send undo request, but only if it's your turn (to avoid dealing with race conditions)
            if (OnlineMultiplayerClient.IsMyTurn())
            {
                OnlineMultiplayerClient.UndoRequest();
            }
        } else
        {
            UndoOneMove();
        }
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

        EndTurn();

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
                EndTurn(); //next guy's turn
            }

            if (IsBlacksTurn && !IsGameOver) IllegalMovesController.ShowIllegalMoves();
            else IllegalMovesController.DestroyIllegalMoveWarnings();

            return true; //successfully placed stoneObj
        }

        return false;
    }

    public void EndTurn()
    {
        IsBlacksTurn = !IsBlacksTurn;

        if (GameConfiguration.IsOnlineGame)
        {
            // make undo button look disabled when it's not our turn (since it already doesn't work when it's not our turn)
            // also, let's avoid the "undo my turn even though I haven't gone yet" bug that occurs when white tries to undo with only 1 black piece on the board
            if (OnlineMultiplayerClient.IsMyTurn() && MovesHistory.Count >= 2)
            {
                SoloUndoButton.interactable = true;
                BlackUndoButton.interactable = true;
                WhiteUndoButton.interactable = true;
            } else
            {
                SoloUndoButton.interactable = false;
                BlackUndoButton.interactable = false;
                WhiteUndoButton.interactable = false;
            }
        }
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

    public void SetWinner(int playerNumber)
    {
        IsGameOver = true;

        // clean up pre-existing instance of WinMessage if for some reason it's still there
        if (WinMessage != null)
        {
            Destroy(WinMessage);
        }

        if (playerNumber == PlayerNumber.One)
        {
            WinMessage = BlackWinMessage;
            if (GameConfiguration.IsOnlineGame)
            {
                WinMessage.GetComponent<TextMesh>().text = OnlineMultiplayerClient.OnlineRoomInfo.RoomSummary.P1 + " Wins!";
            } else
            {
                WinMessage.GetComponent<TextMesh>().text = "Black Wins!";
            }
        }
        else
        {
            WinMessage = WhiteWinMessage;
            if (GameConfiguration.IsOnlineGame)
            {
                WinMessage.GetComponent<TextMesh>().text = OnlineMultiplayerClient.OnlineRoomInfo.RoomSummary.P2 + " Wins!";
            } else
            {
                WinMessage.GetComponent<TextMesh>().text = "White Wins!";
            }
        }

        if (GameConfiguration.IsAndroidGame) //rotate upright relative to device
        {
            WinMessage = Instantiate(WinMessage, WinMessage.transform.position, GameConstants.QuaternionTowardsBlack);
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

    public void ResetGameState()
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
        IsGameOverMouseDowned = false;
        IsBlacksTurn = true;

        if (GameConfiguration.IsOnlineGame)
        {
            OnlineMultiplayerClient.ResetGame();
            OnlineRoomSelection.ExitBackToLobby();
        }
    }

    void InitializeUndoAndOfficePropsButtons()
    {
        if (GameConfiguration.IsAndroidGame)
        {
            // show two undo/respawn props buttons when we're Android
            SoloUndoButton.gameObject.SetActive(false);
            BlackUndoButton.gameObject.SetActive(true);
            WhiteUndoButton.gameObject.SetActive(true);
            SoloRespawnPropsButton.gameObject.SetActive(false);
            BlackRespawnPropsButton.gameObject.SetActive(true);
            WhiteRespawnPropsButton.gameObject.SetActive(true);
        }
        BlackUndoButton.onClick.AddListener(OnUndoButtonPress);
        WhiteUndoButton.onClick.AddListener(OnUndoButtonPress);
        SoloUndoButton.onClick.AddListener(OnUndoButtonPress);
    }
}
