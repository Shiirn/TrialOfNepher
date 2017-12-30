using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayDieValue : MonoBehaviour {

    public LayerMask dieValueColliderLayer = -1;
    private int currentValue = 1;
    public bool stoppedRolling = false;
    public bool rollComplete;
    public bool diceWasRolled;
    private Vector3 origin;

    public GameObject manager;
    GameManager managerScript;

    RaycastHit hit;

    // Update is called once per frame
    private void Start()
    {
        managerScript = manager.GetComponent<GameManager>();
        origin = transform.position;
    }

    void Update () {

        if (Physics.Raycast(transform.position, Vector3.up,out hit, Mathf.Infinity, dieValueColliderLayer))
        {
            currentValue = hit.collider.GetComponent<DieValue>().value;
        }

        if (GetComponent<Rigidbody>().IsSleeping() && !stoppedRolling && diceWasRolled)
        {
            stoppedRolling = true;
            Debug.Log("Roll complete!");
            GetComponent<Rigidbody>().position = origin;
        }

        else if (!GetComponent<Rigidbody>().IsSleeping() && stoppedRolling)
        {
            managerScript.diceRoll = currentValue;
            stoppedRolling = false;
            diceWasRolled = false;
            rollComplete = true;
        }
	}
}
