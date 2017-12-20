using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Stats
{
    public int attack;
    public int defense;
    public int evasion;
    public int hp;
}

public class FighterCard : MonoBehaviour {
        
    enum Nature { Character, Defender, Evader, Special }

    Stats stats;
    string fighterName;
    string type;
    Nature nature;
    string description;
    string cardImgPath;
    bool isAlive;

    void GetDamaged(int damage)
    {
        stats.hp -= damage;

        if (stats.hp <= 0)
            isAlive = false;
    }    
}

public class CharacterCard : FighterCard
{
    int level;
    Stats artifactCounters;
    Stats buffCounters;
    Stats levelCounters;

    void LevelUp(int count) //count is how many times you leveled up in a single go
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

    void LevelDown(int count)
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

    void Buff(Stats buff)
    {
        buffCounters.attack += buff.attack;
        buffCounters.defense += buff.defense;
        buffCounters.evasion += buff.evasion;
    }
}

public class MonsterCard : FighterCard
{
    int id;
}
