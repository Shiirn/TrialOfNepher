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
                break;

            case "flee":
                break;
        }        
    }

    static void Heal(int modifier)
    {
        managerScript.characterScript.Heal(modifier);
    }

    static void Hit(int modifier)
    {
        managerScript.opponentScript.card.GetDamaged(modifier);
    }

    static void LuckyRoll(int modifier)
    {
        if (!managerScript.wasDiceRolled)
        {
            managerScript.rollingForItem = true;
            managerScript.currentInitialSubPhase = GameManager.InitialSubPhases.USINGITEM;
            managerScript.DieRollState();
        }
        else
        {
            if(managerScript.diceRoll >= 4)
            {
                if(GameObject.Find("ArtifactCardPile").GetComponent<ArtifactPile>().cards.Count > 0)
                {
                    managerScript.characterScript.DrawArtifactCards(1);
                    managerScript.ShowArtifacts();
                }
                else
                {
                    managerScript.characterScript.DrawItemCards(1);
                    managerScript.ShowItems();
                }
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
            if(managerScript.opponentScript.artifactsOwned.Count > 0)
            {
                managerScript.StealArtifactFromOpponent();
                managerScript.ShowArtifacts();
            }
        }
    }

    static void Revive()
    {
        if(!managerScript.characterScript.card.isAlive)
        {
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
    }

    static void FullBuff(int modifier)
    {
        Stats fullBuff;

        fullBuff.attack = modifier;
        fullBuff.defense = modifier;
        fullBuff.evasion = modifier;
        fullBuff.maxHp = 0;

        managerScript.opponentScript.card.Buff(fullBuff);
    }

    static void Buff(int modifier)
    {
        if (managerScript.pickedStat != "")
        {
            Stats buff;

            buff.attack = 0;
            buff.defense = 0;
            buff.evasion = 0;
            buff.maxHp = 0;

            switch (managerScript.pickedStat)
            {
                case "attack":
                    buff.attack = modifier;
                    break;

                case "defense":
                    buff.defense = modifier;
                    break;

                case "evasion":
                    buff.evasion = modifier;
                    break;
            }

            managerScript.opponentScript.card.Buff(buff);
            managerScript.pickedStat = "";
        }
    }
}
