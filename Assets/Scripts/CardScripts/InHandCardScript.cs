using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InHandCardScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public int inHandIndex;
    Color cardColor;
    GameManager managerScript;
    GameObject itemCanvas;
    GameObject artifactCanvas;    

    private void Start()
    {
        managerScript = GameObject.Find("Manager").GetComponent<GameManager>();
        cardColor = gameObject.GetComponent<Image>().color;
        itemCanvas = GameObject.Find("CanvasItemCards");
        artifactCanvas = GameObject.Find("CanvasArtifactCards");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale *= 1.5f;
        transform.SetAsLastSibling();
        gameObject.GetComponentInParent<Canvas>().transform.SetAsLastSibling();
        foreach (Transform child in artifactCanvas.transform)
        {
            if (child.gameObject.name.Contains("Indicator"))
            {
                child.gameObject.transform.SetAsLastSibling();
            }
        }

        if (managerScript.selectedItemCard == null && managerScript.selectedArtifactCard == null)
        {
            managerScript.hoveringOntoCard = true;
            Character characterChoosingCards = managerScript.characters[managerScript.currentPlayerPickingCards % 2].GetComponent<Character>();
            managerScript.cardDescription.SetActive(false);

            if (gameObject.GetComponentInParent<Canvas>().name == "CanvasItemCards")
            {
                managerScript.DisplayText("cardDescription", characterChoosingCards.itemsOwned[inHandIndex].itemName
                                            + "\n" +
                                            characterChoosingCards.itemsOwned[inHandIndex].description);
            }
            else if (gameObject.GetComponentInParent<Canvas>().name == "CanvasArtifactCards")
            {
                managerScript.DisplayText("cardDescription", characterChoosingCards.artifactsOwned[inHandIndex].artifactName
                                            + "\n" +
                                            characterChoosingCards.artifactsOwned[inHandIndex].description);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale /= 1.5f;
        if (managerScript.selectedItemCard == null && managerScript.selectedArtifactCard == null)
        {
            managerScript.hoveringOntoCard = false;
            managerScript.cardDescription.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        managerScript.hoveringOntoCard = false;

        if (gameObject.GetComponentInParent<Canvas>().name == "CanvasItemCards")
        {
            managerScript.selectedItemCard = gameObject;
            managerScript.selectedArtifactCard = null;
        }
        else if (gameObject.GetComponentInParent<Canvas>().name == "CanvasArtifactCards")
        {
            managerScript.selectedArtifactCard = gameObject;
            managerScript.selectedItemCard = null;
        }
        
        MakeOthersTransparent();
    }

    public void MakeOthersTransparent()
    {
        foreach (Transform child in artifactCanvas.transform)
        {
            if (!child.gameObject.name.Contains("Indicator"))
            {
                child.gameObject.GetComponent<Image>().color = new Color(cardColor.r, cardColor.g, cardColor.b, cardColor.a / 5);
            }            
        }
        foreach (Transform child in itemCanvas.transform)
        {
            child.gameObject.GetComponent<Image>().color = new Color(cardColor.r, cardColor.g, cardColor.b, cardColor.a / 5);
        }

        gameObject.GetComponent<Image>().color = new Color(cardColor.r, cardColor.g, cardColor.b, cardColor.a);
    }

    public void MakeOthersVisible()
    {
        Canvas cardCanvas = gameObject.GetComponentInParent<Canvas>();

        foreach (Transform child in cardCanvas.transform)
        {
            child.gameObject.GetComponent<Image>().color = new Color(cardColor.r, cardColor.g, cardColor.b, cardColor.a);
        }
    }
}
