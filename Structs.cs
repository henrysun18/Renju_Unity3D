using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Point
{

    public Point(int x, int y) : this()
    {
        X = x;
        Y = y;
    }

    public int X { get; private set; }

    public int Y { get; private set; }
}

public struct IllegalMove
{
    public IllegalMove(Point p, IllegalMoveReason r) : this()
    {
        point = p;
        reason = r;
    }

    public Point point { get; private set; }

    public IllegalMoveReason reason { get; private set; }
}
