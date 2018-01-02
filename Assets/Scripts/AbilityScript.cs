using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AbilityScript {

    static GameObject manager = GameObject.Find("Manager");
    static GameManager managerScript = manager.GetComponent<GameManager>();

    public static void ActivateArtifactAbility(string ability)
    {
        string[] separator = new string[] { " " };

        string abilityName = ability.Split(separator, System.StringSplitOptions.None)[0];
        int modifier = System.Convert.ToInt32(ability.Split(separator, System.StringSplitOptions.None)[1]);

        
        switch (abilityName)
        {
            case "thornmail":
                Thornmail(modifier);
                break;

            case "draw":
                //fai pescare modifier carte al giocatore attivo
                break;

            case "regen":
                Regen(modifier);
                break;

            case "movement":
                MovementBoost(modifier);
                break;
        }
    }

    static void Thornmail(int modifier)
    {
        managerScript.attackerCard.GetDamaged(modifier);
    }

    static void Regen(int modifier)
    {
        managerScript.characterScript.card.hp += modifier;
        if (managerScript.characterScript.card.hp > managerScript.characterScript.card.stats.maxHp)
        {
            managerScript.characterScript.card.hp = managerScript.characterScript.card.stats.maxHp;
        }
    }

    static void MovementBoost(int modifier)
    {
        managerScript.diceRoll += modifier;
    }
}

