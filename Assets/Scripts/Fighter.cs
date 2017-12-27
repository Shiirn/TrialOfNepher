using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Stats
{
    public int attack;
    public int defense;
    public int evasion;
    public int maxHp;
}

public class FighterCard {
        
    public enum Nature { Character, Defender, Evader, Special }

    public Stats stats;
    public int hp;
    public int id;
    public string fighterName;
    public string type;
    public Nature nature;
    public string description;
    public Texture2D cardImg;
    public bool isAlive;

    void GetDamaged(int damage)
    {
        hp -= damage;

        if (hp <= 0)
            isAlive = false;
    }    
}

public class CharacterCard : FighterCard
{
    public int level;
    public Stats artifactCounters;
    public Stats buffCounters;
    Stats levelCounters;

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

    void Equip()
    {

    }

    void Unequip()
    {

    }

    public CharacterCard(int _id)
    {
        id = _id;

        switch(id)
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
    }

    public string ToString()
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
}
