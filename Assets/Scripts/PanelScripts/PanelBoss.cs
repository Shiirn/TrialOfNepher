using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBoss : Panel {
    
    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        manager.GetComponent<GameManager>().currentPhase = GameManager.TurnPhases.BATTLE;
    }

}
