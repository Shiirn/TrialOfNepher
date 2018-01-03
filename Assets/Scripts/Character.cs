using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Fighter {

    public int boardX;
    public int boardY;
    public GameObject currentPanel;
    public bool isMoving = false;
    public GameObject board;

    public CharacterCard card;

    public List<ArtifactCard> artifactsOwned = new List<ArtifactCard>();
    public ArtifactCard equippedArtifact;

    public List<ItemCard> itemsOwned = new List<ItemCard>();

    void Equip(ArtifactCard artifactCard)
    {
        if (equippedArtifact != null)
        {
            Unequip(equippedArtifact);
        }

        card.abilities.Add(artifactCard.ability);
        card.artifactCounters.attack += artifactCard.stats.attack;
        card.artifactCounters.defense += artifactCard.stats.defense;
        card.artifactCounters.evasion += artifactCard.stats.evasion;
        card.artifactCounters.maxHp += artifactCard.stats.maxHp;
        card.hp += artifactCard.stats.maxHp;
    }

    void Unequip(ArtifactCard artifactCard)
    {
        for (int i = 0; i < card.abilities.Count; i++)
        {
            if (card.abilities[i] == artifactCard.ability)
            {
                card.abilities.RemoveAt(i);
            }

            card.artifactCounters.attack -= artifactCard.stats.attack;
            card.artifactCounters.defense -= artifactCard.stats.defense;
            card.artifactCounters.evasion -= artifactCard.stats.evasion;
            card.artifactCounters.maxHp -= artifactCard.stats.maxHp;
            card.GetDamaged(artifactCard.stats.maxHp);
        }
    }

    public void SetCurrentPanel(GameObject _currentPanel)
    {
        currentPanel = _currentPanel;
    }

    public void DiscardCard(int id)
    {
        int indexToRemove = 1337;

        for (int i = 0; i < itemsOwned.Count; i++)
        {
            if(itemsOwned[i].id == id)
            {
                indexToRemove = i;
                break;
            }
        }

        if (indexToRemove != 1337)
        {
            itemsOwned.RemoveAt(indexToRemove);
            GameObject.Find("ItemCardPile").GetComponent<ItemPile>().Discard(id);
        }
    }

    public void DiscardCardAt(int position)
    {
        if (position < itemsOwned.Count)
        {
            GameObject.Find("ItemCardPile").GetComponent<ItemPile>().Discard(itemsOwned[position].id);
            itemsOwned.RemoveAt(position);
        }
    }

    public void DrawItemCards(int cardsToDraw)
    {
        Debug.Log("drawing " + cardsToDraw);

        string[] stringSeparator = new string[] { " " };

        foreach (string ability in card.abilities)
        {
            string[] splitString = ability.Split(stringSeparator, System.StringSplitOptions.None);

            if (splitString[0] == "draw")
            {
                cardsToDraw += System.Convert.ToInt32(splitString[1]);
            }
        }

        Debug.Log("still drawing " + cardsToDraw);

        ItemPile itemPileScript = GameObject.Find("ItemCardPile").GetComponent<ItemPile>();

        for(int i = 0; i < cardsToDraw; i++)
        {
            Debug.Log("Drawing one");
            itemsOwned.Add(itemPileScript.Draw());
        }
    }

    public void DrawArtifactCards(int numberOfArtifacts)
    {
        ArtifactPile artifactPileScript = GameObject.Find("ArtifactCardPile").GetComponent<ArtifactPile>();

        for (int i = 0; i < numberOfArtifacts; i++)
        {
            if (artifactPileScript.cards.Count > 0)
            {
                artifactsOwned.Add(artifactPileScript.Draw());
            }
        }
    }
}