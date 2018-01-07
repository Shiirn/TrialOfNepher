using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovingClouds : MonoBehaviour {

    GameManager managerScript;
    float objectPositionZ;
    float travelSpeedValue;
    float alphaValue;
    float travelTime = 0;
    float horizontalOffset = 0.0f;

	void Start ()
    {
        managerScript = GameObject.Find("Manager").GetComponent<GameManager>();

        horizontalOffset = Random.Range(-10.0f, 0.0f);

        objectPositionZ = Random.Range(-0.8f, 7.2f);
        gameObject.transform.position = new Vector3(gameObject.transform.position.x - horizontalOffset, gameObject.transform.position.y, objectPositionZ);

        travelSpeedValue = Random.Range(0.01f, 0.018f);

        alphaValue = Random.Range(0.25f, 0.5f);
        Color newColor = gameObject.GetComponent<SpriteRenderer>().color;
        newColor.a = alphaValue;
        gameObject.GetComponent<SpriteRenderer>().color = newColor;
	}
		
	void Update ()
    {
        gameObject.transform.Translate(-travelSpeedValue, 0, 0);
        travelTime += travelSpeedValue;

        if(travelTime >= 26)
        {
            managerScript.InstantiateCloud(1);
            Destroy(gameObject);
        }
	}
}
