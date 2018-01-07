using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelCardLoss : Panel {

    public override void PanelEffect()
    {
        GameManager managerScript = GameObject.Find("Manager").GetComponent<GameManager>();

        if (managerScript.characterScript.itemsOwned.Count > 0)
        {
            managerScript.CreateFadingSystemText("You dropped one item card.");
            managerScript.characterScript.DiscardCardAt(Random.Range(0, managerScript.characterScript.itemsOwned.Count));
        }

        managerScript.currentPhase = GameManager.TurnPhases.END;
    }
}
