using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;


public class IllegalMove
{
    public IllegalMove(Point p, IllegalMoveReason r)
    {
        point = p;
        reason = r;
    }

    public Point point { get; private set; }

    public IllegalMoveReason reason { get; private set; }
}
