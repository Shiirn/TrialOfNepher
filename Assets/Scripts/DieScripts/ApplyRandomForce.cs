using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyRandomForce : MonoBehaviour
{
    public string buttonName = null;
    public float forceAmount = 50.0f;
    public ForceMode forceMode;
    public float torque = 10.0f;
        
    public GameObject manager;
    GameManager managerScript;

    private void Start()
    {
        managerScript = manager.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
            
    }

    public void RollDie()
    {
        GetComponent<DisplayDieValue>().diceWasRolled = true;

        GetComponent<Rigidbody>().AddForce((Vector3.up * Random.Range(forceAmount*0.8f, forceAmount)));

        Vector3 funnyAttempt = Random.onUnitSphere;
        funnyAttempt.y = 0;

        GetComponent<Rigidbody>().AddForce((funnyAttempt * forceAmount) * 0.4f);
        GetComponent<Rigidbody>().AddTorque((Random.onUnitSphere * torque));
    }
}

