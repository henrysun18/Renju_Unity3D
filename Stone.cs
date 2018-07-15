using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone {

    public Stone(Point p, GameObject st)
    {
        point = p;
        stone = st;
    }

    public Point point { get; private set; }

    public GameObject stone { get; private set; }

    public static Stone newStoneWithPointAndObjectReference(Point p, GameObject st)
    {
        return new Stone(p, st);
    }
}
