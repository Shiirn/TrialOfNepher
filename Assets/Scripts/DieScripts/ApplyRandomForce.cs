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
    DisplayDieValue dieScript;

    private void Start()
    {
        managerScript = manager.GetComponent<GameManager>();
        dieScript = this.GetComponent<DisplayDieValue>();
    }

    // Update is called once per frame
    void Update()
    {
        if (managerScript.canRollDice)
        {
            if (GetComponent<Rigidbody>().IsSleeping() && Input.GetButtonDown(buttonName))
            {
                RollDie();
            }
        }        
    }

    public void RollDie()
    {
        GetComponent<DisplayDieValue>().diceWasRolled = true;

        GetComponent<Rigidbody>().AddForce((Random.onUnitSphere * forceAmount) * 300);
        GetComponent<Rigidbody>().AddTorque((Random.onUnitSphere * torque) * 300);
    }
}

