using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelGamble : Panel {

    public override void PanelEffect()
    {
        GameManager managerScript = GameObject.Find("Manager").GetComponent<GameManager>();
        managerScript.CreateFadingSystemText("Roll for a Gamble! Test your luck.");

        if (!managerScript.wasDiceRolled)
        {
            managerScript.DieRollState();
        }
        else
        {
            if (managerScript.diceRoll == 1)
            {
                managerScript.LoseArtifactToOpponent();
            }
            if (managerScript.diceRoll == 6)
            {
                if (GameObject.Find("ArtifactCardPile").GetComponent<ArtifactPile>().cards.Count > 0)
                {
                    managerScript.CreateFadingSystemText("Artifact Card Obtained!");
                    managerScript.characterScript.DrawArtifactCards(1);
                }
                else
                {
                    managerScript.StealArtifactFromOpponent();
                }
            }
            else
            {
                managerScript.CreateFadingSystemText("Nothing happened.");
            }
            managerScript.currentPhase = GameManager.TurnPhases.END;
        }
    }
}
