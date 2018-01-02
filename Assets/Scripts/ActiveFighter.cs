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
