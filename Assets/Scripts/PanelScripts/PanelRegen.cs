﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelRegen : Panel {

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        GameManager managerScript = manager.GetComponent<GameManager>();

        Character activeCharacter = managerScript.characters[managerScript.activePlayer].GetComponent<Character>();

        activeCharacter.card.hp++;
        if (activeCharacter.card.hp > activeCharacter.card.stats.maxHp)
            activeCharacter.card.hp = activeCharacter.card.stats.maxHp;

        //DEBUGGYYYY
        Debug.Log("HP + 1! Current HP: " + activeCharacter.card.hp);

        Debug.Log("Moving on to the End Phase");
        manager.GetComponent<GameManager>().currentPhase = GameManager.TurnPhases.END;
    }

}
