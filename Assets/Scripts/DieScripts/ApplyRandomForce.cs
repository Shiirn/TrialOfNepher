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
        Physics.gravity = new Vector3(0, -20.0F, 0);
    }

    // Update is called once per frame
    void Update()
    {
            
    }

    public void RollDie()
    {
        GetComponent<DisplayDieValue>().diceWasRolled = true;

        GetComponent<Rigidbody>().AddForce((Vector3.up * forceAmount));

        Vector3 funnyAttempt = Random.onUnitSphere;
        funnyAttempt.y = 0;

        Vector3 funnyAttempt2 = Random.onUnitSphere;
        funnyAttempt2.x = 0;

        Vector3 funnyAttempt3 = Random.onUnitSphere;
        funnyAttempt2.z = 0;

        GetComponent<Rigidbody>().AddForce((funnyAttempt * forceAmount) * 0.25f);
        GetComponent<Rigidbody>().AddForce((funnyAttempt2 * forceAmount) * 0.1f);
        GetComponent<Rigidbody>().AddForce((funnyAttempt2 * forceAmount) * 0.1f);

        GetComponent<Rigidbody>().AddTorque(Random.onUnitSphere * torque * 10.0f);
    }
}

