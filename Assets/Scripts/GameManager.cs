using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    //Manager Variables
    public int turn;
    //Player    
    public int activePlayer = 0;
    public bool waitingForMovement = false;
    public bool hasMovementStarted = false;
    public bool reviving = false;   
    //Battle
    public bool enemyIsDefendingOrEvading = false;
    public bool isChoosingToFightOpponent = false;
    public bool isChoosingToFightBoss = false;    
    //Die
    public int diceRoll;
    public bool canRollDice = true;    
    public bool rollingInBattle = false;
    public bool wasDiceRolled = false;

    public TurnPhases currentPhase;
    public TurnPhases previousPhase;
    public BattlePhases currentBattlePhase;    

    public enum TurnPhases
    {
        INITIAL,
        MOVEMENT,
        PANEL,
        BATTLE,
        END,
        ROLLINGDIE
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

    //Die Objects
    public GameObject dieContainer;
    public GameObject die;

    //Raycast Objects
    public GameObject raycaster;

    //UI Objects
    public Canvas canvas;
    public Button DefendButton;
    public Button EvadeButton;
    //Monsters
    public GameObject[] MonsterSprites;
    public GameObject activeMonsterSprite;
    public GameObject[] BossSprites;
    public GameObject activeBossSprite;

    //Character Scripts
    Character characterScript;
    public Character opponentScript;    
    HomePanelIdentifier homePanelIdentifier;

    //Monster Scripts
    public ActiveFighter monsterScript;
    public ActiveFighter bossScript;

    //Board Scripts
    BoardMap boardMap;
    Panel panelScript;

    //Die Scripts
    DisplayDieValue dieScript;

    //Raycast Scripts
    RaycastFromCamera raycastScript;
    Panel panelScriptFromRaycast;    
    
    //Battle vars
    bool mustFightOpponent = false;
    bool isDefending = false;
    bool isEvading = false;
    int currentAttack = 0;
    int currentDefense = 0;
    int currentEvasion = 0;

    void Start()
    {
        dieScript = die.GetComponent<DisplayDieValue>();

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
                else if (isChoosingToFightBoss)
                {
                    StartCoroutine(FightBossChoice());
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
            case TurnPhases.ROLLINGDIE:
                StartCoroutine(RollingDie(previousPhase));
                break;
        }
        if (Input.GetKeyDown("j"))
        {
            bossScript.bossCard.hp--;
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
        if (!Input.GetMouseButtonDown(1) && !reviving)
        {
            canRollDice = false;
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
            Debug.Log("Trying to revive");
            reviving = true;
            
            if(wasDiceRolled)
            {                
                int reviveDiceRoll = diceRoll;
                if (reviveDiceRoll >= 4)
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

                wasDiceRolled = false;
                reviving = false;
            }
            else
            {
                DieRollState();
            }
           
        }
    }

    IEnumerator MovementPhase()
    {
        if (wasDiceRolled)
        {
            RollForMovement();
            wasDiceRolled = false;
        }
        else if (!wasDiceRolled && !waitingForMovement && !characterScript.isMoving)
        {            
            DieRollState();
            yield return null;            
        }        
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

        if (characterScript.currentPanel.name.Contains("Boss") && !mustFightOpponent)
        {
            activeBossSprite.SetActive(true);

            if (!wasDiceRolled && !rollingInBattle)
            {
                Debug.Log("Roll to attack");
                DieRollState();
            }
            else if (wasDiceRolled || rollingInBattle)
            {
                if (!rollingInBattle)
                wasDiceRolled = false;

                rollingInBattle = true;                

                switch (currentBattlePhase)
                {
                    case BattlePhases.PLAYERATTACK:

                        if (!wasDiceRolled)
                        {
                            currentAttack = characterScript.card.stats.attack + characterScript.card.buffCounters.attack
                                + diceRoll + characterScript.card.levelCounters.attack;
                            Debug.Log(characterScript.card.fighterName + " attacks with " + currentAttack);

                            if (bossScript.bossCard.nature == FighterCard.Nature.Defender)
                            {
                                Debug.Log("Boss is rolling for Defense");
                            }
                            else if (bossScript.bossCard.nature == FighterCard.Nature.Evader)
                            {
                                Debug.Log("Roll for Boss Evasion");
                            }
                            DieRollState();
                            die.GetComponent<ApplyRandomForce>().RollDie();
                        }

                        else
                        {
                            if (bossScript.bossCard.nature == FighterCard.Nature.Defender)
                            {
                                currentDefense = bossScript.bossCard.stats.defense + diceRoll;
                                Debug.Log(bossScript.bossCard.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    bossScript.bossCard.GetDamaged(currentAttack - currentDefense);
                                }

                                else
                                {
                                    bossScript.bossCard.GetDamaged(1);
                                }
                            }
                            if (bossScript.bossCard.nature == FighterCard.Nature.Evader)
                            {
                                currentEvasion = bossScript.bossCard.stats.evasion + diceRoll;
                                Debug.Log(bossScript.bossCard.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    bossScript.bossCard.GetDamaged(currentAttack);
                                }
                            }

                            Debug.Log(bossScript.bossCard.fighterName + " is left with " + bossScript.bossCard.hp + " HPs");

                            ResetBattleCounters();
                            wasDiceRolled = false;

                            if (bossScript.bossCard.isAlive)
                            {
                                currentBattlePhase = BattlePhases.OPPONENTATTACK;
                            }
                            else if (!bossScript.bossCard.isAlive)
                            {
                                characterScript.card.LevelUp(2);

                                Debug.Log("Your current level is " + characterScript.card.level);
                                Debug.Log("Your current stats are " + (characterScript.card.levelCounters.attack + characterScript.card.buffCounters.attack)
                                    + (characterScript.card.levelCounters.defense + +characterScript.card.buffCounters.defense)
                                     + (characterScript.card.levelCounters.evasion + characterScript.card.buffCounters.evasion));

                                currentBattlePhase = BattlePhases.ENDOFBATTLE;
                            }

                        }

                        break;

                    case BattlePhases.OPPONENTATTACK:

                        if (!wasDiceRolled)
                        {
                            Debug.Log("Roll for boss attack");
                            DieRollState();
                            die.GetComponent<ApplyRandomForce>().RollDie();
                        }
                        else
                        {                           
                            currentAttack = bossScript.bossCard.stats.attack + diceRoll;
                            Debug.Log(bossScript.bossCard.fighterName + " attacks with " + currentAttack);

                            wasDiceRolled = false;
                            currentBattlePhase = BattlePhases.WAITFORTARGET2;
                        }                        

                        break;

                    case BattlePhases.WAITFORTARGET2:

                        //Debug.Log("Pick Defend or Evade");

                        if (isDefending || isEvading)
                        {
                            if (!wasDiceRolled)
                            {
                                if (isDefending)
                                    Debug.Log("Roll for Defense");
                                else
                                    Debug.Log("Roll for Evasion");
                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                        + diceRoll + characterScript.card.levelCounters.defense;
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
                                    currentEvasion = characterScript.card.stats.evasion + characterScript.card.buffCounters.evasion
                                        + diceRoll + characterScript.card.levelCounters.evasion;
                                    Debug.Log(characterScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        characterScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                Debug.Log(characterScript.card.fighterName + " is left with " + characterScript.card.hp + " HPs");

                                ResetBattleCounters();
                                wasDiceRolled = false;

                                currentBattlePhase = BattlePhases.ENDOFBATTLE;
                            }                         
                                                        
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
                        activeBossSprite.SetActive(false);
                        
                        rollingInBattle = false;                        
                        enemyIsDefendingOrEvading = false;

                        currentBattlePhase = BattlePhases.PLAYERATTACK;

                        currentPhase = TurnPhases.END;
                        break;
                }
            }
            
        }
        else if (characterScript.currentPanel.name.Contains("Monster") && !mustFightOpponent)
        {
            activeMonsterSprite.SetActive(true);

            if (!wasDiceRolled && !rollingInBattle)
            {
                Debug.Log("Roll to attack");
                DieRollState();
            }
            else if (wasDiceRolled || rollingInBattle)
            {
                if (!rollingInBattle)
                    wasDiceRolled = false;

                rollingInBattle = true;

                switch (currentBattlePhase)
                {
                    case BattlePhases.PLAYERATTACK:

                        if (!wasDiceRolled)
                        {
                            currentAttack = characterScript.card.stats.attack + characterScript.card.buffCounters.attack 
                                + diceRoll + characterScript.card.levelCounters.attack;
                            Debug.Log(characterScript.card.fighterName + " attacks with " + currentAttack);
                            
                            if (monsterScript.monsterCard.nature == FighterCard.Nature.Defender)
                            {
                                Debug.Log("Monster is rolling for Defense");
                            }
                            else if (monsterScript.monsterCard.nature == FighterCard.Nature.Evader)
                            {
                                Debug.Log("Roll for Monster Evasion");
                            }
                            DieRollState();
                            die.GetComponent<ApplyRandomForce>().RollDie();
                        }

                        else
                        {
                            if (monsterScript.monsterCard.nature == FighterCard.Nature.Defender)
                            {
                                currentDefense = monsterScript.monsterCard.stats.defense + diceRoll;
                                Debug.Log(monsterScript.monsterCard.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    monsterScript.monsterCard.GetDamaged(currentAttack - currentDefense);
                                }

                                else
                                {
                                    monsterScript.monsterCard.GetDamaged(1);
                                }
                            }
                            if (monsterScript.monsterCard.nature == FighterCard.Nature.Evader)
                            {
                                currentEvasion = monsterScript.monsterCard.stats.evasion + diceRoll;
                                Debug.Log(monsterScript.monsterCard.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    monsterScript.monsterCard.GetDamaged(currentAttack);
                                }
                            }

                            Debug.Log(monsterScript.monsterCard.fighterName + " is left with " + monsterScript.monsterCard.hp + " HPs");

                            ResetBattleCounters();
                            wasDiceRolled = false;

                            if (monsterScript.monsterCard.isAlive)
                            {
                                currentBattlePhase = BattlePhases.OPPONENTATTACK;
                            }
                            else if (!monsterScript.monsterCard.isAlive)
                            {
                                characterScript.card.LevelUp(1);

                                Debug.Log("Your current level is " + characterScript.card.level);
                                Debug.Log("Your current stats are " + (characterScript.card.levelCounters.attack + characterScript.card.buffCounters.attack)
                                    + (characterScript.card.levelCounters.defense + +characterScript.card.buffCounters.defense)
                                     + (characterScript.card.levelCounters.evasion + characterScript.card.buffCounters.evasion));

                                currentBattlePhase = BattlePhases.ENDOFBATTLE;
                            }

                        }

                        break;

                    case BattlePhases.OPPONENTATTACK:

                        if (!wasDiceRolled)
                        {
                            Debug.Log("Roll for monster attack");
                            DieRollState();
                            die.GetComponent<ApplyRandomForce>().RollDie();
                        }
                        else
                        {
                            currentAttack = monsterScript.monsterCard.stats.attack + diceRoll;
                            Debug.Log(monsterScript.monsterCard.fighterName + " attacks with " + currentAttack);

                            wasDiceRolled = false;
                            currentBattlePhase = BattlePhases.WAITFORTARGET2;
                        }

                        break;

                    case BattlePhases.WAITFORTARGET2:

                        //Debug.Log("Pick Defend or Evade");

                        if (isDefending || isEvading)
                        {
                            if (!wasDiceRolled)
                            {
                                if (isDefending)
                                    Debug.Log("Roll for Defense");
                                else
                                    Debug.Log("Roll for Evasion");

                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                        + diceRoll + characterScript.card.levelCounters.defense;
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
                                    currentEvasion = characterScript.card.stats.evasion + characterScript.card.buffCounters.evasion 
                                        + diceRoll + characterScript.card.levelCounters.evasion;
                                    Debug.Log(characterScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        characterScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                Debug.Log(characterScript.card.fighterName + " is left with " + characterScript.card.hp + " HPs");

                                ResetBattleCounters();
                                wasDiceRolled = false;

                                currentBattlePhase = BattlePhases.ENDOFBATTLE;
                            }

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
                        activeMonsterSprite.SetActive(false);

                        rollingInBattle = false;
                        enemyIsDefendingOrEvading = false;

                        currentBattlePhase = BattlePhases.PLAYERATTACK;

                        currentPhase = TurnPhases.END;
                        break;
                }
            }

        }
        else if (mustFightOpponent)
        {
            if (!wasDiceRolled && !rollingInBattle)
            {
                Debug.Log("Roll to attack");
                DieRollState();
            }

            if (wasDiceRolled || rollingInBattle)
            {
                if (!rollingInBattle)
                    wasDiceRolled = false;

                rollingInBattle = true;

                switch (currentBattlePhase)
                {
                    case BattlePhases.PLAYERATTACK:
                                                
                        currentAttack = characterScript.card.stats.attack + characterScript.card.buffCounters.attack + diceRoll + characterScript.card.levelCounters.attack;
                        Debug.Log(characterScript.card.fighterName + " attacks with " + currentAttack);
                        currentBattlePhase = BattlePhases.WAITFORTARGET;         
                        
                        break;

                    case BattlePhases.WAITFORTARGET:

                        if (isDefending || isEvading)
                        {
                            if (!wasDiceRolled)
                            {
                                Debug.Log("Pick Defend or Evade");
                                if (isDefending)
                                    Debug.Log("Roll for Defense");
                                else
                                    Debug.Log("Roll for Evasion");
                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = opponentScript.card.stats.defense + opponentScript.card.buffCounters.defense
                                        + diceRoll + opponentScript.card.levelCounters.defense;
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
                                    currentEvasion = opponentScript.card.stats.evasion + opponentScript.card.buffCounters.evasion 
                                        + diceRoll + opponentScript.card.levelCounters.evasion;
                                    Debug.Log(opponentScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        opponentScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                Debug.Log(opponentScript.card.fighterName + " is left with " + opponentScript.card.hp + " HPs");

                                ResetBattleCounters();
                                wasDiceRolled = false;

                                if (opponentScript.card.isAlive)
                                {
                                    currentBattlePhase = BattlePhases.OPPONENTATTACK;
                                }
                                else if (!opponentScript.card.isAlive)
                                {
                                    if(characterScript.card.level < 3)
                                        characterScript.card.LevelUp(1);
                                    Debug.Log("Your current level is " + characterScript.card.level);
                                    Debug.Log("Your current stats are " + (characterScript.card.levelCounters.attack + characterScript.card.buffCounters.attack)
                                        + (characterScript.card.levelCounters.defense + +characterScript.card.buffCounters.defense)
                                         + (characterScript.card.levelCounters.evasion + characterScript.card.buffCounters.evasion));
                                    currentBattlePhase = BattlePhases.ENDOFBATTLE;
                                }
                            }                            
                        }

                        else
                        {
                            yield return null;
                        }
                        break;

                    case BattlePhases.OPPONENTATTACK:

                        if (!wasDiceRolled)
                        {
                            Debug.Log("Roll for enemy attack");
                            DieRollState();
                        }
                        else
                        {
                            currentAttack = opponentScript.card.stats.attack + opponentScript.card.buffCounters.attack + 
                                diceRoll + opponentScript.card.levelCounters.attack;
                            Debug.Log(opponentScript.card.fighterName + " attacks with " + currentAttack);

                            ResetBattleCounters();
                            wasDiceRolled = false;

                            currentBattlePhase = BattlePhases.WAITFORTARGET2;
                        }                        

                        break;

                    case BattlePhases.WAITFORTARGET2:

                        if (isDefending || isEvading)
                        {
                            if (!wasDiceRolled)
                            {
                                Debug.Log("Pick Defend or Evade");
                                if (isDefending)
                                    Debug.Log("Roll for Defense");
                                else
                                    Debug.Log("Roll for Evasion");
                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                        + diceRoll + characterScript.card.levelCounters.defense;
                                    Debug.Log(characterScript.card.fighterName + " defends with: " + currentDefense);

                                    if (currentDefense < currentAttack)
                                    {
                                        characterScript.card.GetDamaged(currentAttack - currentDefense);
                                    }
                                    else
                                    {
                                        if (characterScript.card.level < 2)
                                            characterScript.card.GetDamaged(1);
                                    }
                                }

                                if (isEvading)
                                {
                                    currentEvasion = characterScript.card.stats.evasion + characterScript.card.buffCounters.evasion 
                                        + diceRoll + characterScript.card.levelCounters.evasion;
                                    Debug.Log(characterScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        characterScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                Debug.Log(characterScript.card.fighterName + " is left with " + characterScript.card.hp + " HPs");

                                ResetBattleCounters();
                                wasDiceRolled = false;
                                if (!characterScript.card.isAlive)
                                {
                                    opponentScript.card.LevelUp(1);
                                    Debug.Log("Your opponent level is " + opponentScript.card.level);
                                    Debug.Log("Your opponent current stats are " + (opponentScript.card.levelCounters.attack + opponentScript.card.buffCounters.attack)
                                        + (opponentScript.card.levelCounters.defense + opponentScript.card.buffCounters.defense)
                                         + (opponentScript.card.levelCounters.evasion + opponentScript.card.buffCounters.evasion));
                                }

                                currentBattlePhase = BattlePhases.ENDOFBATTLE;
                            }
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

                        mustFightOpponent = false;

                        DefendButton.gameObject.SetActive(false);
                        EvadeButton.gameObject.SetActive(false);
                                                
                        enemyIsDefendingOrEvading = false;
                        rollingInBattle = false;

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

            
        }
        yield return null;
    }
   
    IEnumerator EndPhase()
    {
        hasMovementStarted = false;
        wasDiceRolled = false;
        isChoosingToFightOpponent = false;
        isChoosingToFightBoss = false;

        ResetBattleCounters();

        turn++;
        currentPhase = TurnPhases.INITIAL;

        Debug.Log("Moving on to the Initial Phase");
        yield return null;
    }    

    IEnumerator RollingDie(TurnPhases previousPhase)
    {        
        if (!wasDiceRolled)
        {
            canRollDice = true;
            KillDieIfRolled();
            yield return null;
        }
        else
        {            
            currentPhase = previousPhase;
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

        if (characterScript.currentPanel.name.Contains("Boss"))
        {
            if (diceRoll >= 2)
            {
                Debug.Log("Press Mouse Wheel to fight a boss monster, Right click to ignore");
                isChoosingToFightBoss = true;
            }
            else if (diceRoll == 0)
            {
                characterScript.isMoving = false;
                currentPhase = TurnPhases.PANEL;
            }
        }

        if (diceRoll > 1)
        {
            if (!isChoosingToFightOpponent && !isChoosingToFightBoss)
            {
                diceRoll--;
                RollForMovement();
            }
        }
        else if (!isChoosingToFightOpponent && !isChoosingToFightBoss)
        {
            characterScript.isMoving = false;
            currentPhase = TurnPhases.PANEL;
        }      
    }

    IEnumerator FightOpponentChoice()
    {
        if(Input.GetMouseButtonDown(2))
        {
            Debug.Log("yes");
            isChoosingToFightOpponent = false;
            mustFightOpponent = true;
            diceRoll = 0;
            wasDiceRolled = false;
            characterScript.isMoving = false;
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

                characterScript.isMoving = false;
                currentPhase = TurnPhases.PANEL;
            }
        }
        else
        {
            yield return null;
        }
    }

    IEnumerator FightBossChoice()
    {
        if (Input.GetMouseButtonDown(2))
        {
            Debug.Log("yes");
            isChoosingToFightBoss = false;
            diceRoll = 0;
            wasDiceRolled = false;

            characterScript.isMoving = false;
            currentPhase = TurnPhases.PANEL;

        }
        else if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("noooo");
            isChoosingToFightBoss = false;
            diceRoll--;
            if (diceRoll > 0)
            {
                RollForMovement();
            }
            else
            {
                characterScript.isMoving = false;
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
        isEvading = false;
    }
    
    void EvadePicked()
    {
        isEvading = true;
        isDefending = false;
    }    
    
    void DieRollState()
    {
        previousPhase = currentPhase;
        currentPhase = TurnPhases.ROLLINGDIE;
    }

    void KillDieIfRolled()
    {
        if (dieScript.rollComplete && canRollDice)
        {
            canRollDice = false;
            wasDiceRolled = true;
            dieScript.rollComplete = false;
        }
    }
}