using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FighterCardParser {

    public static string[] ParseMonsterCard(TextAsset monsterCsv, int id)
    {
        string[] stringSeparator = new string[] { "\r\n" };
        string[] monsterStrings = monsterCsv.text.Split(stringSeparator, System.StringSplitOptions.None);
        string[] ret = new string[] { };

        foreach(string monsterString in monsterStrings)
        {
            if(monsterString[0].ToString() == id.ToString())
            {
                ret = monsterString.Split(',');
                Debug.Log(ret);
                break;
            }
        }

        return ret;
    }
}
