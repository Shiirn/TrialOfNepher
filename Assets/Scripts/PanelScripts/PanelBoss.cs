using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBoss : Panel {

    public TextAsset monsterCsv;

    public override void PanelEffect()
    {
        GameObject manager = GameObject.Find("Manager");
        GameManager managerScript = manager.GetComponent<GameManager>();

        GameObject activePlayerCard;
        GameObject activeBoss = GameObject.Find("ActiveBoss");
        ActiveFighter activeBossScript = activeBoss.GetComponent<ActiveFighter>();

        activeBossScript.CheckIfBossIsAlive();

        if (!activeBossScript.isBossActive)
        {
            activeBossScript.FlipNewActiveBoss(FighterCardParser.ParseMonsterCard(monsterCsv, Random.Range(5, 8)));

            foreach (GameObject prefab in managerScript.BossSprites)
            {
                if (prefab.name == activeBossScript.bossCard.spriteName)
                {
                    GameObject activeBossSprite = Instantiate(prefab);
                    managerScript.SetCard("boss", activeBossSprite);

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

        managerScript.bossScript = activeBossScript;

        manager.GetComponent<GameManager>().currentPhase = GameManager.TurnPhases.BATTLE;
    }

}
