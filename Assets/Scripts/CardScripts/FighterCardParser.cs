using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FighterCardParser {

    public static string[] ParseMonsterCard(TextAsset monsterCsv, int id)
    {
        string[] stringSeparator = new string[] { "\r\n" };
        string[] monsterStrings = monsterCsv.text.Split(stringSeparator, System.StringSplitOptions.None);
        string[] monsterStringToReturn = new string[] { };

        foreach(string monsterString in monsterStrings)
        {
            if(monsterString[0].ToString() == id.ToString())
            {
                monsterStringToReturn = monsterString.Split(',');
                break;
            }
        }

        return monsterStringToReturn;
    }
}
