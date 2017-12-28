using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveFighter : Fighter {

    public bool isMonsterActive = false;
    public FighterCard card;
    

    public void FlipNewActiveFighter(int id)
    {
        card = new MonsterCard(id);
        Debug.Log(card.nature);
    }

    public void CheckIfMonsterIsAlive()
    {
        if(card != null)
            if (!card.isAlive)
                isMonsterActive = false;
    }

}
