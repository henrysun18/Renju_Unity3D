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
    public GameObject Double3Warning;
    public GameObject Double4Warning;
    public GameObject OverlineWarning;
    public GameObject BlackWinMessage;
    public GameObject WhiteWinMessage;

    public static bool isBlacksTurn = true;
    public static readonly int BOARD_SIZE = 15;
    private static OccupancyState[,] board = new OccupancyState[BOARD_SIZE, BOARD_SIZE];
    private List<IllegalMove> illegalMoves = new List<IllegalMove>();
    private List<GameObject> warningObjects = new List<GameObject>();

    private bool shouldCalculateIllegalMoves = true; //so we don't calculate illegal moves every frame


    // Use this for initialization
    void Start () {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore, //need to be able to update some properties of RoomDto without overwriting others
        };
        InvokeRepeating("SyncGameWithDB", 0.2f, 0.5f); //0.2s delay, repeat every 0.5s
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

    // Update is called once per frame
    void Update () {

	    if (shouldCalculateIllegalMoves)
	    {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            IllegalMovesCalculator calculator = new IllegalMovesCalculator(board);
	        illegalMoves = calculator.CalculateIllegalMoves();
            watch.Stop();
            Debug.Log("milliseconds used to calculate illegal moves: " + watch.ElapsedMilliseconds);

	        InstantiateIllegalMoveWarnings();
	        shouldCalculateIllegalMoves = false;
	    }

	    //DEBUG WITH ONLY BLACK PIECES, RECALCULATE ILLEGAL MOVES AFTER EACH MOVE
	    /*if (Input.GetButtonDown("Fire1"))
	    {
	        isBlacksTurn = true;
	        DestroyIllegalMoveWarnings();
	        for (int x = 0; x < BOARD_SIZE; x++)
	        {
	            for (int y = 0; y < BOARD_SIZE; y++)
	            {
	                if (board[x, y] == OccupancyState.IllegalMove)
	                {
	                    SetPointOnBoardOccupancyState(Point.At(x, y), OccupancyState.None);
	                }
	            }
	        }

	        IllegalMovesCalculator calculator = new IllegalMovesCalculator(board);
	        illegalMoves = calculator.CalculateIllegalMoves();
	        InstantiateIllegalMoveWarnings();
	    }*/
    }

    void OnMouseDown()
    {
        //ONLINE MULTIPLAYER
        if (FirebaseDao.IsMyTurn())
        {
            Ray ray = computerPlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 50))
            {
                Point myMove = CalculateNearestGridPoint(hit);
                bool isMovePlacedSuccessfully = AttemptToPlaceStone(myMove);
                if (isMovePlacedSuccessfully)
                {
                    StartCoroutine(FirebaseDao.SetTurnOverAfterMyMove(myMove));
                }
            }
        }

        //LOCAL
        /*Ray ray = computerPlayerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 50))
        {
            AttemptToPlaceStone(hit);
        }*/
    }

    bool AttemptToPlaceStone(Point gridPoint)
    {
        Vector3 worldVector = new Vector3(gridPoint.X, 0, gridPoint.Y);

        if (GetPointOnBoardOccupancyState(gridPoint) == OccupancyState.None)
        {
            if (isBlacksTurn)
            {
                Instantiate(blackStone, worldVector, Quaternion.identity);
                SetPointOnBoardOccupancyState(gridPoint, OccupancyState.Black);
                DestroyIllegalMoveWarnings();
                if (IllegalMovesCalculator.MoveProducesFiveToWin(gridPoint.X, gridPoint.Y, OccupancyState.Black))
                {
                    SetWinner(PlayerColour.Black);
                }
            }
            else
            {
                Instantiate(whiteStone, worldVector, Quaternion.identity);
                SetPointOnBoardOccupancyState(gridPoint, OccupancyState.White);
                if (IllegalMovesCalculator.MoveProducesFiveToWin(gridPoint.X, gridPoint.Y, OccupancyState.White))
                {
                    SetWinner(PlayerColour.White);
                }
            }

            isBlacksTurn = !isBlacksTurn; //next guy's turn
            if (isBlacksTurn) shouldCalculateIllegalMoves = true;

            return true; //successfully placed stone
        }

        return false;
    }

    Point CalculateNearestGridPoint(RaycastHit hit)
    {
        int X = (int)Math.Round(hit.point.x);
        int Y = (int)Math.Round(hit.point.z);
        return Point.At(X, Y);
    }

    void InstantiateIllegalMoveWarnings()
    {
        foreach (IllegalMove illegalMove in illegalMoves)
        {
            SetPointOnBoardOccupancyState(illegalMove.point, OccupancyState.IllegalMove);

            float worldX = illegalMove.point.X;
            float worldY = 0;
            float worldZ = illegalMove.point.Y;
            Vector3 worldVector3OfIllegalMove = new Vector3(worldX, worldY, worldZ);

            GameObject warning;
            switch (illegalMove.reason)
            {
                case IllegalMoveReason.Double3:
                    warning = Instantiate(Double3Warning, worldVector3OfIllegalMove, Quaternion.AngleAxis(180, Vector3.down)); //prefabs are flipped lol
                    warningObjects.Add(warning);
                    break;
                case IllegalMoveReason.Double4:
                    warning = Instantiate(Double4Warning, worldVector3OfIllegalMove, Quaternion.AngleAxis(180, Vector3.down));
                    warningObjects.Add(warning);
                    break;
                case IllegalMoveReason.Overline:
                    warning = Instantiate(OverlineWarning, worldVector3OfIllegalMove, Quaternion.AngleAxis(180, Vector3.down));
                    warningObjects.Add(warning);
                    break;
                default:
                    throw new Exception("Illegal moves must have a 3x3, 4x4, or overline reason!");
            }
        }
    }

    void DestroyIllegalMoveWarnings()
    {
        foreach (IllegalMove illegalMove in illegalMoves)
        {
            SetPointOnBoardOccupancyState(illegalMove.point, OccupancyState.None);
        }

        foreach (GameObject warning in warningObjects)
        {
            Destroy(warning);
        }
    }

    public static OccupancyState GetPointOnBoardOccupancyState(Point point)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= BOARD_SIZE || point.Y >= BOARD_SIZE)
        {
            return OccupancyState.OutsideOfBoard;
        }
        return board[point.X, point.Y];
    }

    void SetPointOnBoardOccupancyState(Point point, OccupancyState state)
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
