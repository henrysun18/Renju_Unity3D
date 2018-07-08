using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public bool playerColour;

    public static readonly int BOARD_SIZE = 15;
    private OccupancyState[,] board = new OccupancyState[BOARD_SIZE, BOARD_SIZE];
    private List<IllegalMove> illegalMoves = new List<IllegalMove>();
    private List<GameObject> warningObjects = new List<GameObject>();

    private bool isBlacksTurn = true;
    private bool shouldCalculateIllegalMoves = true; //so we don't calculate illegal moves every frame

    // Use this for initialization
    void Start () {
		
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

	    if (Input.GetButtonDown("Fire1"))
	    {
	        Ray ray = computerPlayerCamera.ScreenPointToRay(Input.mousePosition);
	        RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 50))
            {
                AttemptToPlaceStone(hit);
	        }
        }

        //DEBUG
	    if (Input.GetButtonDown("Fire2"))
	    {
	        shouldCalculateIllegalMoves = false;
            DestroyIllegalMoveWarnings();
	    }
	}

    void AttemptToPlaceStone(RaycastHit hit)
    {
        Vector3 nearestGridPoint = CalculateNearestGridPoint(hit);
        int X = (int)Math.Round(nearestGridPoint.x);
        int Y = (int)Math.Round(nearestGridPoint.z);

        if (board[X, Y] == OccupancyState.None)
        {
            if (isBlacksTurn)
            {
                Instantiate(blackStone, nearestGridPoint, Quaternion.identity);
                SetPointOnBoardOccupancyState(new Point(X, Y), OccupancyState.Black);
                //shouldCalculateIllegalMoves = false;
                //DestroyIllegalMoveWarnings();
                if (IllegalMovesCalculator.MoveProducesFiveToWin(X, Y, OccupancyState.Black))
                {
                    SetWinner(PlayerColour.Black);
                    return;
                }
shouldCalculateIllegalMoves = true;
            }
            else
            {
                Instantiate(whiteStone, nearestGridPoint, Quaternion.identity);
                SetPointOnBoardOccupancyState(new Point(X, Y), OccupancyState.White);
                if (IllegalMovesCalculator.MoveProducesFiveToWin(X, Y, OccupancyState.White))
                {
                    SetWinner(PlayerColour.White);
                    return;
                }

                shouldCalculateIllegalMoves = true; //recalculate only once the board is updated with new move (not every frame)
            }

            //isBlacksTurn = !isBlacksTurn; //next guy's turn
        }
    }

    Vector3 CalculateNearestGridPoint(RaycastHit hit)
    {
        float nearestGridPointX = (float)Math.Round((decimal)hit.point.x, MidpointRounding.ToEven);
        float nearestGridPointY = (float)Math.Round((decimal)hit.point.y, MidpointRounding.ToEven);
        float nearestGridPointZ = (float)Math.Round((decimal)hit.point.z, MidpointRounding.ToEven);
        return new Vector3(nearestGridPointX, nearestGridPointY, nearestGridPointZ);
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

    OccupancyState GetPointOnBoardOccupancyState(Point point)
    {
        if (point.X < 0 || point.Y < 0 || point.X > Math.Sqrt(board.Length) || point.Y > Math.Sqrt(board.Length))
        {
            return OccupancyState.OutsideOfBoard;
        }
        else
        {
            return board[point.X, point.Y];
        }
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
