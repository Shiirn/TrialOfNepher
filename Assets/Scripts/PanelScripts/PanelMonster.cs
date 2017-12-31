using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelMonster : Panel {

    public TextAsset monsterCsv;

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        GameManager managerScript = manager.GetComponent<GameManager>();

        GameObject activePlayerCard;
        GameObject activeMonster = GameObject.Find("ActiveMonster");
        ActiveFighter activeMonsterScript = activeMonster.GetComponent<ActiveFighter>();        

        activeMonsterScript.CheckIfMonsterIsAlive();

        if (!activeMonsterScript.isMonsterActive)
        {
            activeMonsterScript.FlipNewActiveMonster(FighterCardParser.ParseMonsterCard(monsterCsv, Random.Range(0, 5)));

            foreach (GameObject prefab in managerScript.MonsterSprites)
            {                
                if (prefab.name == activeMonsterScript.monsterCard.spriteName)
                {                    
                    GameObject activeMonsterSprite = Instantiate(prefab);
                    managerScript.SetCard("monster", activeMonsterSprite);

                    if (managerScript.characters[managerScript.activePlayer].GetComponent<Character>().card.fighterName == "White Hood")
                    {
                        activePlayerCard = managerScript.whiteHoodSprite;
                        managerScript.SetCard("activePlayer", activePlayerCard);
                    }
                    else
                    {
                        activePlayerCard = managerScript.blackHoodSprite;
                        managerScript.SetCard("activePlayer", activePlayerCard);
                    }                   
                }
            }
            
        }

        managerScript.monsterScript = activeMonsterScript;

        managerScript.currentPhase = GameManager.TurnPhases.BATTLE;
    }
}