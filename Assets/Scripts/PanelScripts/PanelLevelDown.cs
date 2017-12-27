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

        //DEBYGGYIU
        Debug.Log(activeCharacter.card.level);


        Debug.Log("Moving on to the End Phase");
        manager.GetComponent<GameManager>().currentPhase = GameManager.TurnPhases.END;
    }

}
