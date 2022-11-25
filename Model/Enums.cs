using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// will be used to talk to server
public class PlayerNumber
{
    public static int Neither = 0;
    public static int One = 1;
    public static int Two = 2;
    public static int Spectator = 3;
}

public enum IllegalMoveReason
{
    Double3,
    Double4,
    Overline
};

public enum OccupancyState
{
    None,
    Black,
    White,
    IllegalMove,
    OutsideOfBoard
}

public enum Direction
{
    S_N,
    SW_NE,
    W_E,
    NW_SE
}