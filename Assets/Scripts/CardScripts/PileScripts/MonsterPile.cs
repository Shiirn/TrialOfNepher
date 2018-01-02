using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPile : Pile {

    GameObject activeMonster;
    ActiveFighter activeMonsterScript;

	// Use this for initialization
	void Start () {

        activeMonster = GameObject.Find("ActiveMonster");
        activeMonsterScript = activeMonster.GetComponent<ActiveFighter>();

        csvStrings = csv.text.Split(stringSeparator, System.StringSplitOptions.None);

        //There are 5 normal monsters
        for (int i = 1; i < 6; i++)
        {
            cards.Add(i - 1);
        }

        Shuffle();

        //DEBUGGING
        foreach (int card in cards)
        {
            Debug.Log(card);
        }
        //DEBUGGING
    }

    // Update is called once per frame
    void Update () {
		
	}

    public MonsterCard FlipNewActiveMonster()
    {
        int drawnCard = cards[cards.Count - 1];

        cards.RemoveAt(cards.Count - 1);

        string stringToFlip = csvStrings[drawnCard + 1];
        string[] parsedString = stringToFlip.Split(',');

        MonsterCard flippedMonsterCard = new MonsterCard(System.Convert.ToInt32(parsedString[0]),
                                                        parsedString[1],
                                                        parsedString[2],
                                                        parsedString[3],
                                                        parsedString[4],
                                                        System.Convert.ToInt32(parsedString[5]),
                                                        System.Convert.ToInt32(parsedString[6]),
                                                        System.Convert.ToInt32(parsedString[7]),
                                                        System.Convert.ToInt32(parsedString[8]),
                                                        parsedString[9]);
        activeMonsterScript.monsterCard = flippedMonsterCard;

        return flippedMonsterCard;
    }
}
