using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelDraw : Panel {

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        Debug.Log("You draw a card.");
    }

}
