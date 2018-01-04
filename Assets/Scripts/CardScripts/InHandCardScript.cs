using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InHandCardScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public int inHandIndex;
    Color cardColor;
    GameManager managerScript;

    private void Start()
    {
        managerScript = GameObject.Find("Manager").GetComponent<GameManager>();
        cardColor = gameObject.GetComponent<Image>().color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale *= 2.5f;
        transform.SetAsLastSibling();
        gameObject.GetComponentInParent<Canvas>().transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale /= 2.5f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(gameObject.GetComponentInParent<Canvas>().name == "CanvasItemCards")
        {
            managerScript.selectedItemCard = gameObject;
        }
        else if (gameObject.GetComponentInParent<Canvas>().name == "CanvasArtifactCards")
        {
            managerScript.selectedArtifactCard = gameObject;
        }
        MakeOthersTransparent();
    }

    public void MakeOthersTransparent()
    {
        Canvas cardCanvas = gameObject.GetComponentInParent<Canvas>();

        foreach (Transform child in cardCanvas.transform)
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
