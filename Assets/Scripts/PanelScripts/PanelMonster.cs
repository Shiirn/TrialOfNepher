using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelMonster : Panel {

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        GameManager managerScript = manager.GetComponent<GameManager>();

        GameObject activeMonster = GameObject.Find("ActiveMonster");
        ActiveFighter activeMonsterScript = activeMonster.GetComponent<ActiveFighter>();        

        activeMonsterScript.CheckIfMonsterIsAlive();

        if (!activeMonsterScript.isMonsterActive)
        {
            activeMonsterScript.FlipNewActiveFighter(0, "Hell Hound Pig", "Monster", "Defender", "Among the most famous demonic entities.", -1, 1, 0, 5, "HellHoundPig");            
            foreach (GameObject prefab in managerScript.activeMonsterSprites)
            {                
                if (prefab.name == activeMonsterScript.card.spriteName)
                {                    
                    GameObject activeMonsterSprite = Instantiate(prefab);
                    managerScript.activeMonsterSprite = activeMonsterSprite;
                    activeMonsterSprite.transform.SetParent(managerScript.canvas.transform);
                }
            }
            
        }

        managerScript.monsterScript = activeMonsterScript;

        managerScript.currentPhase = GameManager.TurnPhases.BATTLE;
    }
}