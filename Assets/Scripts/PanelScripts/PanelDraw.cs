using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelDraw : Panel {

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        GameManager managerScript = manager.GetComponent<GameManager>();

        GameObject itemPile = GameObject.Find("ItemCardPile");
        ItemPile itemPileScript = itemPile.GetComponent<ItemPile>();

        managerScript.CreateFadingSystemText("You obtained one Item Card.");
        managerScript.characterScript.DrawItemCards(1);

        managerScript.currentPhase = GameManager.TurnPhases.END;
    }

}
