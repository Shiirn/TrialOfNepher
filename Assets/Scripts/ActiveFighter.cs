using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveFighter : Fighter {

    public bool isMonsterActive = false;
    public FighterCard card;
    

    public void FlipNewActiveFighter(int _id, string _name, string _type, string _nature, string _description, int attack, int defense, int evasion, int maxHp, string sprite)
    {
        card = new MonsterCard(0, "Hell Hound Pig", "Monster", "Defender", "Among the most famous demonic entities.", -1, 1, 0, 5, "HellHoundPig");
    }

    public void CheckIfMonsterIsAlive()
    {
        if(card != null)
            if (!card.isAlive)
                isMonsterActive = false;
    }

}
