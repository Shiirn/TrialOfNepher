using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelShrine : Panel {


    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        GameManager managerScript = manager.GetComponent<GameManager>();

        Character activeCharacter = managerScript.characters[managerScript.activePlayer].GetComponent<Character>();
        
        int shrineDiceRoll = Random.Range(1, 7);

        Stats buff;
        buff.attack = 0;
        buff.defense = 0;
        buff.evasion = 0;
        buff.maxHp = 0;

        switch(shrineDiceRoll)
        {
            case 1:
                buff.attack--;
                break;
            case 2:
                buff.defense--;
                break;
            case 3:
                buff.evasion--;
                break;
            case 4:
                buff.attack++;
                break;
            case 5:
                buff.defense++;
                break;
            case 6:
                buff.evasion++;
                break;
        }

        activeCharacter.card.Buff(buff);

        //DEBUGGYYYY
        Debug.Log("Applied buff " + buff.attack + "/" + buff.defense + "/" + buff.evasion + " to character." +
            "Buff counters are now " + activeCharacter.card.buffCounters.attack + "/" + activeCharacter.card.buffCounters.defense + "/" + activeCharacter.card.buffCounters.evasion);

        Debug.Log("Moving on to the End Phase");
        manager.GetComponent<GameManager>().currentPhase = GameManager.TurnPhases.END;
    }

}
