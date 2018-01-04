using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactPile : Pile {
    
	// Use this for initialization
	void Start () {

        csvStrings = csv.text.Split(stringSeparator, System.StringSplitOptions.None);

        GeneratePile();
    }

    // Update is called once per frame
    void Update () {
		
	}

    void GeneratePile()
    {
        for (int i = 1; i < csvStrings.Length; i++)
        {
            cards.Add(i - 1);
        }
        Shuffle();
    }

    public ArtifactCard Draw()
    {
        int drawnCard = cards[cards.Count - 1];

        cards.RemoveAt(cards.Count - 1);

        string stringToDraw = csvStrings[drawnCard + 1];

        string[] parsedString = stringToDraw.Split(',');

        Debug.Log(System.Convert.ToInt32(parsedString[0]) + " " +
                                parsedString[1] + " " +
                                parsedString[2] + " " +
                                parsedString[3] + " " +
                                parsedString[4] + " " +
                                System.Convert.ToInt32(parsedString[5]) + " " +
                                System.Convert.ToInt32(parsedString[6]) + " " +
                                System.Convert.ToInt32(parsedString[7]) + " " +
                                System.Convert.ToInt32(parsedString[8]) + " " +
                                parsedString[9] + " " +
                                parsedString[10]);

        return new ArtifactCard(System.Convert.ToInt32(parsedString[0]),
                                parsedString[1],
                                parsedString[2],
                                parsedString[3],
                                parsedString[4],
                                System.Convert.ToInt32(parsedString[5]),
                                System.Convert.ToInt32(parsedString[6]),
                                System.Convert.ToInt32(parsedString[7]),
                                System.Convert.ToInt32(parsedString[8]),
                                parsedString[9],
                                parsedString[10]);        
    }
}
