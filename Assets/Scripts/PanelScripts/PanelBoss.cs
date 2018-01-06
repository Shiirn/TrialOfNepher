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

        GameObject activeFinalBoss = GameObject.Find("ActiveFinalBoss");
        ActiveFighter activeFinalBossScript = activeFinalBoss.GetComponent<ActiveFighter>();

        GameObject bossCardPile = GameObject.Find("BossCardPile");
        BossPile bossCardPileScript = bossCardPile.GetComponent<BossPile>();

        if (managerScript.characterScript.artifactsOwned.Count >= 4 && !managerScript.wasFinalBossFightIgnored)
        {
            managerScript.isChoosingToFightFinalBoss = true;
            StartCoroutine(managerScript.FightFinalBossChoice());
        }

        if (!managerScript.fightFinalBoss && !managerScript.canvasFightChoice.isActiveAndEnabled)
        {
            if (activeBossScript.bossCard == null || activeBossScript.bossCard.isAlive == false)
            {
                bossCardPileScript.FlipNewActiveBoss();

                foreach (GameObject prefab in managerScript.bossSprites)
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
            managerScript.wasFinalBossFightIgnored = false;
            manager.GetComponent<GameManager>().currentPhase = GameManager.TurnPhases.BATTLE;
        }
        else if(managerScript.fightFinalBoss && !managerScript.canvasFightChoice.isActiveAndEnabled)
        {
            if (activeFinalBossScript.finalBossCard == null)
            {
                string[] stringSeparator = new string[] { "\r\n" };
                string[] csvStrings = monsterCsv.text.Split(stringSeparator, System.StringSplitOptions.None);

                for (int i = 1; i < csvStrings.Length; i++)
                {
                    string[] parsedString = csvStrings[i].Split(',');


                    if (System.Convert.ToInt32(parsedString[0]) == 8)
                    {
                        MonsterCard flippedFinalBossCard = new MonsterCard(System.Convert.ToInt32(parsedString[0]),
                                                                    parsedString[1],
                                                                    parsedString[2],
                                                                    parsedString[3],
                                                                    parsedString[4],
                                                                    System.Convert.ToInt32(parsedString[5]),
                                                                    System.Convert.ToInt32(parsedString[6]),
                                                                    System.Convert.ToInt32(parsedString[7]),
                                                                    System.Convert.ToInt32(parsedString[8]),
                                                                    parsedString[9]);

                        activeFinalBossScript.finalBossCard = flippedFinalBossCard;
                        break;
                    }
                }
            }
            else
            {
                GameObject activeFinalBossSprite = Instantiate(managerScript.finalBossSprites[0]);
                managerScript.SetCard("finalBoss", activeFinalBossSprite);

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

                managerScript.finalBossScript = activeFinalBossScript;
                managerScript.wasFinalBossFightIgnored = false;
                managerScript.currentPhase = GameManager.TurnPhases.BATTLE;
                managerScript.currentBattlePhase = GameManager.BattlePhases.INITIAL;
            }
        }
    }
}
