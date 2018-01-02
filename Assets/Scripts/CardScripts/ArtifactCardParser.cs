using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArtifactCardParser
{

    public static string[] ParseArtifactCard(TextAsset artifactCsv, int id)
    {
        string[] stringSeparator = new string[] { "\r\n" };
        string[] artifactStrings = artifactCsv.text.Split(stringSeparator, System.StringSplitOptions.None);
        string[] artifactStringToReturn = new string[] { };

        foreach (string artifactString in artifactStrings)
        {
            if (artifactString[0].ToString() == id.ToString())
            {
                artifactStringToReturn = artifactString.Split(',');
                break;
            }
        }

        return artifactStringToReturn;
    }
}