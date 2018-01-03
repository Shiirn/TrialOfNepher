using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPile : Pile {

    public TextAsset itemDeckCsv;
    string[] itemDeckStrings;

    // Use this for initialization
    void Start () {
        
        csvStrings = csv.text.Split(stringSeparator, System.StringSplitOptions.None);
        itemDeckStrings = itemDeckCsv.text.Split(stringSeparator, System.StringSplitOptions.None);

        GeneratePile();
    }

    void GeneratePile()
    {
        for (int i = 1; i < itemDeckStrings.Length; i++)
        {
            string[] subStrings = itemDeckStrings[i].Split(',');
            for (int j = 0; j < System.Convert.ToInt32(subStrings[1]); j++)
            {
                cards.Add(System.Convert.ToInt32(subStrings[0]));
            }
        }
        Shuffle();
    }

    public ItemCard Draw()
    {
        if (cards.Count <= 0)
        {
            ResetDiscardedCards();
        }

        int drawnCard = cards[cards.Count - 1];

        cards.RemoveAt(cards.Count - 1);

        string stringToDraw = csvStrings[drawnCard + 1];
        string[] parsedString = stringToDraw.Split(',');

        return new ItemCard(System.Convert.ToInt32(parsedString[0]),
                                parsedString[1],
                                parsedString[2],
                                parsedString[3],
                                parsedString[4],
                                parsedString[5],
                                parsedString[6]);        
    }
}
