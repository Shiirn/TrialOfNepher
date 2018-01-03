using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArtifactNature { Equippable, Passive }

public class ArtifactCard
{
    public Stats stats;
    public int id;
    public string artifactName;
    public string type;
    public ArtifactNature nature;
    public string description;
    public string ability;
    public string spriteName;
    public Sprite cardImg;
    public bool isEquipped;

    public ArtifactCard(int _id, string _name, string _type, string _nature, string _description, int attack, int defense, int evasion, int maxHp, string sprite, string _ability)
    {
        id = _id;
        artifactName = _name;
        type = _type;

        switch (_nature)
        {
            case "Equippable":
                nature = ArtifactNature.Equippable;
                break;
            case "Passive":
                nature = ArtifactNature.Passive;
                break;
        }

        description = _description;
        ability = _ability;

        spriteName = "sprite" + sprite;

        stats.attack = attack;
        stats.defense = defense;
        stats.evasion = evasion;
        stats.maxHp = maxHp;
    }
}