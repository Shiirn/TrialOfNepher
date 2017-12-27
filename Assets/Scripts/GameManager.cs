using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    //Manager Variables
    int turn;
    int activePlayer = 0;
    bool waitingForMovement = false;
    int diceRoll;

    //Characters Objects
    public GameObject[] characterPrefabs;
    List<GameObject> characters = new List<GameObject>();

    //Board Objects
    public GameObject board;
    List<GameObject> homePanels = new List<GameObject>();

    //Raycast Objects
    public GameObject raycaster;

    //Character Scripts
    Character characterScript;
    HomePanelIdentifier homePanelIdentifier;

    //Board Scripts
    BoardMap boardMap;
    Panel panelScript;

    //Raycast Scripts
    RaycastFromCamera raycastScript;
    Panel panelScriptFromRaycast;
    

    
    void Start ()
    {
        boardMap = board.GetComponent<BoardMap>();
        raycastScript = raycaster.GetComponent<RaycastFromCamera>();

        //Temporarily moving the character in Start
        GetHomePanels();
        SpawnCharacters();
        diceRoll = 1000;
        RollForMovement();

    }
	
	void Update () {
        if(waitingForMovement)
        {
            StartCoroutine(ChooseDirection());
        }
    }

    void GetHomePanels()
    {
        homePanels.Add(boardMap.GetPanel("PanelHome1(Clone)"));
        homePanels.Add(boardMap.GetPanel("PanelHome2(Clone)"));
    }

    void SpawnCharacters()
    {
        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            characters.Add(Instantiate(characterPrefabs[i]));
            homePanelIdentifier = characters[i].GetComponent<HomePanelIdentifier>();
            characterScript = characters[i].GetComponent<Character>();

            foreach (GameObject panel in boardMap.board)
            {
                if (panel.name.Contains(homePanelIdentifier.homePanel.name))
                {
                    panelScript = panel.GetComponent<Panel>();
                    characterScript.boardX = panelScript.boardX;
                    characterScript.boardY = panelScript.boardY;
                    characters[i].transform.position = new Vector3(
                        characterScript.boardX, 0.9f, characterScript.boardY);

                    characterScript.SetCurrentPanel(panel);
                    characterScript.card = new CharacterCard(i);
                }
            }
        }
    }

    void RollForMovement()
    {
        characterScript = characters[activePlayer].GetComponent<Character>();

        characterScript.SetCurrentPanel(boardMap.GetPanel(characterScript.boardX,
                                                characterScript.boardY));

        panelScript = characterScript.currentPanel.GetComponent<Panel>();
            
        switch (panelScript.direction)
        {
            case "right":
                characterScript.boardX--;
                StartCoroutine(Move(-1, 0));
                break;
            case "downRight":
                waitingForMovement = true;
                break;
            case "down":
                characterScript.boardY++;
                StartCoroutine(Move(0, +1));
                break;
            case "up":
                characterScript.boardY--;
                StartCoroutine(Move(0, -1));
                break;
            case "upRight":
                waitingForMovement = true;
                break;
            case "upLeft":
                waitingForMovement = true;
                break;
            case "left":
                characterScript.boardX++;
                StartCoroutine(Move(+1, 0));
                break;
            case "downLeft":
                waitingForMovement = true;
                break;
        }

        characterScript.SetCurrentPanel(boardMap.GetPanel(characterScript.boardX,
                                                characterScript.boardY));
    }

    IEnumerator ChooseDirection()
    {
        while(waitingForMovement)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Gets the Panel script from the panel selected by the raycaster
                panelScriptFromRaycast = raycastScript.selectedPanel.GetComponent<Panel>();

                //Gets the Panel the character is standing on
                panelScript = characterScript.currentPanel.GetComponent<Panel>();

                switch(panelScript.direction)
                {
                    case "downRight":
                        PickDirectionRight();
                        PickDirectionDown();
                        break;
                    case "downLeft":
                        PickDirectionDown();
                        PickDirectionLeft();
                        break;
                    case "upRight":
                        PickDirectionUp();
                        PickDirectionRight();
                        break;
                    case "upLeft":
                        PickDirectionUp();
                        PickDirectionLeft();
                        break;
                }
            }
            yield return null;
        }

    }

    IEnumerator Move(int x, int y)
    {
        characterScript.isMoving = true;

        Vector3 target = new Vector3(
                characterScript.gameObject.transform.position.x + x,
                characterScript.gameObject.transform.position.y,
                characterScript.gameObject.transform.position.z + y);
        Vector3 start = new Vector3(
                characterScript.gameObject.transform.position.x,
                characterScript.gameObject.transform.position.y,
                characterScript.gameObject.transform.position.z);

        if (x != 0)
        {
            if(x < 0)
                for (float f = 0.0f; f < 1.0f; f += 0.1f)
                {
                    characterScript.gameObject.transform.position = new Vector3(start.x - f, start.y, start.z);

                    yield return new WaitForSeconds(.01f);
                }

            else if(x > 0)
                for (float f = 0.0f; f < 1.0f; f += 0.1f)
                {
                    characterScript.gameObject.transform.position = new Vector3(start.x + f, start.y, start.z);

                    yield return new WaitForSeconds(.01f);
                }
        }

        if (y != 0)
        {
            if (y < 0)
                for (float f = 0.0f; f < 1.0f; f += 0.1f)
                {
                    characterScript.gameObject.transform.position = new Vector3(start.x, start.y, start.z - f);
                    
                    yield return new WaitForSeconds(.01f);
                }

            else if (y > 0)
                for (float f = 0.0f; f < 1.0f; f += 0.1f)
                {
                    characterScript.gameObject.transform.position = new Vector3(start.x, start.y, start.z + f);
                    
                    yield return new WaitForSeconds(.01f);
                }
        }
        characterScript.transform.position = target;

        if (diceRoll > 1)
        {
            diceRoll--;
            RollForMovement();
        }
        else
        {
            characterScript.isMoving = false;
        }
    }

    void PickDirectionRight()
    {
        if (panelScriptFromRaycast.boardX == characterScript.boardX - 1 && panelScriptFromRaycast.boardY == characterScript.boardY)
        {
            waitingForMovement = false;
            characterScript.boardX--;
            StartCoroutine(Move(-1, 0));
        }
    }

    void PickDirectionUp()
    {
        if (panelScriptFromRaycast.boardX == characterScript.boardX && panelScriptFromRaycast.boardY == characterScript.boardY - 1)
        {
            waitingForMovement = false;
            characterScript.boardY--;
            StartCoroutine(Move(0, -1));
        }
    }

    void PickDirectionDown()
    {
        if (panelScriptFromRaycast.boardX == characterScript.boardX && panelScriptFromRaycast.boardY == characterScript.boardY + 1)
        {
            waitingForMovement = false;
            characterScript.boardY++;
            StartCoroutine(Move(0, +1));
        }
    }

    void PickDirectionLeft()
    {
        if (panelScriptFromRaycast.boardX == characterScript.boardX + 1 && panelScriptFromRaycast.boardY == characterScript.boardY)
        {
            waitingForMovement = false;
            characterScript.boardX++;
            StartCoroutine(Move(+1, 0));
        }
    }
}
