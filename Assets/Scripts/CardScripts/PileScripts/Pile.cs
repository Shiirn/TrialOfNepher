using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pile : MonoBehaviour {

    public TextAsset csv;

    protected string[] stringSeparator = new string[] { "\r\n" };
    protected string[] csvStrings;

    public List<int> cards = new List<int>();
    public List<int> discardedCards = new List<int>();

    public void Shuffle()
    {
        int temp;

        for (int i = 0; i < cards.Count * 5; i++)
        {
            int randomIndex1 = Random.Range(0, cards.Count);
            int randomIndex2 = Random.Range(0, cards.Count);

            temp = cards[randomIndex1];
            cards[randomIndex1] = cards[randomIndex2];
            cards[randomIndex2] = temp;
        }
    }

    public void Discard(int id)
    {
        discardedCards.Add(id);

        if(cards.Count == 0)
        {
            ResetDiscardedCards();
        }
    }

    public virtual void ResetDiscardedCards()
    {
        for (int i = 0; i < discardedCards.Count; i++)
        {
            cards.Add(discardedCards[i]);
        }

        discardedCards.Clear();
        discardedCards = new List<int>();

        Shuffle();
    }

    // Use this for initialization
    void Start () {
        csvStrings = csv.text.Split(stringSeparator, System.StringSplitOptions.None);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
