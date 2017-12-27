using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelHome : Panel {

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        GameManager managerScript = manager.GetComponent<GameManager>();

        CharacterCard activeCharacterCard = managerScript.characters[managerScript.activePlayer].GetComponent<Character>().GetComponent<CharacterCard>();

        activeCharacterCard.hp += 2;
        if (activeCharacterCard.hp > activeCharacterCard.stats.maxHp)
            activeCharacterCard.hp = activeCharacterCard.stats.maxHp;
    }

}
