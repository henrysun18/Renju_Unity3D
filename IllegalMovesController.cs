using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class IllegalMovesController {

    public static GameObject Double3Warning = Resources.Load<GameObject>("Double3");
    public static GameObject Double4Warning = Resources.Load<GameObject>("Double4");
    public static GameObject OverlineWarning = Resources.Load<GameObject>("Overline");

    private static List<IllegalMove> illegalMoves = new List<IllegalMove>();
    private static List<GameObject> warningObjects = new List<GameObject>();

    // Update is called once per frame
    public static void ShowIllegalMoves()
    {
        if (RenjuBoard.IsDebugModeWithOnlyBlackPieces)
        {
            DestroyIllegalMoveWarnings(); //destroy then refresh, otherwise old illegal moves stay there even when legal
        }

        var watch = System.Diagnostics.Stopwatch.StartNew();
        illegalMoves = IllegalMovesCalculator.CalculateIllegalMoves();
        watch.Stop();
        Debug.Log("milliseconds used to calculate illegal moves: " + watch.ElapsedMilliseconds);
        InstantiateIllegalMoveWarnings();
    }

    public static void InstantiateIllegalMoveWarnings()
    {
        foreach (IllegalMove illegalMove in illegalMoves)
        {
            RenjuBoard.SetPointOnBoardOccupancyState(illegalMove.point, OccupancyState.IllegalMove);

            float worldX = illegalMove.point.X;
            float worldY = 0;
            float worldZ = illegalMove.point.Y;
            Vector3 worldVector3OfIllegalMove = new Vector3(worldX, worldY, worldZ);

            GameObject warning;
            switch (illegalMove.reason)
            {
                case IllegalMoveReason.Double3:
                    warning = UnityEngine.Object.Instantiate(Double3Warning, worldVector3OfIllegalMove, Quaternion.AngleAxis(180, Vector3.down)); //prefabs are flipped lol
                    warningObjects.Add(warning);
                    break;
                case IllegalMoveReason.Double4:
                    warning = UnityEngine.Object.Instantiate(Double4Warning, worldVector3OfIllegalMove, Quaternion.AngleAxis(180, Vector3.down));
                    warningObjects.Add(warning);
                    break;
                case IllegalMoveReason.Overline:
                    warning = UnityEngine.Object.Instantiate(OverlineWarning, worldVector3OfIllegalMove, Quaternion.AngleAxis(180, Vector3.down));
                    warningObjects.Add(warning);
                    break;
                default:
                    throw new Exception("Illegal moves must have a 3x3, 4x4, or overline reason!");
            }
        }
    }

    public static void DestroyIllegalMoveWarnings()
    {
        foreach (IllegalMove illegalMove in illegalMoves)
        {
            RenjuBoard.SetPointOnBoardOccupancyState(illegalMove.point, OccupancyState.None);
        }

        foreach (GameObject warning in warningObjects)
        {
            UnityEngine.Object.Destroy(warning);
        }
    }
}
