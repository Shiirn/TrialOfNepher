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
            activeMonsterScript.FlipNewActiveMonster(0, "Hell Hound Pig", "Monster", "Defender", "Among the most famous demonic entities.", -1, 1, 0, 5, "HellHoundPig");            
            foreach (GameObject prefab in managerScript.MonsterSprites)
            {                
                if (prefab.name == activeMonsterScript.monsterCard.spriteName)
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