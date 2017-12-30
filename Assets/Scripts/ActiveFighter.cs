using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ActiveFighter : Fighter {

    public bool isMonsterActive = false;
    public bool isBossActive = false;
    public FighterCard monsterCard;
    public FighterCard bossCard;

    public void FlipNewActiveMonster(string[] monsterString)
    {
        monsterCard = new MonsterCard(Convert.ToInt32(monsterString[0]),
                                        monsterString[1], 
                                        monsterString[2], 
                                        monsterString[3], 
                                        monsterString[4], 
                                        Convert.ToInt32(monsterString[5]), 
                                        Convert.ToInt32(monsterString[6]), 
                                        Convert.ToInt32(monsterString[7]), 
                                        Convert.ToInt32(monsterString[8]), 
                                        monsterString[9]);
        isMonsterActive = true;
    }

    public void FlipNewActiveBoss(string[] monsterString)
    {
        bossCard = new MonsterCard(Convert.ToInt32(monsterString[0]),
                                        monsterString[1],
                                        monsterString[2],
                                        monsterString[3],
                                        monsterString[4],
                                        Convert.ToInt32(monsterString[5]),
                                        Convert.ToInt32(monsterString[6]),
                                        Convert.ToInt32(monsterString[7]),
                                        Convert.ToInt32(monsterString[8]),
                                        monsterString[9]);
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
