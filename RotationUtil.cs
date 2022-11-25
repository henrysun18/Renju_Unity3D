using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationUtil : MonoBehaviour
{
    public GameObject rotate2DToBlack;
    public GameObject rotate3DToBlack;
    public GameObject rotate2DToWhite;
    public GameObject rotate3DToWhite;

    void Start()
    {
        RotateTowardsBlackIfAndroid();
        RotateTowardsWhiteIfAndroid();
    }

    public void RotateTowardsBlackIfAndroid()
    {
        if (GameConfiguration.IsAndroidGame)
        {
            if (rotate2DToBlack)
            {
                rotate2DToBlack.transform.rotation = GameConstants.QuaternionTowardsBlack2D;
            }
            if (rotate3DToBlack)
            {
                rotate3DToBlack.transform.rotation = GameConstants.QuaternionTowardsBlack;
            }
        }
    }

    public void RotateTowardsWhiteIfAndroid()
    {
        if (GameConfiguration.IsAndroidGame)
        {
            if (rotate2DToWhite) 
            {
                rotate2DToWhite.transform.rotation = GameConstants.QuaternionTowardsWhite2D;
            }
            if (rotate3DToWhite)
            {
                rotate3DToWhite.transform.rotation = GameConstants.QuaternionTowardsWhite;
            }
        }
    }
}
