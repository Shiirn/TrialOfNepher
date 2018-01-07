using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionScript {

    static GameObject manager = GameObject.Find("Manager");
    static GameManager managerScript = manager.GetComponent<GameManager>();

    public static void ActivateItemFunction(string function)
    {
        string[] separator = new string[] { " " };

        string functionName = function.Split(separator, System.StringSplitOptions.None)[0];
        int modifier = System.Convert.ToInt32(function.Split(separator, System.StringSplitOptions.None)[1]);

        switch (functionName)
        {
            case "heal":
                Heal(modifier);
                break;

            case "hit":
                Hit(modifier);
                break;

            case "luckyroll":
                LuckyRoll(modifier);
                break;

            case "steal":
                Steal(modifier);
                break;

            case "revive":
                Revive();
                break;

            case "fullDebuff":
                FullDebuff(modifier);
                break;

            case "buff":
                Buff(modifier);
                break;

            case "fullBuff":
                FullBuff(modifier);
                break;

            case "smite":
                Smite(modifier);
                break;

            case "movement":
                TemporaryMovementUp(modifier);
                break;

            case "flee":
                Flee();
                break;
        }        
    }

    static void Heal(int modifier)
    {
        managerScript.characterScript.Heal(modifier);
        managerScript.CreateFadingSystemText("You recovered " + modifier + "HP.");
    }

    static void Hit(int modifier)
    {
        managerScript.opponentScript.card.GetDamaged(modifier);
        managerScript.CreateFadingSystemText("The opponent lost " + modifier + "HP.");
    }

    static void LuckyRoll(int modifier)
    {
        managerScript.CreateFadingSystemText("Try a Lucky Roll! " + modifier + " or higher wins.");

        if (!managerScript.wasDiceRolled)
        {
            managerScript.rollingForItem = true;
            managerScript.UseItemButton.gameObject.SetActive(false);
            managerScript.currentInitialSubPhase = GameManager.InitialSubPhases.USINGITEM;
            managerScript.DieRollState();
        }
        else
        {
            if(managerScript.diceRoll >= modifier)
            {
                if(GameObject.Find("ArtifactCardPile").GetComponent<ArtifactPile>().cards.Count > 0)
                {
                    managerScript.CreateFadingSystemText("You succesfully obtained 1 artifact(s)!");
                    managerScript.characterScript.DrawArtifactCards(1);
                    managerScript.ShowArtifacts();
                }
                else
                {
                    managerScript.CreateFadingSystemText("No artifacts left in the pile! You draw one item card.");
                    managerScript.characterScript.DrawItemCards(1);
                    managerScript.ShowItems(managerScript.characterScript);
                }
            }
            else
            {
                managerScript.CreateFadingSystemText("Better luck next time!");
            }

            managerScript.canRollDice = true;
            managerScript.wasDiceRolled = false;
            managerScript.rollingForItem = false;
        }
    }

    static void Steal(int modifier)
    {
        for(int i = 0; i < modifier; i++)
        {
            if(managerScript.opponentScript.artifactsOwned.Count > 0 && managerScript.opponentScript.artifactsOwned.Count != 4)
            {
                managerScript.CreateFadingSystemText("You stole an artifact from the opponent.");
                managerScript.StealArtifactFromOpponent();
                managerScript.ShowArtifacts();
            }
        }
    }

    static void Revive()
    {
        if(!managerScript.characterScript.card.isAlive)
        {
            managerScript.CreateFadingSystemText("You revived, and your HP are fully recovered.");
            managerScript.characterScript.card.isAlive = true;
            managerScript.characterScript.card.hp = managerScript.characterScript.card.GetCurrentStats().maxHp;
        }
    }

    static void FullDebuff(int modifier)
    {
        Stats fullDebuff;

        fullDebuff.attack = -modifier;
        fullDebuff.defense = -modifier;
        fullDebuff.evasion = -modifier;
        fullDebuff.maxHp = 0;

        managerScript.opponentScript.card.Buff(fullDebuff);

        managerScript.characterScript.card.GetDamaged(2);

        if (!managerScript.characterScript.card.isAlive)
        {
            managerScript.CreateFadingSystemText("You lost 2 HP! The opponent is fully debuffed.");
            managerScript.currentPhase = GameManager.TurnPhases.END;
            managerScript.currentEndSubPhase = GameManager.EndSubPhases.DISCARD;
        }
    }

    static void FullBuff(int modifier)
    {
        Stats fullBuff;

        fullBuff.attack = modifier;
        fullBuff.defense = modifier;
        fullBuff.evasion = modifier;
        fullBuff.maxHp = 0;

        managerScript.CreateFadingSystemText("You're fully buffed!");
        managerScript.characterScript.card.Buff(fullBuff);
    }

    static void Buff(int modifier)
    {
        managerScript.currentBattlePhase = GameManager.BattlePhases.USINGITEM;
        managerScript.tempBuffModifier = modifier;
        managerScript.canvasPickStat.enabled = true;
    }

    static void TemporaryMovementUp(int modifier)
    {
        managerScript.CreateFadingSystemText("You'll move one panel further this turn.");
        managerScript.tempMovementBuff += modifier;
    }

    static void Smite(int modifier)
    {
        managerScript.targetCard.GetDamaged(modifier);
        managerScript.DisplayStats(managerScript.targetCard, "enemy");
        managerScript.CreateFadingSystemText("The enemy lost 3 HP.");
        if (!managerScript.targetCard.isAlive)
        {
            managerScript.currentBattlePhase = GameManager.BattlePhases.ENDOFBATTLE;
        }
    }

    static void Flee()
    {
        managerScript.CreateFadingSystemText("Battle avoided!");
        managerScript.isFleeing = true;
        managerScript.currentBattlePhase = GameManager.BattlePhases.ENDOFBATTLE;
    }
}
