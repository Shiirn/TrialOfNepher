using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelGamble : Panel {

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        Debug.Log("You'll roll a dice, if you roll 1 you lose an artifact, or if you roll 6 you get one.");
    }

}
