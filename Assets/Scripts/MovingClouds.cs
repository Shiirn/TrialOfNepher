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

	void Start ()
    {
        managerScript = GameObject.Find("Manager").GetComponent<GameManager>();

        objectPositionZ = Random.Range(-0.5f, 5.9f);
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, objectPositionZ);

        travelSpeedValue = Random.Range(0.01f, 0.1f);

        alphaValue = Random.Range(0.2f, 1f);
        Color newColor = gameObject.GetComponent<SpriteRenderer>().color;
        newColor.a = alphaValue;
        gameObject.GetComponent<SpriteRenderer>().color = newColor;

        
	}
		
	void Update ()
    {
        gameObject.transform.Translate(-travelSpeedValue, 0, 0);
        travelTime += travelSpeedValue;

        if(travelTime >= 15)
        {
            managerScript.InstantiateCloud(1);
            Destroy(gameObject);
        }
	}
}
