﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastFromCamera : MonoBehaviour
{

    new public Camera camera;

    public GameObject panelMarker;
    public GameObject selectedPanelPrefab;
    public GameObject selectedPanel;
    public GameObject selectedArtifactCard;
    public GameObject selectedItemCard;

    GameObject manager;
    GameManager managerScript;

    private void Start()
    {
        manager = GameObject.Find("Manager");
        managerScript = manager.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastPanel();
        RaycastArtifactCards();
        RaycastItemCards();
    }

    void RaycastArtifactCards()
    {
        selectedArtifactCard = selectedPanelPrefab;

        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            GameObject objectHit = hit.transform.gameObject;
            //Debug.Log(objectHit.name);
            if (objectHit.name.Contains("Panel"))
            {
                panelMarker.transform.position = (new Vector3(objectHit.transform.position.x,
                                                            panelMarker.transform.position.y,
                                                            objectHit.transform.position.z));
            }

            if (Input.GetMouseButtonDown(0))
            {
                selectedPanel = objectHit;
            }

        }
        else
        {
            panelMarker.transform.position = (new Vector3(0, 3, 0));
        }
    }

    void RaycastItemCards()
    {
        selectedItemCard = selectedPanelPrefab;

        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            GameObject objectHit = hit.transform.gameObject;
            //Debug.Log(objectHit.name);

            if (objectHit.name.Contains("sprite"))
            {
                foreach(GameObject item in managerScript.itemCardSprites)
                {
                    if(objectHit.name.Contains(item.name))
                    {
                        Debug.Log("wewe");
                    }
                }
            }
            panelMarker.transform.position = (new Vector3(objectHit.transform.position.x,
                                                        panelMarker.transform.position.y,
                                                        objectHit.transform.position.z));
            

            if (Input.GetMouseButtonDown(0))
            {
                selectedPanel = objectHit;
            }

        }
        else
        {
            panelMarker.transform.position = (new Vector3(0, 3, 0));
        }
    }

    void RaycastPanel()
    {
        selectedPanel = selectedPanelPrefab;

        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            GameObject objectHit = hit.transform.gameObject;
            //Debug.Log(objectHit.name);
            if (objectHit.name.Contains("Panel"))
            {
                panelMarker.transform.position = (new Vector3(objectHit.transform.position.x,
                                                            panelMarker.transform.position.y,
                                                            objectHit.transform.position.z));
            }

            if (Input.GetMouseButtonDown(0))
            {
                selectedPanel = objectHit;
            }

        }
        else
        {
            panelMarker.transform.position = (new Vector3(0, 3, 0));
        }
    }
}
