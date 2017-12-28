using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelMonster : Panel {

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        GameManager managerScript = manager.GetComponent<GameManager>();

        GameObject activeMonster = GameObject.Find("ActiveMonster");
        ActiveFighter activeMonsterScript = activeMonster.GetComponent<ActiveFighter>();
        
        activeMonsterScript.CheckIfMonsterIsAlive();

        if (!activeMonsterScript.isMonsterActive)
        {

            activeMonsterScript.FlipNewActiveFighter(Random.Range(1, 5));
            activeMonsterScript.isMonsterActive = true;
        }

        managerScript.monsterScript = activeMonsterScript;

        managerScript.currentPhase = GameManager.TurnPhases.BATTLE;
    }
}
