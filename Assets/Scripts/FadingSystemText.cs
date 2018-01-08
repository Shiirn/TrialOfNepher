using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadingSystemText : MonoBehaviour {

    public Text textScript;
    public bool isLastText;
    float timeStartedFading;
    GameObject textContainer;

    // Use this for initialization
    void Start () {
	}
	
	public void SetText(string _text)
    {
        isLastText = true;
        textScript = GetComponent<Text>();
        textScript.text = _text;

        textContainer = Instantiate(GameObject.Find("TextContainer"));
        textContainer.transform.SetParent(GameObject.Find("CanvasSystemText").transform, false);
    }

    public void PushBack()
    {
        if(isLastText)
        {
            isLastText = false;
            timeStartedFading = Time.time;
            textScript.color = new Color(240, 240, 240, 255);
        }

        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                    gameObject.transform.position.y + 100 * gameObject.GetComponentInParent<Canvas>().scaleFactor,
                                                    gameObject.transform.position.z);

        textContainer.transform.position = gameObject.transform.position;

        gameObject.transform.SetAsLastSibling();
        
    }

    private void Update()
    {
        if (gameObject != null)
        {
            if (!isLastText)
            {
                if (timeStartedFading < (Time.time - 5.0f))
                {
                    textScript.CrossFadeAlpha(0, 2, true);

                    gameObject.transform.position = new Vector3(gameObject.transform.position.x + 1,
                                                    gameObject.transform.position.y,
                                                    gameObject.transform.position.z);
                    textContainer.transform.position = gameObject.transform.position;

                    gameObject.transform.SetAsLastSibling();
                }

                if (timeStartedFading < (Time.time - 10.0f))
                {
                    Destroy(textContainer);
                    Destroy(gameObject);
                    GameObject.Find("Manager").GetComponent<GameManager>().systemTexts.RemoveAt(0);
                }
            }
        }
    }
}
