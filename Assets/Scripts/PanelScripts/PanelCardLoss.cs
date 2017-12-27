using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelCardLoss : Panel {

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        Debug.Log("If you had at least a card, now you lost one");
    }
}
