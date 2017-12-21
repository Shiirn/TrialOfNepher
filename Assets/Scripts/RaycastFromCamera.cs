using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastFromCamera : MonoBehaviour
{

    public Camera camera;
    public GameObject panelMarker;

    private void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
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

        }
        else
        {
            panelMarker.transform.position = (new Vector3(0, 3, 0));
        }
    }
}
