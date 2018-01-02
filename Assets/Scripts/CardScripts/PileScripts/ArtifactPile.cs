using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactPile : MonoBehaviour {

    public TextAsset artifactCsv;

    string[] stringSeparator = new string[] { "\r\n" };
    string[] artifactStrings;

    public List<int> cards = new List<int>();



	// Use this for initialization
	void Start () {

        artifactStrings = artifactCsv.text.Split(stringSeparator, System.StringSplitOptions.None);

        for (int i = 1; i < artifactStrings.Length; i++)
        {
            cards.Add(i-1);
        }

        Shuffle();

        foreach(int card in cards)
        {
            Debug.Log(card);
        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void Shuffle()
    {
        int temp;

        for(int i = 0; i < cards.Count * 5; i++)
        {
            int randomIndex1 = Random.Range(0, cards.Count);
            int randomIndex2 = Random.Range(0, cards.Count);

            temp = cards[randomIndex1];
            cards[randomIndex1] = cards[randomIndex2];
            cards[randomIndex2] = temp;
        }
    }

    public ArtifactCard Draw()
    {
        int drawnCard = cards[cards.Count - 1];

        cards.RemoveAt(cards.Count - 1);

        string stringToDraw = artifactStrings[drawnCard + 1];

        string[] parsedString = stringToDraw.Split(',');

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
