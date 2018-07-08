using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerColour
{
    Black,
    White
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
    SE_NE,
    W_E,
    NW_SE
}