using System;
using System.Collections.Generic;
using UnityEngine;

public class IllegalMovesController {

    public static GameObject Double3Warning = Resources.Load<GameObject>("Double3");
    public static GameObject Double4Warning = Resources.Load<GameObject>("Double4");
    public static GameObject OverlineWarning = Resources.Load<GameObject>("Overline");

    private RenjuBoard RenjuBoard;
    private IllegalMovesCalculator IllegalMovesCalculator;
    private static List<IllegalMove> IllegalMoves = new List<IllegalMove>();
    private static List<GameObject> WarningObjects = new List<GameObject>();

    public IllegalMovesController(RenjuBoard boardToOperateOn, IllegalMovesCalculator illegalMovesCalculator)
    {
        RenjuBoard = boardToOperateOn;
        IllegalMovesCalculator = illegalMovesCalculator;
    }

    // Update is called once per frame
    public void ShowIllegalMoves()
    {
        if (GameConfiguration.IsDebugModeWithSamePieceUntilUndo)
        {
            DestroyIllegalMoveWarnings(); //destroy then refresh, otherwise old illegal moves stay there even when legal
        }

        var watch = System.Diagnostics.Stopwatch.StartNew();
        IllegalMoves = IllegalMovesCalculator.CalculateIllegalMoves();
        watch.Stop();
        Debug.Log("milliseconds used to calculate illegal moves: " + watch.ElapsedMilliseconds);
        InstantiateIllegalMoveWarnings();
    }

    public void InstantiateIllegalMoveWarnings()
    {
        foreach (IllegalMove illegalMove in IllegalMoves)
        {
            RenjuBoard.SetPointOnBoardOccupancyState(illegalMove.point, OccupancyState.IllegalMove);

            float worldX = illegalMove.point.X;
            float worldY = 0;
            float worldZ = illegalMove.point.Y;
            Vector3 worldVector3OfIllegalMove = new Vector3(worldX, worldY, worldZ);
            Quaternion illegalMoveRotation = GameConfiguration.IsAndroidGame
                ? GameConstants.IllegalMoveQuaternion
                : Quaternion.identity;

            GameObject warning;
            switch (illegalMove.reason)
            {
                case IllegalMoveReason.Double3:
                    warning = UnityEngine.Object.Instantiate(Double3Warning, worldVector3OfIllegalMove, illegalMoveRotation);
                    WarningObjects.Add(warning);
                    break;
                case IllegalMoveReason.Double4:
                    warning = UnityEngine.Object.Instantiate(Double4Warning, worldVector3OfIllegalMove, illegalMoveRotation);
                    WarningObjects.Add(warning);
                    break;
                case IllegalMoveReason.Overline:
                    warning = UnityEngine.Object.Instantiate(OverlineWarning, worldVector3OfIllegalMove, illegalMoveRotation);
                    WarningObjects.Add(warning);
                    break;
                default:
                    throw new Exception("Illegal moves must have a 3x3, 4x4, or overline reason!");
            }
        }
    }

    public void DestroyIllegalMoveWarnings()
    {
        foreach (IllegalMove illegalMove in IllegalMoves)
        {
            RenjuBoard.SetPointOnBoardOccupancyState(illegalMove.point, OccupancyState.None);
        }

        foreach (GameObject warning in WarningObjects)
        {
            UnityEngine.Object.Destroy(warning);
        }
    }
}
