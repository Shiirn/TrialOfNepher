using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Fighter
{

    public int boardX;
    public int boardY;
    public GameObject currentPanel;
    public bool isMoving = false;
    public GameObject board;

    string[] stringSeparator = new string[] { " " };

    public CharacterCard card;

    public List<ArtifactCard> artifactsOwned = new List<ArtifactCard>();
    public ArtifactCard equippedArtifact;

    public List<ItemCard> itemsOwned = new List<ItemCard>();

    public void Equip(ArtifactCard artifactCard)
    {
        if (artifactCard.nature == ArtifactNature.Equippable)
        {
            if (equippedArtifact != null)
            {
                Unequip(equippedArtifact);
            }

            equippedArtifact = artifactCard;

            if (artifactCard.ability != "")
            {
                card.abilities.Add(artifactCard.ability);
            }
            card.artifactCounters.attack += artifactCard.stats.attack;
            card.artifactCounters.defense += artifactCard.stats.defense;
            card.artifactCounters.evasion += artifactCard.stats.evasion;
            card.artifactCounters.maxHp += artifactCard.stats.maxHp;
            card.hp += artifactCard.stats.maxHp;
        }
    }

    public void Unequip(ArtifactCard artifactCard)
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
            if (itemsOwned[i].id == id)
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
        foreach (string ability in card.abilities)
        {
            string[] splitString = ability.Split(stringSeparator, System.StringSplitOptions.None);

            if (splitString[0] == "draw")
            {
                cardsToDraw += AbilityScript.Draw(System.Convert.ToInt32(splitString[1]));
            }
        }

        ItemPile itemPileScript = GameObject.Find("ItemCardPile").GetComponent<ItemPile>();

        for (int i = 0; i < cardsToDraw; i++)
        {
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

                if (artifactsOwned[artifactsOwned.Count - 1].nature == ArtifactNature.Passive)
                {
                    card.abilities.Add(artifactsOwned[artifactsOwned.Count - 1].ability);
                }
            }
        }
    }

    public void LoseArtifact(int lostArtifactIndex)
    {
        if (artifactsOwned[lostArtifactIndex].nature == ArtifactNature.Passive)
        {
            for (int i = 0; i < card.abilities.Count; i++)
            {
                if (card.abilities[i] == artifactsOwned[lostArtifactIndex].ability)
                {
                    card.abilities.RemoveAt(i);
                }
            }
            artifactsOwned.RemoveAt(lostArtifactIndex);
        }
        else
        {
            for (int i = 0; i < artifactsOwned.Count; i++)
            {
                if (artifactsOwned[i] == equippedArtifact)
                {
                    Unequip(equippedArtifact);
                }
            }
            artifactsOwned.RemoveAt(lostArtifactIndex);
        }
    }

    public void Heal(int healAmount)
    {
        foreach (string ability in card.abilities)
        {
            string[] splitString = ability.Split(stringSeparator, System.StringSplitOptions.None);

            if (splitString[0] == "regen")
            {
                healAmount += AbilityScript.Regen(System.Convert.ToInt32(splitString[1]));
            }
        }

        card.hp += healAmount;

        if (card.hp > card.GetCurrentStats().maxHp)
        {
            card.hp = card.GetCurrentStats().maxHp;
        }
    }
}