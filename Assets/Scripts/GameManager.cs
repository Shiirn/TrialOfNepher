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

    //Raycast Objects
    public GameObject raycaster;
    //Raycast Scripts
    RaycastFromCamera raycastScript;
    Panel panelScriptFromRaycast;

    //Characters Objects
    public GameObject[] characterPrefabs;
    public List<GameObject> characters = new List<GameObject>();
    //Character Scripts
    public Character characterScript;
    public Character opponentScript;    
    HomePanelIdentifier homePanelIdentifier;

    //Monster Scripts
    public ActiveFighter monsterScript;
    public ActiveFighter bossScript;

    //Board Objects
    public GameObject board;
    public List<GameObject> homePanels = new List<GameObject>();
    //Board Scripts
    BoardMap boardMap;
    Panel panelScript;

    //Die Objects
    public GameObject dieContainer;
    public GameObject die;
    //Die Scripts
    DisplayDieValue dieScript;

    //Pile Objects
    GameObject artifactPile;
    //Pile Scripts
    ArtifactPile artifactPileScript;

    //UI Objects
    public Canvas canvas;
    public Button DefendButton;
    public Button EvadeButton;
    //Cards
    public GameObject[] MonsterSprites;
    public GameObject activeMonsterSprite;
    public GameObject[] BossSprites;
    public GameObject activeBossSprite;
    public GameObject blackHoodSprite;
    public GameObject whiteHoodSprite;
    public GameObject activeCharSprite;
    public GameObject enemyCharSprite;
    //Text
    public GameObject playerTurn;
    public GameObject system;
    public GameObject attackerStats;
    public GameObject enemyStats;

    //Battle vars
    bool mustFightOpponent = false;
    bool isDefending = false;
    bool isEvading = false;
    int currentAttack = 0;
    int currentDefense = 0;
    int currentEvasion = 0;
    //Temporary battle Scripts
    public FighterCard attackerCard;
    public FighterCard targetCard;


    void Start()
    {
        artifactPile = GameObject.Find("ArtifactCardPile");
        artifactPileScript = artifactPile.GetComponent<ArtifactPile>();

        dieScript = die.GetComponent<DisplayDieValue>();

        boardMap = board.GetComponent<BoardMap>();
        raycastScript = raycaster.GetComponent<RaycastFromCamera>();

        DefendButton.onClick.AddListener(DefendPicked);
        EvadeButton.onClick.AddListener(EvadePicked);
        
        GetHomePanels();
        SpawnCharacters();

        DefendButton.gameObject.SetActive(false);
        EvadeButton.gameObject.SetActive(false);
        attackerStats.SetActive(false);
        enemyStats.SetActive(false);

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
        //DEBUGGING
        if (Input.GetKeyDown("j"))
        {
            monsterScript.monsterCard.hp--;
        }
        //DEBUGGING
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

        //DEBUGGING
        foreach (ArtifactCard artifact in characterScript.card.artifactsOwned)
        {
            Debug.Log(artifact.artifactName);
        }
        //DEBUGGING

        string playerTurnString = characterScript.card.fighterName + " is now playing.";
        DisplayText("playerTurn", playerTurnString);

        //TODO: Set conditions to exit the initial phase
        if (!Input.GetMouseButtonDown(1) && !reviving)
        {
            canRollDice = false;
            yield return null;
        }
        else if (characterScript.card.isAlive)
        {
            canRollDice = true;
            DisplayText("system", "Press Space to roll the Die, then click on the Panel next to your character to move.");
            currentPhase=TurnPhases.MOVEMENT;
        }
        else if (!characterScript.card.isAlive)
        {
            DisplayText("system", "Roll to revive");
            reviving = true;
            
            if(wasDiceRolled)
            {                
                int reviveDiceRoll = diceRoll;
                if (reviveDiceRoll >= 4)
                {
                    characterScript.card.isAlive = true;
                    characterScript.card.hp = characterScript.card.stats.maxHp;

                    DisplayText("system", "You rolled a " + reviveDiceRoll + "! You successfully revived!");

                    currentPhase = TurnPhases.END;
                }
                else
                {
                    DisplayText("system", "You rolled a " + reviveDiceRoll + ". You couldn't revive.");

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
        if (!wasDiceRolled && !waitingForMovement && !characterScript.isMoving)
        {
            DieRollState();
            yield return null;
        }
        else if (wasDiceRolled)
        {
            RollForMovement();
            wasDiceRolled = false;
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
            DisplayStats("attacker", characterScript.card.fighterName,
                                characterScript.card.hp,
                                characterScript.card.stats.attack + characterScript.card.buffCounters.attack
                                + characterScript.card.levelCounters.attack,
                                characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                + characterScript.card.levelCounters.defense,
                                characterScript.card.stats.evasion + characterScript.card.buffCounters.evasion
                                + characterScript.card.levelCounters.evasion);
            DisplayStats("enemy", bossScript.bossCard.fighterName,
                                    bossScript.bossCard.hp,
                                    bossScript.bossCard.stats.attack,
                                    bossScript.bossCard.stats.defense,
                                    bossScript.bossCard.stats.evasion);

            attackerStats.SetActive(true);
            enemyStats.SetActive(true);
            activeCharSprite.SetActive(true);
            activeBossSprite.SetActive(true);

            if (!wasDiceRolled && !rollingInBattle)
            {
                DisplayText("system", "Roll to attack");
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
                            attackerCard = characterScript.card;
                            targetCard = bossScript.bossCard;

                            currentAttack = characterScript.card.stats.attack + characterScript.card.buffCounters.attack
                                + diceRoll + characterScript.card.levelCounters.attack;
                            DisplayText("system", characterScript.card.fighterName + " attacks with " + currentAttack);

                            if (bossScript.bossCard.nature == Nature.Defender)
                            {
                                DisplayText("system", "Boss is rolling for Defense");
                            }
                            else if (bossScript.bossCard.nature == Nature.Evader)
                            {
                                DisplayText("system", "Roll for Boss Evasion");
                            }
                            DieRollState();
                            canRollDice = false;
                            die.GetComponent<ApplyRandomForce>().RollDie();
                        }

                        else
                        {
                            if (bossScript.bossCard.nature == Nature.Defender)
                            {
                                currentDefense = bossScript.bossCard.stats.defense + diceRoll;
                                DisplayText("system", bossScript.bossCard.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    bossScript.bossCard.GetDamaged(currentAttack - currentDefense);
                                }

                                else
                                {
                                    bossScript.bossCard.GetDamaged(1);
                                }
                            }
                            if (bossScript.bossCard.nature == Nature.Evader)
                            {
                                currentEvasion = bossScript.bossCard.stats.evasion + diceRoll;
                                DisplayText("system", bossScript.bossCard.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    bossScript.bossCard.GetDamaged(currentAttack);
                                }
                            }

                            DisplayText("system", bossScript.bossCard.fighterName + " is left with " + bossScript.bossCard.hp + " HPs");

                            ResetBattleCounters();
                            wasDiceRolled = false;

                            if (bossScript.bossCard.isAlive)
                            {
                                currentBattlePhase = BattlePhases.OPPONENTATTACK;
                            }
                            else if (!bossScript.bossCard.isAlive)
                            {
                                characterScript.card.LevelUp(2);

                                DisplayText("system", "Your current level is " + characterScript.card.level + "\n" + "Your current stats are "
                                    + (characterScript.card.levelCounters.attack + characterScript.card.buffCounters.attack)
                                    + (characterScript.card.levelCounters.defense + +characterScript.card.buffCounters.defense)
                                     + (characterScript.card.levelCounters.evasion + characterScript.card.buffCounters.evasion));

                                currentBattlePhase = BattlePhases.ENDOFBATTLE;
                            }

                        }

                        break;

                    case BattlePhases.OPPONENTATTACK:

                        if (!wasDiceRolled)
                        {
                            DisplayText("system", "Roll for boss attack");
                            DieRollState();
                            canRollDice = false;
                            die.GetComponent<ApplyRandomForce>().RollDie();
                        }
                        else
                        {                           
                            currentAttack = bossScript.bossCard.stats.attack + diceRoll;
                            DisplayText("system", bossScript.bossCard.fighterName + " attacks with " + currentAttack);

                            wasDiceRolled = false;
                            currentBattlePhase = BattlePhases.WAITFORTARGET2;
                        }                        

                        break;

                    case BattlePhases.WAITFORTARGET2:

                        DisplayText("system", "Pick Defend or Evade");

                        if (isDefending || isEvading)
                        {
                            if (!wasDiceRolled)
                            {
                                attackerCard = bossScript.bossCard;
                                targetCard = characterScript.card;

                                if (isDefending)
                                    DisplayText("system", "Roll for Defense");
                                else
                                    DisplayText("system", "Roll for Evasion");
                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                        + diceRoll + characterScript.card.levelCounters.defense;
                                    DisplayText("system", characterScript.card.fighterName + " defends with: " + currentDefense);

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
                                    DisplayText("system", characterScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        characterScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                DisplayText("system", characterScript.card.fighterName + " is left with " + characterScript.card.hp + " HPs");

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
                        activeCharSprite.SetActive(false);
                        attackerStats.SetActive(false);
                        enemyStats.SetActive(false);

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
            DisplayStats("attacker", characterScript.card.fighterName,
                                characterScript.card.hp,
                                characterScript.card.stats.attack + characterScript.card.buffCounters.attack
                                + characterScript.card.levelCounters.attack,
                                characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                + characterScript.card.levelCounters.defense,
                                characterScript.card.stats.evasion + characterScript.card.buffCounters.evasion
                                + characterScript.card.levelCounters.evasion);
            DisplayStats("enemy", monsterScript.monsterCard.fighterName,
                                    monsterScript.monsterCard.hp,
                                    monsterScript.monsterCard.stats.attack,
                                    monsterScript.monsterCard.stats.defense,
                                    monsterScript.monsterCard.stats.evasion);

            attackerStats.SetActive(true);
            enemyStats.SetActive(true);
            activeCharSprite.SetActive(true);
            activeMonsterSprite.SetActive(true);

            if (!wasDiceRolled && !rollingInBattle)
            {
                DisplayText("system", "Roll to attack");
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
                            attackerCard = characterScript.card;
                            targetCard = monsterScript.monsterCard;

                            currentAttack = characterScript.card.stats.attack + characterScript.card.buffCounters.attack 
                                + diceRoll + characterScript.card.levelCounters.attack;
                            DisplayText("system", characterScript.card.fighterName + " attacks with " + currentAttack);
                            
                            if (monsterScript.monsterCard.nature == Nature.Defender)
                            {
                                DisplayText("system", "Monster is rolling for Defense");
                            }
                            else if (monsterScript.monsterCard.nature == Nature.Evader)
                            {
                                DisplayText("system", "Roll for Monster Evasion");
                            }
                            DieRollState();
                            canRollDice = false;
                            die.GetComponent<ApplyRandomForce>().RollDie();
                        }

                        else
                        {
                            if (monsterScript.monsterCard.nature == Nature.Defender)
                            {
                                currentDefense = monsterScript.monsterCard.stats.defense + diceRoll;
                                DisplayText("system", monsterScript.monsterCard.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    monsterScript.monsterCard.GetDamaged(currentAttack - currentDefense);
                                }

                                else
                                {
                                    monsterScript.monsterCard.GetDamaged(1);
                                }
                            }
                            if (monsterScript.monsterCard.nature == Nature.Evader)
                            {
                                currentEvasion = monsterScript.monsterCard.stats.evasion + diceRoll;
                                DisplayText("system", monsterScript.monsterCard.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    monsterScript.monsterCard.GetDamaged(currentAttack);
                                }
                            }

                            DisplayText("system", monsterScript.monsterCard.fighterName + " is left with " + monsterScript.monsterCard.hp + " HPs");

                            ResetBattleCounters();
                            wasDiceRolled = false;

                            if (monsterScript.monsterCard.isAlive)
                            {
                                currentBattlePhase = BattlePhases.OPPONENTATTACK;
                            }
                            else if (!monsterScript.monsterCard.isAlive)
                            {
                                characterScript.card.LevelUp(1);

                                DisplayText("system", "Your current level is " + characterScript.card.level + "\n" + "Your current stats are " 
                                    + (characterScript.card.levelCounters.attack + characterScript.card.buffCounters.attack)
                                    + (characterScript.card.levelCounters.defense + +characterScript.card.buffCounters.defense)
                                     + (characterScript.card.levelCounters.evasion + characterScript.card.buffCounters.evasion));

                                currentBattlePhase = BattlePhases.ENDOFBATTLE;
                            }

                        }

                        break;

                    case BattlePhases.OPPONENTATTACK:

                        if (!wasDiceRolled)
                        {
                            DisplayText("system", "Roll for monster attack");
                            DieRollState();
                            canRollDice = false;
                            die.GetComponent<ApplyRandomForce>().RollDie();
                        }
                        else
                        {
                            attackerCard = monsterScript.monsterCard;
                            targetCard = characterScript.card;

                            currentAttack = monsterScript.monsterCard.stats.attack + diceRoll;
                            DisplayText("system", monsterScript.monsterCard.fighterName + " attacks with " + currentAttack);

                            wasDiceRolled = false;
                            currentBattlePhase = BattlePhases.WAITFORTARGET2;
                        }

                        break;

                    case BattlePhases.WAITFORTARGET2:

                        DisplayText("system", "Pick Defend or Evade");

                        if (isDefending || isEvading)
                        {
                            if (!wasDiceRolled)
                            {
                                if (isDefending)
                                    DisplayText("system", "Roll for Defense");
                                else
                                    DisplayText("system", "Roll for Evasion");

                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                        + diceRoll + characterScript.card.levelCounters.defense;
                                    DisplayText("system", characterScript.card.fighterName + " defends with: " + currentDefense);

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
                                    DisplayText("system", characterScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        characterScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                DisplayText("system", characterScript.card.fighterName + " is left with " + characterScript.card.hp + " HPs");

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

                        if (!monsterScript.monsterCard.isAlive && artifactPileScript.cards.Count > 0)
                        {
                            characterScript.card.artifactsOwned.Add(artifactPileScript.Draw());
                        }

                        ResetBattleCounters();

                        characterScript.card.ResetBuffs();
                        opponentScript.card.ResetBuffs();

                        DefendButton.gameObject.SetActive(false);
                        EvadeButton.gameObject.SetActive(false);
                        activeMonsterSprite.SetActive(false);
                        activeCharSprite.SetActive(false);
                        attackerStats.SetActive(false);
                        enemyStats.SetActive(false);

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
            DisplayStats("attacker", characterScript.card.fighterName,
                                characterScript.card.hp,
                                characterScript.card.stats.attack + characterScript.card.buffCounters.attack
                                + characterScript.card.levelCounters.attack,
                                characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                + characterScript.card.levelCounters.defense,
                                characterScript.card.stats.evasion + characterScript.card.buffCounters.evasion
                                + characterScript.card.levelCounters.evasion);
            DisplayStats("enemy", opponentScript.card.fighterName,
                                    opponentScript.card.hp,
                                    opponentScript.card.stats.attack + opponentScript.card.buffCounters.attack
                                    + opponentScript.card.levelCounters.attack,
                                    opponentScript.card.stats.defense + opponentScript.card.buffCounters.defense
                                    + opponentScript.card.levelCounters.defense,
                                    opponentScript.card.stats.evasion + opponentScript.card.buffCounters.evasion
                                    + opponentScript.card.levelCounters.evasion);

            attackerStats.SetActive(true);
            enemyStats.SetActive(true);

            if (characters[activePlayer].GetComponent<Character>().card.fighterName == "White Hood")
            {
                SetCard("activePlayer", whiteHoodSprite);
                SetCard("enemyPlayer", blackHoodSprite);
            }
            else
            {
                SetCard("activePlayer", blackHoodSprite);
                SetCard("enemyPlayer", whiteHoodSprite);
            }            
            activeCharSprite.SetActive(true);
            enemyCharSprite.SetActive(true);

            if (!wasDiceRolled && !rollingInBattle)
            {
                DisplayText("system", "Roll to attack");
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

                        attackerCard = characterScript.card;
                        targetCard = opponentScript.card;

                        currentAttack = characterScript.card.stats.attack + characterScript.card.buffCounters.attack + diceRoll + characterScript.card.levelCounters.attack;
                        DisplayText("system", characterScript.card.fighterName + " attacks with " + currentAttack);
                        currentBattlePhase = BattlePhases.WAITFORTARGET;         
                        
                        break;

                    case BattlePhases.WAITFORTARGET:

                        if (isDefending || isEvading)
                        {
                            if (!wasDiceRolled)
                            {
                                DisplayText("system", "Pick Defend or Evade");
                                if (isDefending)
                                    DisplayText("system", "Roll for Defense");
                                else
                                    DisplayText("system", "Roll for Evasion");
                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = opponentScript.card.stats.defense + opponentScript.card.buffCounters.defense
                                        + diceRoll + opponentScript.card.levelCounters.defense;
                                    DisplayText("system", opponentScript.card.fighterName + " defends with: " + currentDefense);

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
                                    DisplayText("system", opponentScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        opponentScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                DisplayText("system", opponentScript.card.fighterName + " is left with " + opponentScript.card.hp + " HPs");

                                ResetBattleCounters();
                                wasDiceRolled = false;

                                if (opponentScript.card.isAlive)
                                {
                                    currentBattlePhase = BattlePhases.OPPONENTATTACK;
                                }
                                else if (!opponentScript.card.isAlive)
                                {
                                    characterScript.card.LevelUp(1);
                                    DisplayText("system", "Your current level is " + characterScript.card.level + "\n" + "Your current stats are " 
                                        + (characterScript.card.levelCounters.attack + characterScript.card.buffCounters.attack)
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
                            DisplayText("system", "Roll for enemy attack");
                            DieRollState();
                        }
                        else
                        {
                            attackerCard = opponentScript.card;
                            targetCard = characterScript.card;

                            currentAttack = opponentScript.card.stats.attack + opponentScript.card.buffCounters.attack + 
                                diceRoll + opponentScript.card.levelCounters.attack;
                            DisplayText("system", opponentScript.card.fighterName + " attacks with " + currentAttack);

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
                                DisplayText("system", "Pick Defend or Evade");
                                if (isDefending)
                                    DisplayText("system", "Roll for Defense");
                                else
                                    DisplayText("system", "Roll for Evasion");
                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                        + diceRoll + characterScript.card.levelCounters.defense;
                                    DisplayText("system", characterScript.card.fighterName + " defends with: " + currentDefense);

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
                                    DisplayText("system", characterScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        characterScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                DisplayText("system", characterScript.card.fighterName + " is left with " + characterScript.card.hp + " HPs");

                                ResetBattleCounters();
                                wasDiceRolled = false;
                                if (!characterScript.card.isAlive)
                                {
                                    opponentScript.card.LevelUp(1);
                                    DisplayText("system", "Your opponent level is " + opponentScript.card.level + "\n" + "Your opponent current stats are " 
                                        + (opponentScript.card.levelCounters.attack + opponentScript.card.buffCounters.attack)
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
                        activeCharSprite.SetActive(false);
                        enemyCharSprite.SetActive(false);
                        attackerStats.SetActive(true);
                        enemyStats.SetActive(true);

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

        DisplayText("system", "Moving on to the Initial Phase");
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
            CheckAndActivateAbility("movement");

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
                    DisplayText("system", "Press Mouse Wheel to fight opponent, Right click to ignore");
                    isChoosingToFightOpponent = true;
                }
            }
        }

        if (characterScript.currentPanel.name.Contains("Boss"))
        {
            if (diceRoll >= 2)
            {
                DisplayText("system", "Press Mouse Wheel to fight a boss monster, Right click to ignore");
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
            isChoosingToFightOpponent = false;
            mustFightOpponent = true;
            diceRoll = 0;
            wasDiceRolled = false;
            characterScript.isMoving = false;
            currentPhase = TurnPhases.BATTLE;

        }
        else if (Input.GetMouseButtonDown(1))
        {
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
            isChoosingToFightBoss = false;
            diceRoll = 0;
            wasDiceRolled = false;

            characterScript.isMoving = false;
            currentPhase = TurnPhases.PANEL;

        }
        else if (Input.GetMouseButtonDown(1))
        {
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

    public void SetCard(string _cardType, GameObject _card)
    {
        switch (_cardType)
        {
            case "boss":                
                activeBossSprite = _card;
                _card.transform.SetParent(canvas.transform);
                break;
            case "monster":                
                activeMonsterSprite = _card;
                _card.transform.SetParent(canvas.transform);
                activeMonsterSprite.GetComponent<RectTransform>().position.Set(15, -353, 0);
                activeMonsterSprite.GetComponent<RectTransform>().anchorMin.Set(0, 1);
                activeMonsterSprite.GetComponent<RectTransform>().anchorMax.Set(0, 1);
                activeMonsterSprite.GetComponent<RectTransform>().pivot.Set(0, 1);
                break;
            case "activePlayer":                
                activeCharSprite = _card;
                break;
            case "enemyPlayer":
                enemyCharSprite = _card;
                break;
        }
            
    }

    public void DisplayText(string _textType, string _text)
    {
        switch (_textType)
        {
            case "playerTurn":
                playerTurn.GetComponent<Text>().text = _text;
                break;
            case "system":
                system.GetComponent<Text>().text = _text;
                break;            

        }
    }

    public void DisplayStats(string _fighter, string name, int _hp, int _atk, int _def, int _evs)
    {
        switch (_fighter)
        {
            case "attacker":
                attackerStats.GetComponent<Text>().text = name + "\n" + "HP" + "<color=red>" + _hp + "</color>" + "\n"
                                                                      + "ATK" + "<color=orange>" + _atk + "</color>" + "/"
                                                                      + "DEF" + "<color=blue>" + _def + "</color>" + "/"
                                                                      + "EVS" + "<color=green>" + _evs + "</color>";
                break;
            case "enemy":
                enemyStats.GetComponent<Text>().text = name + "\n" + "HP" + "<color=red>" + _hp + "</color>" + "\n"
                                                                      + "ATK" + "<color=orange>" + _atk + "</color>" + "/"
                                                                      + "DEF" + "<color=blue>" + _def + "</color>" + "/"
                                                                      + "EVS" + "<color=green>" + _evs + "</color>";
                break;
        }
    }

    public void CheckAndActivateAbility(string targetAbility)
    {
        foreach (string ability in characterScript.card.abilities)
        {
            if (ability.Contains(targetAbility))
            {
                AbilityScript.ActivateArtifactAbility(ability);
            }
        }
    }
}