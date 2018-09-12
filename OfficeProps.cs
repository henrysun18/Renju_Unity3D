using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OfficeProps : MonoBehaviour
{
    public Object officePropsPrefab;

	// Attach this script to OfficeProps to enable point and click interaction with the props on table
	void Start () {
	    foreach (Transform setOfProps in transform) //BlackProps and WhiteProps
	    {
	        foreach (Transform prop in setOfProps)
	        {
	            if (prop.GetComponent<BoxCollider>() == null)
	            {
	                prop.gameObject.AddComponent<BoxCollider>(); //simple collider even for lightbulb and pens
	            }
	            else
	            {
	                prop.GetComponent<BoxCollider>().enabled = true; //had to manually set colliders for parent objects
	            }
                prop.gameObject.AddComponent<OfficeProp>();
                prop.gameObject.AddComponent<Rigidbody>();
	            prop.gameObject.tag = GameConstants.PROP;
            }
	    }
	}

    public void ResetAllProps()
    {
        ResetBlackProps();
        ResetWhiteProps();
    }

    public void ResetBlackProps()
    {
        Transform blackProps = transform.Find(GameConstants.BLACK_PROPS);
        foreach (Transform prop in blackProps)
        {
            prop.GetComponent<OfficeProp>().ResetToOriginalTransform();
        }
    }

    public void ResetWhiteProps()
    {
        Transform whiteProps = transform.Find(GameConstants.WHITE_PROPS);
        foreach (Transform prop in whiteProps)
        {
            prop.GetComponent<OfficeProp>().ResetToOriginalTransform();
        }
    }
}

public class OfficeProp : MonoBehaviour
{
    private int negateIfBlack;
    private GameObject original;

    void Start()
    {
        negateIfBlack = transform.parent.name == "BlackProps" ? -1 : 1;
        original = Instantiate(gameObject);
        original.SetActive(false); //hide the temporary object storing original transform values
    }
    void OnMouseDown()
    {
        FlyAway();
    }

    public void FlyAway()
    {
        float speedTowardsRightSideOfTable = negateIfBlack * Random.value*30 - 5;
        float speedTowardsTopSideOfTable = Random.value*10 - 5; //(-5, 5)
        float speedTowardsTheAir = Random.value * 20 + 5;

        float angularSpeedX = Random.value * 20 - 5;
        float angularSpeedY = Random.value * 20 - 5;
        float angularSpeedZ = Random.value * 20 - 5;

        GetComponent<Rigidbody>().velocity = new Vector3(speedTowardsRightSideOfTable, speedTowardsTheAir, speedTowardsTopSideOfTable);
        GetComponent<Rigidbody>().angularVelocity = new Vector3(angularSpeedX, angularSpeedY, angularSpeedZ);
    }

    public void ResetToOriginalTransform()
    {
        GetComponent<Rigidbody>().position = original.transform.position;
        GetComponent<Rigidbody>().rotation = original.transform.rotation;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}
