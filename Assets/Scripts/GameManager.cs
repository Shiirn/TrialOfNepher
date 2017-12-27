using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    //Manager Variables
    public int turn;
    public int activePlayer = 0;
    public bool waitingForMovement = false;
    public int diceRoll;
    public bool wasDiceRolled = false;
    public bool isChoosingToFightOpponent = false;
    public bool hasMovementStarted = false;
    public bool canRollDice = true;

    public TurnPhases currentPhase;
    public enum TurnPhases
    {
        INITIAL,
        MOVEMENT,
        PANEL,
        BATTLE,
        END
    }

    //Characters Objects
    public GameObject[] characterPrefabs;
    public List<GameObject> characters = new List<GameObject>();

    //Board Objects
    public GameObject board;
    public List<GameObject> homePanels = new List<GameObject>();

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

        currentPhase = TurnPhases.INITIAL;
    }
	
	void Update () {
        switch(currentPhase)
        {
            case TurnPhases.INITIAL:
                StartCoroutine(InitialPhase());
                break;

            case TurnPhases.MOVEMENT:
                StartCoroutine(MovementPhase());
                if (waitingForMovement)
                {
                    StartCoroutine(ChooseDirection());
                }
                else if(isChoosingToFightOpponent)
                {
                    StartCoroutine(FightOpponentChoice());
                }
                break;

            case TurnPhases.PANEL:
                StartCoroutine(PanelPhase());
                break;

            case TurnPhases.BATTLE:
                //TEMPORARY
                Debug.Log("BATTLE PHASE AHEHAEHHA");
                currentPhase = TurnPhases.END;
                break;

            case TurnPhases.END:
                StartCoroutine(EndPhase());
                break;
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

    IEnumerator InitialPhase()
    {
        activePlayer = turn % characters.Count;

        //TODO: Set conditions to exit the initial phase
        if (!Input.GetMouseButtonDown(1))
        {
            yield return null;
        }
        else
        {
            canRollDice = true;
            Debug.Log("Moving on to the Movement Phase");
            currentPhase++;
        }
    }

    IEnumerator MovementPhase()
    {
        if(wasDiceRolled)
        {
            RollForMovement();
            wasDiceRolled = false;
        }
        yield return null;
    }

    IEnumerator PanelPhase()
    {
        characterScript.currentPanel.GetComponent<Panel>().PanelEffect();
        yield return null;
    }

    IEnumerator EndPhase()
    {
        hasMovementStarted = false;
        wasDiceRolled = false;
        isChoosingToFightOpponent = false;
        turn++;
        currentPhase = TurnPhases.INITIAL;
        Debug.Log("Moving on to the Initial Phase");
        yield return null;
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Roll") && canRollDice && currentPhase == TurnPhases.MOVEMENT)
        {
            canRollDice = false;
            wasDiceRolled = true;
            diceRoll = Random.Range(1, 7);
            Debug.Log(diceRoll);
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
                StartCoroutine(Move(-1, 0));
                break;
            case "downRight":
                waitingForMovement = true;
                break;
            case "down":
                StartCoroutine(Move(0, +1));
                break;
            case "up":
                StartCoroutine(Move(0, -1));
                break;
            case "upRight":
                waitingForMovement = true;
                break;
            case "upLeft":
                waitingForMovement = true;
                break;
            case "left":
                StartCoroutine(Move(+1, 0));
                break;
            case "downLeft":
                waitingForMovement = true;
                break;
        }
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
        hasMovementStarted = true;

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
        characterScript.boardX = (int)characterScript.gameObject.transform.position.x;
        characterScript.boardY = (int)characterScript.gameObject.transform.position.z;

        characterScript.SetCurrentPanel(boardMap.GetPanel(characterScript.boardX,
                                                            characterScript.boardY));

        foreach (GameObject character in characters)
        {
            if (characters[activePlayer].name != character.name)
            {
                if (characters[activePlayer].transform.position.x == character.transform.position.x &&
                   characters[activePlayer].transform.position.z == character.transform.position.z)
                {
                    Debug.Log("Press Mouse Wheel to fight opponent, Right click to ignore");
                    isChoosingToFightOpponent = true;
                }
            }
        }
        
        if(characterScript.currentPanel.name.Contains("Boss"))
        { 
            Debug.Log("Press Mouse Wheel to fight a boss monster, Right click to ignore");
            isChoosingToFightOpponent = true;
        }

        if (diceRoll > 1 && !isChoosingToFightOpponent)
        {
            diceRoll--;
            RollForMovement();
        }
        else if (!isChoosingToFightOpponent)
        {
            currentPhase = TurnPhases.PANEL;
            characterScript.isMoving = false;
        }
    }

    IEnumerator FightOpponentChoice()
    {
        if(Input.GetMouseButtonDown(2))
        {
            Debug.Log("yes");
            isChoosingToFightOpponent = false;
            diceRoll = 0;
            wasDiceRolled = false;
            currentPhase = TurnPhases.BATTLE;

        }
        else if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("noooo");
            isChoosingToFightOpponent = false;
            diceRoll--;
            if (diceRoll > 0)
            {
                RollForMovement();
            }
            else
            {
                currentPhase = TurnPhases.PANEL;
            }
        }
        else
        {
            yield return null;
        }
    }

    void PickDirectionRight()
    {
        if (panelScriptFromRaycast.boardX == characterScript.boardX - 1 && panelScriptFromRaycast.boardY == characterScript.boardY)
        {
            waitingForMovement = false;
            if(diceRoll > 0)
                StartCoroutine(Move(-1, 0));
        }
    }

    void PickDirectionUp()
    {
        if (panelScriptFromRaycast.boardX == characterScript.boardX && panelScriptFromRaycast.boardY == characterScript.boardY - 1)
        {
            waitingForMovement = false;
            if (diceRoll > 0)
                StartCoroutine(Move(0, -1));
        }
    }

    void PickDirectionDown()
    {
        if (panelScriptFromRaycast.boardX == characterScript.boardX && panelScriptFromRaycast.boardY == characterScript.boardY + 1)
        {
            waitingForMovement = false;
            if (diceRoll > 0)
                StartCoroutine(Move(0, +1));
        }
    }

    void PickDirectionLeft()
    {
        if (panelScriptFromRaycast.boardX == characterScript.boardX + 1 && panelScriptFromRaycast.boardY == characterScript.boardY)
        {
            waitingForMovement = false;
            if (diceRoll > 0)
                StartCoroutine(Move(+1, 0));
        }
    }
}
