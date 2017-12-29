using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveFighter : Fighter {

    public bool isMonsterActive = false;
    public bool isBossActive = false;
    public FighterCard monsterCard;
    public FighterCard bossCard;

    public void FlipNewActiveMonster(int _id, string _name, string _type, string _nature, string _description, int attack, int defense, int evasion, int maxHp, string sprite)
    {
        monsterCard = new MonsterCard(0, "Hell Hound Pig", "Monster", "Defender", "Among the most famous demonic entities.", -1, 1, 0, 5, "HellHoundPig");
        isMonsterActive = true;
    }

    public void FlipNewActiveBoss(int _id, string _name, string _type, string _nature, string _description, int attack, int defense, int evasion, int maxHp, string sprite)
    {
        bossCard = new MonsterCard(5, "Mummytaur", "MidBoss", "Defender", "blaaaaaaaaaaarhg", 2, 1, 0, 7, "Mummytaur");
        isBossActive = true;
    }


    public void CheckIfMonsterIsAlive()
    {
        if(monsterCard != null)
            if (!monsterCard.isAlive)
                isMonsterActive = false;
    }

    public void CheckIfBossIsAlive()
    {
        if (bossCard != null)
            if (!bossCard.isAlive)
                isBossActive = false;
    }

}
