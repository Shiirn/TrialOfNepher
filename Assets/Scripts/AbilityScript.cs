using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AbilityScript
{

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
                Draw(modifier);
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
        if (managerScript.attackerCard.hp > 1)
        {
            managerScript.attackerCard.GetDamaged(modifier);
        }
    }

    public static int Regen(int modifier)
    {
        return modifier;
    }

    static void MovementBoost(int modifier)
    {
        managerScript.diceRoll += modifier;
    }

    public static int Draw(int modifier)
    {
        return modifier;
    }
}