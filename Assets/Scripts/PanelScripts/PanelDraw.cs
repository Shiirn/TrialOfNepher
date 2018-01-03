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
        
        managerScript.characterScript.itemsOwned.Add(itemPileScript.Draw());

        Debug.Log("You draw a card.");

        Debug.Log("Moving on to the End Phase");
        managerScript.currentPhase = GameManager.TurnPhases.END;
    }

}
