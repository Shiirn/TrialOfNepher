using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum Nature { Character, Defender, Evader, Special }

public struct Stats
{
    public int attack;
    public int defense;
    public int evasion;
    public int maxHp;
}

public class FighterCard
{

    public Stats stats;
    public int hp;
    public int id;
    public string fighterName;
    public string type;
    public Nature nature;
    public string description;
    public string spriteName;
    public Sprite cardImg;
    public bool isAlive;
    public ArtifactCard equippedArtifact;
    public List<ArtifactCard> artifactsOwned = new List<ArtifactCard>();
    public List<string> abilities = new List<string>();

    public virtual void GetDamaged(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            isAlive = false;
            hp = 0;
        }
    }
}

public class CharacterCard : FighterCard
{
    public int level;
    public Stats artifactCounters;
    public Stats buffCounters;
    public Stats levelCounters;

    public void LevelUp(int count) //count is how many times you leveled up in a single go
    {
        for (int i = 0; i < count; i++)
        {
            if (level < 3)
            {
                level++;
                switch (level)
                {
                    case 1:
                        levelCounters.evasion++;
                        break;
                    case 2:
                        levelCounters.defense++;
                        break;
                    case 3:
                        levelCounters.attack++;
                        break;
                }
            }
        }
    }

    public void LevelDown(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (level > 0)
            {
                level--;
                switch (level)
                {
                    case 0:
                        levelCounters.evasion--;
                        break;
                    case 1:
                        levelCounters.defense--;
                        break;
                    case 2:
                        levelCounters.attack--;
                        break;
                }
            }
        }
    }


    public override void GetDamaged(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            isAlive = false;
            hp = 0;
            if(level > 0)
            LevelDown(1);
        }
    }

    void Equip(ArtifactCard artifactCard)
    {
        if(equippedArtifact != null)
        {
            Unequip(equippedArtifact);
        }

        abilities.Add(artifactCard.ability);
        artifactCounters.attack += artifactCard.stats.attack;
        artifactCounters.defense += artifactCard.stats.defense;
        artifactCounters.evasion += artifactCard.stats.evasion;
        artifactCounters.maxHp += artifactCard.stats.maxHp;
        hp += artifactCard.stats.maxHp;
    }

    void Unequip(ArtifactCard artifactCard)
    {
        for(int i = 0; i < abilities.Count; i++)
        {
            if(abilities[i] == artifactCard.ability)
            {
                abilities.RemoveAt(i);
            }

            artifactCounters.attack -= artifactCard.stats.attack;
            artifactCounters.defense -= artifactCard.stats.defense;
            artifactCounters.evasion -= artifactCard.stats.evasion;
            artifactCounters.maxHp -= artifactCard.stats.maxHp;
            GetDamaged(artifactCard.stats.maxHp);
        }
    }

    public CharacterCard(int _id)
    {
        id = _id;

        switch (id)
        {
            case 0:
                fighterName = "White Hood";
                break;
            case 1:
                fighterName = "Black Hood";
                break;
        }

        nature = Nature.Character;
        description = "This is you.";
        isAlive = true;
        stats.attack = 0;
        stats.defense = 0;
        stats.evasion = 0;
        stats.maxHp = 5;
        hp = stats.maxHp;
        levelCounters.attack = 0;
        levelCounters.defense = 0;
        levelCounters.evasion = 0;
    }

    new public string ToString()
    {
        return fighterName + " " + id + " " + nature + " " + description + " " + isAlive + " " + stats.attack + " " + stats.maxHp;
    }

    public void Buff(Stats buff)
    {
        buffCounters.attack += buff.attack;
        buffCounters.defense += buff.defense;
        buffCounters.evasion += buff.evasion;
    }

    public void ResetBuffs()
    {
        buffCounters.attack = 0;
        buffCounters.defense = 0;
        buffCounters.evasion = 0;
    }
}

public class MonsterCard : FighterCard
{
    public MonsterCard(int _id, string _name, string _type, string _nature, string _description, int attack, int defense, int evasion, int maxHp, string sprite)
    {
        id = _id;
        fighterName = _name;
        type = _type;
        switch(_nature)
        {
            case "Defender":
                nature = Nature.Defender;
                break;
            case "Evader":
                nature = Nature.Evader;
                break;
            case "Special":
                nature = Nature.Special;
                break;
        }
        description = _description;
        stats.attack = attack;
        stats.defense = defense;
        stats.evasion = evasion;
        stats.maxHp = maxHp;
        hp = maxHp;

        isAlive = true;

        spriteName = "sprite" + sprite;

        cardImg = Resources.Load(spriteName) as Sprite;
    }
}