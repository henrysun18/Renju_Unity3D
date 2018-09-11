using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone {

    public Stone(Point p, GameObject st)
    {
        point = p;
        stoneObj = st;
    }

    public Point point { get; private set; }

    public GameObject stoneObj { get; private set; }

    public static Stone newStoneWithPointAndObjectReference(Point p, GameObject st)
    {
        return new Stone(p, st);
    }
}
