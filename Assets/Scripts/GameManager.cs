using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public BattlePhases currentBattlePhase;
    public enum TurnPhases
    {
        INITIAL,
        MOVEMENT,
        PANEL,
        BATTLE,
        END
    }

    public enum BattlePhases
    {
        PLAYERATTACK,
        WAITFORTARGET,
        OPPONENTATTACK,
        WAITFORTARGET2,
        ENDOFBATTLE
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
    public Character opponentScript;
    public ActiveFighter monsterScript;
    HomePanelIdentifier homePanelIdentifier;

    //Board Scripts
    BoardMap boardMap;
    Panel panelScript;

    //Raycast Scripts
    RaycastFromCamera raycastScript;
    Panel panelScriptFromRaycast;

    //Battle Buttons
    public Button DefendButton;
    public Button EvadeButton;

    //Battle vars
    bool isDefending = false;
    bool isEvading = false;
    int currentAttack = 0;
    int currentDefense = 0;
    int currentEvasion = 0;

    void Start()
    {
        boardMap = board.GetComponent<BoardMap>();
        raycastScript = raycaster.GetComponent<RaycastFromCamera>();
        DefendButton.onClick.AddListener(DefendPicked);
        EvadeButton.onClick.AddListener(EvadePicked);
        
        GetHomePanels();
        SpawnCharacters();

        DefendButton.gameObject.SetActive(false);
        EvadeButton.gameObject.SetActive(false);

        currentPhase = TurnPhases.INITIAL;
        currentBattlePhase = BattlePhases.PLAYERATTACK;
    }
	
	void Update() {
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
                StartCoroutine(BattlePhase());
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
        characterScript = characters[activePlayer].GetComponent<Character>();

        //TODO: Set conditions to exit the initial phase
        if (!Input.GetMouseButtonDown(1))
        {
            yield return null;
        }
        else if (characterScript.card.isAlive)
        {
            canRollDice = true;
            Debug.Log("Moving on to the Movement Phase");
            currentPhase=TurnPhases.MOVEMENT;
        }
        else if (!characterScript.card.isAlive)
        {
            int reviveDiceRoll = Random.Range(1, 7);
            if(reviveDiceRoll >= 4)
            {
                characterScript.card.isAlive = true;
                characterScript.card.hp = characterScript.card.stats.maxHp;

                Debug.Log("You rolled a " + reviveDiceRoll + "! You successfully revived!");

                currentPhase = TurnPhases.END;
            }
            else
            {
                Debug.Log("You rolled a " + reviveDiceRoll + ". You couldn't revive.");

                currentPhase = TurnPhases.END;
            }
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

    IEnumerator BattlePhase()
    {
        DefendButton.gameObject.SetActive(true);
        EvadeButton.gameObject.SetActive(true);

        if (characterScript.currentPanel.name.Contains("Monster"))
        {
            switch (currentBattlePhase)
            {
                case BattlePhases.PLAYERATTACK:

                    currentAttack = characterScript.card.stats.attack + characterScript.card.buffCounters.attack + Random.Range(1, 7);
                    Debug.Log(characterScript.card.fighterName + " attacks with " + currentAttack);
                    
                    if (monsterScript.card.nature == FighterCard.Nature.Defender)
                    {
                        currentDefense = monsterScript.card.stats.defense + Random.Range(1, 7);
                        Debug.Log(monsterScript.card.fighterName + " defends with: " + currentDefense);

                        if (currentDefense < currentAttack)
                        {
                            monsterScript.card.GetDamaged(currentAttack - currentDefense);
                        }

                        else
                        {
                            monsterScript.card.GetDamaged(1);
                        }
                    }
                    if (monsterScript.card.nature == FighterCard.Nature.Evader)
                    {
                        currentEvasion = monsterScript.card.stats.evasion + Random.Range(1, 7);
                        Debug.Log(monsterScript.card.fighterName + " evades with: " + currentEvasion);

                        if (currentAttack >= currentEvasion)
                        {
                            monsterScript.card.GetDamaged(currentAttack);
                        }
                    }
                        

                    Debug.Log(monsterScript.card.fighterName + " is left with " + monsterScript.card.hp + " HPs");

                    ResetBattleCounters();

                    if (monsterScript.card.isAlive)
                    {
                        currentBattlePhase = BattlePhases.OPPONENTATTACK;
                    }
                    else if (!monsterScript.card.isAlive)
                    {
                        currentBattlePhase = BattlePhases.ENDOFBATTLE;
                    }

                    break;

                case BattlePhases.OPPONENTATTACK:

                    currentAttack = monsterScript.card.stats.attack + Random.Range(1, 7);
                    Debug.Log(monsterScript.card.fighterName + " attacks with " + currentAttack);

                    currentBattlePhase = BattlePhases.WAITFORTARGET2;

                    break;

                case BattlePhases.WAITFORTARGET2:

                    if (isDefending || isEvading)
                    {
                        if (isDefending)
                        {
                            currentDefense = characterScript.card.stats.defense + characterScript.card.buffCounters.defense + Random.Range(1, 7);
                            Debug.Log(characterScript.card.fighterName + " defends with: " + currentDefense);

                            if (currentDefense < currentAttack)
                            {
                                characterScript.card.GetDamaged(currentAttack - currentDefense);
                            }
                            else
                            {
                                characterScript.card.GetDamaged(1);
                            }
                        }

                        if (isEvading)
                        {
                            currentEvasion = characterScript.card.stats.evasion + characterScript.card.buffCounters.evasion + Random.Range(1, 7);
                            Debug.Log(characterScript.card.fighterName + " evades with: " + currentEvasion);

                            if (currentAttack >= currentEvasion)
                            {
                                characterScript.card.GetDamaged(currentAttack);
                            }
                        }

                        Debug.Log(characterScript.card.fighterName + " is left with " + characterScript.card.hp + " HPs");

                        ResetBattleCounters();

                        currentBattlePhase = BattlePhases.ENDOFBATTLE;
                    }

                    else
                    {
                        yield return null;
                    }
                    break;

                case BattlePhases.ENDOFBATTLE:

                    ResetBattleCounters();

                    characterScript.card.ResetBuffs();
                    opponentScript.card.ResetBuffs();

                    DefendButton.gameObject.SetActive(false);
                    EvadeButton.gameObject.SetActive(false);

                    currentBattlePhase = BattlePhases.PLAYERATTACK;
                    
                    currentPhase = TurnPhases.END;
                    break;
            }
        }
        else
        {
            switch (currentBattlePhase)
            {
                case BattlePhases.PLAYERATTACK:

                    currentAttack = characterScript.card.stats.attack + characterScript.card.buffCounters.attack + Random.Range(1, 7);
                    Debug.Log(characterScript.card.fighterName + " attacks with " + currentAttack);

                    currentBattlePhase = BattlePhases.WAITFORTARGET;
                    break;

                case BattlePhases.WAITFORTARGET:
                    if (isDefending || isEvading)
                    {
                        if (isDefending)
                        {
                            currentDefense = opponentScript.card.stats.defense + opponentScript.card.buffCounters.defense + Random.Range(1, 7);
                            Debug.Log(opponentScript.card.fighterName + " defends with: " + currentDefense);

                            if (currentDefense < currentAttack)
                            {
                                opponentScript.card.GetDamaged(currentAttack - currentDefense);
                            }

                            else
                            {
                                opponentScript.card.GetDamaged(1);
                            }
                        }
                        if (isEvading)
                        {
                            currentEvasion = opponentScript.card.stats.evasion + opponentScript.card.buffCounters.evasion + Random.Range(1, 7);
                            Debug.Log(opponentScript.card.fighterName + " evades with: " + currentEvasion);

                            if (currentAttack >= currentEvasion)
                            {
                                opponentScript.card.GetDamaged(currentAttack);
                            }
                        }

                        Debug.Log(opponentScript.card.fighterName + " is left with " + opponentScript.card.hp + " HPs");

                        ResetBattleCounters();

                        if (opponentScript.card.isAlive)
                        {
                            currentBattlePhase = BattlePhases.OPPONENTATTACK;
                        }
                        else if (!opponentScript.card.isAlive)
                        {
                            currentBattlePhase = BattlePhases.ENDOFBATTLE;
                        }
                    }

                    else
                    {
                        yield return null;
                    }
                    break;

                case BattlePhases.OPPONENTATTACK:

                    currentAttack = opponentScript.card.stats.attack + opponentScript.card.buffCounters.attack + Random.Range(1, 7);
                    Debug.Log(opponentScript.card.fighterName + " attacks with " + currentAttack);

                    currentBattlePhase = BattlePhases.WAITFORTARGET2;

                    break;

                case BattlePhases.WAITFORTARGET2:

                    if (isDefending || isEvading)
                    {
                        if (isDefending)
                        {
                            currentDefense = characterScript.card.stats.defense + characterScript.card.buffCounters.defense + Random.Range(1, 7);
                            Debug.Log(characterScript.card.fighterName + " defends with: " + currentDefense);

                            if (currentDefense < currentAttack)
                            {
                                characterScript.card.GetDamaged(currentAttack - currentDefense);
                            }
                            else
                            {
                                characterScript.card.GetDamaged(1);
                            }
                        }

                        if (isEvading)
                        {
                            currentEvasion = characterScript.card.stats.evasion + characterScript.card.buffCounters.evasion + Random.Range(1, 7);
                            Debug.Log(characterScript.card.fighterName + " evades with: " + currentEvasion);

                            if (currentAttack >= currentEvasion)
                            {
                                characterScript.card.GetDamaged(currentAttack);
                            }
                        }

                        Debug.Log(characterScript.card.fighterName + " is left with " + characterScript.card.hp + " HPs");

                        ResetBattleCounters();

                        currentBattlePhase = BattlePhases.ENDOFBATTLE;
                    }

                    else
                    {
                        yield return null;
                    }
                    break;

                case BattlePhases.ENDOFBATTLE:

                    ResetBattleCounters();

                    characterScript.card.ResetBuffs();
                    opponentScript.card.ResetBuffs();

                    DefendButton.gameObject.SetActive(false);
                    EvadeButton.gameObject.SetActive(false);

                    currentBattlePhase = BattlePhases.PLAYERATTACK;

                    if (characterScript.card.isAlive)
                    {
                        currentPhase = TurnPhases.PANEL;
                    }
                    else
                    {
                        currentPhase = TurnPhases.END;
                    }

                    break;
            }
        }
        yield return null;
    }
   
    IEnumerator EndPhase()
    {
        hasMovementStarted = false;
        wasDiceRolled = false;
        isChoosingToFightOpponent = false;

        ResetBattleCounters();

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

    void ResetBattleCounters()
    {
        currentAttack = 0;
        currentDefense = 0;
        currentEvasion = 0;
        isDefending = false;
        isEvading = false;
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
                        PickDirection("right");
                        PickDirection("down");
                        break;
                    case "downLeft":
                        PickDirection("down");
                        PickDirection("left");
                        break;
                    case "upRight":
                        PickDirection("up");
                        PickDirection("right");
                        break;
                    case "upLeft":
                        PickDirection("up");
                        PickDirection("left");
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
                opponentScript = character.GetComponent<Character>();

                if (characterScript.boardX == opponentScript.boardX &&
                   characterScript.boardY == opponentScript.boardY &&
                   opponentScript.card.isAlive)
                {
                    Debug.Log("Press Mouse Wheel to fight opponent, Right click to ignore");
                    isChoosingToFightOpponent = true;
                }
            }
        }
        
        if(characterScript.currentPanel.name.Contains("Boss"))
        {
            if (diceRoll > 0)
            {
                Debug.Log("Press Mouse Wheel to fight a boss monster, Right click to ignore");
                isChoosingToFightOpponent = true;
            }
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

    void PickDirection(string _direction)
    {
        switch(_direction)
        {
            case "right":
                if (panelScriptFromRaycast.boardX == characterScript.boardX - 1 && panelScriptFromRaycast.boardY == characterScript.boardY)
                {
                    waitingForMovement = false;
                    if (diceRoll > 0)
                        StartCoroutine(Move(-1, 0));
                }
                break;

            case "up":
                if (panelScriptFromRaycast.boardX == characterScript.boardX && panelScriptFromRaycast.boardY == characterScript.boardY - 1)
                {
                    waitingForMovement = false;
                    if (diceRoll > 0)
                        StartCoroutine(Move(0, -1));
                }
                break;

            case "down":
                if (panelScriptFromRaycast.boardX == characterScript.boardX && panelScriptFromRaycast.boardY == characterScript.boardY + 1)
                {
                    waitingForMovement = false;
                    if (diceRoll > 0)
                        StartCoroutine(Move(0, +1));
                }
                break;

            case "left":
                if (panelScriptFromRaycast.boardX == characterScript.boardX + 1 && panelScriptFromRaycast.boardY == characterScript.boardY)
                {
                    waitingForMovement = false;
                    if (diceRoll > 0)
                    StartCoroutine(Move(+1, 0));
                }
                break;
        }
    }

    void DefendPicked()
    {
        isDefending = true;
    }
    
    void EvadePicked()
    {
        isEvading = true;
    }
}