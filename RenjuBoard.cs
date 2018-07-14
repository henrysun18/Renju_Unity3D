using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class RenjuBoard : MonoBehaviour
{
    public Camera computerPlayerCamera;
    public GameObject blackStone;
    public GameObject whiteStone;
    
    public GameObject BlackWinMessage;
    public GameObject WhiteWinMessage;

    public static bool IsDebugModeWithOnlyBlackPieces = true;
    private static bool IsMultiplayerGame = false;
    public static bool IsBlacksTurn = true;
    public static readonly int BOARD_SIZE = 15;
    private static OccupancyState[,] board = new OccupancyState[BOARD_SIZE, BOARD_SIZE];
    

    // Use this for initialization
    void Start () {
        if (IsMultiplayerGame)
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
        if (FirebaseDao.IsOpponentDoneChoosingAMove())
        {
            AttemptToPlaceStone(FirebaseDao.GetOpponentsLastMove());
        }

        GameObject.Find("P1Label").GetComponent<TextMesh>().text = "P1: " + FirebaseDao.OnlineRoomInfo.Player1;
        GameObject.Find("P2Label").GetComponent<TextMesh>().text = "P2: " + FirebaseDao.OnlineRoomInfo.Player2;
    }

    void OnMouseDown()
    {
        if (IsMultiplayerGame)
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

    bool AttemptToPlaceStone(Point gridPoint)
    {
        Vector3 worldVector = new Vector3(gridPoint.X, 0, gridPoint.Y);

        if (GetPointOnBoardOccupancyState(gridPoint) == OccupancyState.None)
        {
            if (IsBlacksTurn || IsDebugModeWithOnlyBlackPieces)
            {
                Instantiate(blackStone, worldVector, Quaternion.identity);
                SetPointOnBoardOccupancyState(gridPoint, OccupancyState.Black);
                if (IllegalMovesCalculator.MoveProducesFiveToWin(gridPoint, OccupancyState.Black))
                {
                    SetWinner(PlayerColour.Black);
                }
            }
            else
            {
                Instantiate(whiteStone, worldVector, Quaternion.identity);
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
        return board[point.X, point.Y];
    }

    public static void SetPointOnBoardOccupancyState(Point point, OccupancyState state)
    {
        board[point.X, point.Y] = state;
    }

    void SetWinner(PlayerColour pc)
    {
        if (pc == PlayerColour.Black)
        {
            Instantiate(BlackWinMessage);
        }
        else
        {
            Instantiate(WhiteWinMessage);
        }
    }
}
