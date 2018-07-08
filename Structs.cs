using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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

    public static Point At(int x, int y)
    {
        return new Point(x, y);
    }

    public Point GetPointNStepsAfter(int steps, Direction dir)
    {
        int deltaX = 0;
        int deltaY = 0;
        switch (dir)
        {
            case Direction.S_N:
                deltaX = 0;
                deltaY = 1;
                break;
            case Direction.SW_NE:
                deltaX = 1;
                deltaY = 1;
                break;
            case Direction.W_E:
                deltaX = 1;
                deltaY = 0;
                break;
            case Direction.NW_SE:
                deltaX = 1;
                deltaY = -1;
                break;
        }

        return new Point(X + steps*deltaX, Y + steps*deltaY);
    }
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
