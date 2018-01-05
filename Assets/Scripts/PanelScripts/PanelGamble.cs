using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelGamble : Panel {

    public override void PanelEffect()
    {
        GameManager managerScript = GameObject.Find("Manager").GetComponent<GameManager>();

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
                    managerScript.characterScript.DrawArtifactCards(1);
                }
                else
                {
                    managerScript.StealArtifactFromOpponent();
                }
            }
            managerScript.currentPhase = GameManager.TurnPhases.END;
        }
    }
}
