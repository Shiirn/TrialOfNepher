using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelLevelDown : Panel {

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        GameManager managerScript = manager.GetComponent<GameManager>();

        Character activeCharacter = managerScript.characters[managerScript.activePlayer].GetComponent<Character>();

        activeCharacter.card.LevelDown(1);

        manager.GetComponent<GameManager>().currentPhase = GameManager.TurnPhases.END;
    }

}
