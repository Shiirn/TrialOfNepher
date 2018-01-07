using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelShrine : Panel {


    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        GameManager managerScript = manager.GetComponent<GameManager>();

        managerScript.CreateFadingSystemText("Roll for a Buff/Debuff!");

        Character activeCharacter = managerScript.characters[managerScript.activePlayer].GetComponent<Character>();

        if (!managerScript.wasDiceRolled)
        {
            managerScript.DieRollState();
        }
        else
        {
            Stats buff;
            buff.attack = 0;
            buff.defense = 0;
            buff.evasion = 0;
            buff.maxHp = 0;

            switch (managerScript.diceRoll)
            {
                case 1:
                    buff.attack--;
                    managerScript.CreateFadingSystemText("Debuff: -1 Attack.");
                    break;
                case 2:
                    buff.defense--;
                    managerScript.CreateFadingSystemText("Debuff: -1 Defense.");
                    break;
                case 3:
                    buff.evasion--;
                    managerScript.CreateFadingSystemText("Debuff: -1 Evasion.");
                    break;
                case 4:
                    buff.attack++;
                    managerScript.CreateFadingSystemText("Buff: +1 Attack!");
                    break;
                case 5:
                    buff.defense++;
                    managerScript.CreateFadingSystemText("Buff: +1 Defense!");
                    break;
                case 6:
                    buff.evasion++;
                    managerScript.CreateFadingSystemText("Buff: +1 Evasion!");
                    break;
            }

            activeCharacter.card.Buff(buff);

            manager.GetComponent<GameManager>().currentPhase = GameManager.TurnPhases.END;
        }
    }

}
