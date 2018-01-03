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
    GameObject monsterPile;
    GameObject bossPile;
    GameObject itemPile;
    //Pile Scripts
    ArtifactPile artifactPileScript;
    MonsterPile monsterPileScript;
    BossPile bossPileScript;
    ItemPile itemPileScript;

    //UI Objects
    public Canvas canvasInPlay;
    public Canvas canvasInBattle;
    public Button DefendButton;
    public Button EvadeButton;
    //Cards
    public GameObject[] MonsterSprites;
    public GameObject activeMonsterSprite;
    public GameObject[] BossSprites;
    public GameObject activeBossSprite;
    public GameObject blackHoodSprite;
    public GameObject whiteHoodSprite;
    public GameObject blackHoodSpriteInBattle;
    public GameObject whiteHoodSpriteInBattle;    
    //Text    
    public GameObject system;
    public GameObject systemInBattle;
    public GameObject blackHoodStats;
    public GameObject whiteHoodStats;
    public GameObject attackerStats;
    public GameObject enemyStats;

    //Battle Vars
    bool mustFightOpponent = false;
    bool pvpCardsAreSet = false;
    bool isDefending = false;
    bool isEvading = false;
    int currentAttack = 0;
    int currentDefense = 0;
    int currentEvasion = 0;
    //Temporary Battle Scripts
    public FighterCard attackerCard;
    public FighterCard targetCard;

    //Bounty Vars
    public struct Bounty
    {
        public int artifacts;
        public int levels;
        public int items;
    }

    Bounty monsterBounty;
    Bounty bossBounty;
    Bounty characterBounty;

    void Start()
    {
        artifactPile = GameObject.Find("ArtifactCardPile");
        artifactPileScript = artifactPile.GetComponent<ArtifactPile>();
        monsterPile = GameObject.Find("MonsterCardPile");
        monsterPileScript = monsterPile.GetComponent<MonsterPile>();
        bossPile = GameObject.Find("BossCardPile");
        bossPileScript = bossPile.GetComponent<BossPile>();
        itemPile = GameObject.Find("ItemCardPile");
        itemPileScript = itemPile.GetComponent<ItemPile>();

        dieScript = die.GetComponent<DisplayDieValue>();

        boardMap = board.GetComponent<BoardMap>();
        raycastScript = raycaster.GetComponent<RaycastFromCamera>();

        DefendButton.onClick.AddListener(DefendPicked);
        EvadeButton.onClick.AddListener(EvadePicked);
        
        GetHomePanels();
        SpawnCharacters();
        InitializeBounties();

        canvasInPlay.GetComponent<Canvas>().enabled = true;
        canvasInBattle.GetComponent<Canvas>().enabled = false;
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
        //DEBUGGING
        if (Input.GetKeyDown("j"))
        {
            bossScript.bossCard.GetDamaged(1);
        }
        if (Input.GetKeyDown("k"))
        {
            monsterScript.monsterCard.GetDamaged(1);
        }
        if (Input.GetKeyDown("a"))
        {
            characterScript.card.GetDamaged(1);
        }
        if (Input.GetKeyDown("s"))
        {
            opponentScript.card.GetDamaged(1);
        }
        if (Input.GetKeyDown("d"))
        {
            if (characterScript.itemsOwned.Count > 0)
            {
                characterScript.DiscardCardAt(Random.Range(0,characterScript.itemsOwned.Count));
            }
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
        foreach (GameObject character in characters)
        {
            if (characters[activePlayer].name != character.name)
            {
                opponentScript = character.GetComponent<Character>();
            }
        }        

        if (characterScript.card.fighterName == "White Hood")
        {
            blackHoodSprite.GetComponent<CanvasGroup>().alpha = 0.2f;
            whiteHoodSprite.GetComponent<CanvasGroup>().alpha = 1f;
            DisplayStats("whiteHood", characterScript.card.fighterName,
                                characterScript.card.hp,
                                characterScript.card.stats.attack + characterScript.card.buffCounters.attack
                                + characterScript.card.levelCounters.attack,
                                characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                + characterScript.card.levelCounters.defense,
                                characterScript.card.stats.evasion + characterScript.card.buffCounters.evasion
                                + characterScript.card.levelCounters.evasion);
            DisplayStats("blackHood", opponentScript.card.fighterName,
                                    opponentScript.card.hp,
                                    opponentScript.card.stats.attack + opponentScript.card.buffCounters.attack
                                    + opponentScript.card.levelCounters.attack,
                                    opponentScript.card.stats.defense + opponentScript.card.buffCounters.defense
                                    + opponentScript.card.levelCounters.defense,
                                    opponentScript.card.stats.evasion + opponentScript.card.buffCounters.evasion
                                    + opponentScript.card.levelCounters.evasion);

        }
        else
        {
            blackHoodSprite.GetComponent<CanvasGroup>().alpha = 1f;
            whiteHoodSprite.GetComponent<CanvasGroup>().alpha = 0.2f;
            DisplayStats("whiteHood", opponentScript.card.fighterName,
                                    opponentScript.card.hp,
                                    opponentScript.card.stats.attack + opponentScript.card.buffCounters.attack
                                    + opponentScript.card.levelCounters.attack,
                                    opponentScript.card.stats.defense + opponentScript.card.buffCounters.defense
                                    + opponentScript.card.levelCounters.defense,
                                    opponentScript.card.stats.evasion + opponentScript.card.buffCounters.evasion
                                    + opponentScript.card.levelCounters.evasion);
            DisplayStats("blackHood", characterScript.card.fighterName,
                                characterScript.card.hp,
                                characterScript.card.stats.attack + characterScript.card.buffCounters.attack
                                + characterScript.card.levelCounters.attack,
                                characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                + characterScript.card.levelCounters.defense,
                                characterScript.card.stats.evasion + characterScript.card.buffCounters.evasion
                                + characterScript.card.levelCounters.evasion);
        }

        DisplayText("system", "You can change equipped artifact or use a item before rolling. Press Right Click to roll.");

        //TODO: Set conditions to exit the initial phase
        if (!Input.GetMouseButtonDown(1) && !reviving)
        {
            canRollDice = false;
            yield return null;
        }
        else if (characterScript.card.isAlive)
        {
            canRollDice = true;
            DisplayText("system", "Press Space to roll the Die");            
            currentPhase = TurnPhases.MOVEMENT;
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
            DisplayText("system", "Press Space to roll the Die");
            DieRollState();
            yield return null;
        }
        else if (wasDiceRolled)
        {
            DisplayText("system", "Click on the Panel next to your character to move.");
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
        canvasInPlay.GetComponent<Canvas>().enabled = false;
        canvasInBattle.GetComponent<Canvas>().enabled = true;
        DefendButton.gameObject.SetActive(true);
        EvadeButton.gameObject.SetActive(true);
        if (characterScript.card.fighterName == "White Hood")
        {
            whiteHoodSpriteInBattle.SetActive(true);
        }
        else
            blackHoodSpriteInBattle.SetActive(true);

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
                            DisplayText("systemInBattle", characterScript.card.fighterName + " attacks with " + currentAttack);

                            if (bossScript.bossCard.nature == Nature.Defender)
                            {
                                DisplayText("systemInBattle", "Boss is rolling for Defense");
                            }
                            else if (bossScript.bossCard.nature == Nature.Evader)
                            {
                                DisplayText("systemInBattle", "Roll for Boss Evasion");
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
                                DisplayText("systemInBattle", bossScript.bossCard.fighterName + " defends with: " + currentDefense);

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
                                DisplayText("systemInBattle", bossScript.bossCard.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    bossScript.bossCard.GetDamaged(currentAttack);
                                }
                            }

                            DisplayText("systemInBattle", bossScript.bossCard.fighterName + " is left with " + bossScript.bossCard.hp + " HPs");

                            ResetBattleCounters();
                            wasDiceRolled = false;

                            if (bossScript.bossCard.isAlive)
                            {
                                currentBattlePhase = BattlePhases.OPPONENTATTACK;
                            }
                            else if (!bossScript.bossCard.isAlive)
                            {
                                currentBattlePhase = BattlePhases.ENDOFBATTLE;
                            }

                        }

                        break;

                    case BattlePhases.OPPONENTATTACK:

                        if (!wasDiceRolled)
                        {
                            DisplayText("systemInBattle", "Roll for boss attack");
                            DieRollState();
                            canRollDice = false;
                            die.GetComponent<ApplyRandomForce>().RollDie();
                        }
                        else
                        {                           
                            currentAttack = bossScript.bossCard.stats.attack + diceRoll;
                            DisplayText("systemInBattle", bossScript.bossCard.fighterName + " attacks with " + currentAttack);

                            wasDiceRolled = false;
                            currentBattlePhase = BattlePhases.WAITFORTARGET2;
                        }                        

                        break;

                    case BattlePhases.WAITFORTARGET2:

                        DisplayText("systemInBattle", "Pick Defend or Evade");

                        if (isDefending || isEvading)
                        {
                            if (!wasDiceRolled)
                            {
                                attackerCard = bossScript.bossCard;
                                targetCard = characterScript.card;

                                if (isDefending)
                                    DisplayText("systemInBattle", "Roll for Defense");
                                else
                                    DisplayText("systemInBattle", "Roll for Evasion");
                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                        + diceRoll + characterScript.card.levelCounters.defense;
                                    DisplayText("systemInBattle", characterScript.card.fighterName + " defends with: " + currentDefense);

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
                                    DisplayText("systemInBattle", characterScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        characterScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                DisplayText("systemInBattle", characterScript.card.fighterName + " is left with " + characterScript.card.hp + " HPs");

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

                        if (!bossScript.bossCard.isAlive)
                        {
                            bossPileScript.Discard(bossScript.bossCard.id);

                            if (artifactPileScript.cards.Count > 0)
                            {
                                characterScript.DrawArtifactCards(bossBounty.artifacts);
                            }

                            characterScript.DrawItemCards(bossBounty.items);

                            characterScript.card.LevelUp(bossBounty.levels);

                            DisplayText("systemInBattle", "Your current level is " + characterScript.card.level + "\n" + "Your current stats are "
                                 + (characterScript.card.levelCounters.attack + characterScript.card.buffCounters.attack)
                                 + (characterScript.card.levelCounters.defense + +characterScript.card.buffCounters.defense)
                                 + (characterScript.card.levelCounters.evasion + characterScript.card.buffCounters.evasion));
                        }

                        ResetBattleCounters();

                        characterScript.card.ResetBuffs();
                        opponentScript.card.ResetBuffs();

                        canvasInPlay.GetComponent<Canvas>().enabled = true;
                        canvasInBattle.GetComponent<Canvas>().enabled = false;
                        DefendButton.gameObject.SetActive(false);
                        EvadeButton.gameObject.SetActive(false);
                        activeBossSprite.SetActive(false);
                        whiteHoodSpriteInBattle.SetActive(false);
                        blackHoodSpriteInBattle.SetActive(false);
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
            activeMonsterSprite.SetActive(true);

            if (!wasDiceRolled && !rollingInBattle)
            {
                DisplayText("systemInBattle", "Roll to attack");
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
                            DisplayText("systemInBattle", characterScript.card.fighterName + " attacks with " + currentAttack);
                            
                            if (monsterScript.monsterCard.nature == Nature.Defender)
                            {
                                DisplayText("systemInBattle", "Monster is rolling for Defense");
                            }
                            else if (monsterScript.monsterCard.nature == Nature.Evader)
                            {
                                DisplayText("systemInBattle", "Roll for Monster Evasion");
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
                                DisplayText("systemInBattle", monsterScript.monsterCard.fighterName + " defends with: " + currentDefense);

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
                                DisplayText("systemInBattle", monsterScript.monsterCard.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    monsterScript.monsterCard.GetDamaged(currentAttack);
                                }
                            }

                            DisplayText("systemInBattle", monsterScript.monsterCard.fighterName + " is left with " + monsterScript.monsterCard.hp + " HPs");

                            ResetBattleCounters();
                            wasDiceRolled = false;

                            if (monsterScript.monsterCard.isAlive)
                            {
                                currentBattlePhase = BattlePhases.OPPONENTATTACK;
                            }
                            else if (!monsterScript.monsterCard.isAlive)
                            {
                                currentBattlePhase = BattlePhases.ENDOFBATTLE;
                            }

                        }

                        break;

                    case BattlePhases.OPPONENTATTACK:

                        if (!wasDiceRolled)
                        {
                            DisplayText("systemInBattle", "Roll for monster attack");
                            DieRollState();
                            canRollDice = false;
                            die.GetComponent<ApplyRandomForce>().RollDie();
                        }
                        else
                        {
                            attackerCard = monsterScript.monsterCard;
                            targetCard = characterScript.card;

                            currentAttack = monsterScript.monsterCard.stats.attack + diceRoll;
                            DisplayText("systemInBattle", monsterScript.monsterCard.fighterName + " attacks with " + currentAttack);

                            wasDiceRolled = false;
                            currentBattlePhase = BattlePhases.WAITFORTARGET2;
                        }

                        break;

                    case BattlePhases.WAITFORTARGET2:

                        DisplayText("systemInBattle", "Pick Defend or Evade");

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
                                    DisplayText("systemInBattle", characterScript.card.fighterName + " defends with: " + currentDefense);

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
                                    DisplayText("systemInBattle", characterScript.card.fighterName + " evades with: " + currentEvasion);

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

                        if (!monsterScript.monsterCard.isAlive)
                        {
                            monsterPileScript.Discard(monsterScript.monsterCard.id);

                            if (artifactPileScript.cards.Count > 0)
                            {
                                characterScript.DrawArtifactCards(monsterBounty.artifacts);
                            }

                            characterScript.DrawItemCards(monsterBounty.items);

                            characterScript.card.LevelUp(monsterBounty.levels);

                            DisplayText("systemInBattle", "Your current level is " + characterScript.card.level + "\n" + "Your current stats are "
                                 + (characterScript.card.levelCounters.attack + characterScript.card.buffCounters.attack)
                                 + (characterScript.card.levelCounters.defense + +characterScript.card.buffCounters.defense)
                                 + (characterScript.card.levelCounters.evasion + characterScript.card.buffCounters.evasion));
                        }

                        ResetBattleCounters();

                        characterScript.card.ResetBuffs();
                        opponentScript.card.ResetBuffs();

                        canvasInPlay.GetComponent<Canvas>().enabled = true;
                        canvasInBattle.GetComponent<Canvas>().enabled = false;
                        DefendButton.gameObject.SetActive(false);
                        EvadeButton.gameObject.SetActive(false);
                        activeMonsterSprite.SetActive(false);
                        whiteHoodSpriteInBattle.SetActive(false);
                        blackHoodSpriteInBattle.SetActive(false);
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
            if (!pvpCardsAreSet)
            {
                if (characterScript.card.fighterName == "White Hood")
                {
                    blackHoodSpriteInBattle.SetActive(true);
                    blackHoodSpriteInBattle.transform.Translate(+285, 0, 0);
                }
                else
                {
                    whiteHoodSpriteInBattle.SetActive(true);
                    whiteHoodSpriteInBattle.transform.Translate(+285, 0, 0);
                }

                pvpCardsAreSet = true;
            }

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

            if (!wasDiceRolled && !rollingInBattle)
            {
                DisplayText("systemInBattle", "Roll to attack");
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
                        DisplayText("systemInBattle", characterScript.card.fighterName + " attacks with " + currentAttack);
                        currentBattlePhase = BattlePhases.WAITFORTARGET;         
                        
                        break;

                    case BattlePhases.WAITFORTARGET:

                        if (isDefending || isEvading)
                        {
                            if (!wasDiceRolled)
                            {
                                DisplayText("systemInBattle", "Pick Defend or Evade");
                                if (isDefending)
                                    DisplayText("systemInBattle", "Roll for Defense");
                                else
                                    DisplayText("systemInBattle", "Roll for Evasion");
                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = opponentScript.card.stats.defense + opponentScript.card.buffCounters.defense
                                        + diceRoll + opponentScript.card.levelCounters.defense;
                                    DisplayText("systemInBattle", opponentScript.card.fighterName + " defends with: " + currentDefense);

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
                                    DisplayText("systemInBattle", opponentScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        opponentScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                DisplayText("systemInBattle", opponentScript.card.fighterName + " is left with " + opponentScript.card.hp + " HPs");

                                ResetBattleCounters();
                                wasDiceRolled = false;

                                if (opponentScript.card.isAlive)
                                {
                                    currentBattlePhase = BattlePhases.OPPONENTATTACK;
                                }
                                else if (!opponentScript.card.isAlive)
                                {
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
                            DisplayText("systemInBattle", "Roll for enemy attack");
                            DieRollState();
                        }
                        else
                        {
                            attackerCard = opponentScript.card;
                            targetCard = characterScript.card;

                            currentAttack = opponentScript.card.stats.attack + opponentScript.card.buffCounters.attack + 
                                diceRoll + opponentScript.card.levelCounters.attack;
                            DisplayText("systemInBattle", opponentScript.card.fighterName + " attacks with " + currentAttack);

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
                                DisplayText("systemInBattle", "Pick Defend or Evade");
                                if (isDefending)
                                    DisplayText("systemInBattle", "Roll for Defense");
                                else
                                    DisplayText("systemInBattle", "Roll for Evasion");
                                DieRollState();
                            }
                            else
                            {
                                if (isDefending)
                                {
                                    currentDefense = characterScript.card.stats.defense + characterScript.card.buffCounters.defense
                                        + diceRoll + characterScript.card.levelCounters.defense;
                                    DisplayText("systemInBattle", characterScript.card.fighterName + " defends with: " + currentDefense);

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
                                    DisplayText("systemInBattle", characterScript.card.fighterName + " evades with: " + currentEvasion);

                                    if (currentAttack >= currentEvasion)
                                    {
                                        characterScript.card.GetDamaged(currentAttack);
                                    }
                                }

                                DisplayText("systemInBattle", characterScript.card.fighterName + " is left with " + characterScript.card.hp + " HPs");

                                ResetBattleCounters();
                                wasDiceRolled = false;
                                if (!characterScript.card.isAlive)
                                {
                                    opponentScript.card.LevelUp(1);
                                    DisplayText("systemInBattle", "Your opponent level is " + opponentScript.card.level + "\n" + "Your opponent current stats are " 
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

                        if (!opponentScript.card.isAlive)
                        {
                            Debug.Log("This is happening, right?");

                            if (artifactPileScript.cards.Count > 0)
                            {
                                characterScript.DrawArtifactCards(characterBounty.artifacts);
                            }

                            characterScript.DrawItemCards(characterBounty.items);

                            characterScript.card.LevelUp(characterBounty.levels);

                            DisplayText("systemInBattle", "Your current level is " + characterScript.card.level + "\n" + "Your current stats are "
                                 + (characterScript.card.levelCounters.attack + characterScript.card.buffCounters.attack)
                                 + (characterScript.card.levelCounters.defense + +characterScript.card.buffCounters.defense)
                                 + (characterScript.card.levelCounters.evasion + characterScript.card.buffCounters.evasion));
                        }

                        if (!characterScript.card.isAlive)
                        {
                            Debug.Log("This is happening, right?");

                            if (artifactPileScript.cards.Count > 0)
                            {
                                opponentScript.DrawArtifactCards(characterBounty.artifacts);
                            }

                            opponentScript.DrawItemCards(characterBounty.items);

                            opponentScript.card.LevelUp(characterBounty.levels);

                            DisplayText("systemInBattle", "Your current level is " + opponentScript.card.level + "\n" + "Your current stats are "
                                 + (opponentScript.card.levelCounters.attack + opponentScript.card.buffCounters.attack)
                                 + (opponentScript.card.levelCounters.defense + +opponentScript.card.buffCounters.defense)
                                 + (opponentScript.card.levelCounters.evasion + opponentScript.card.buffCounters.evasion));
                        }

                        ResetBattleCounters();

                        characterScript.card.ResetBuffs();
                        opponentScript.card.ResetBuffs();

                        mustFightOpponent = false;

                        canvasInPlay.GetComponent<Canvas>().enabled = true;
                        canvasInBattle.GetComponent<Canvas>().enabled = false;
                        DefendButton.gameObject.SetActive(false);
                        EvadeButton.gameObject.SetActive(false);
                        
                        if (characterScript.card.fighterName == "White Hood")
                        {                            
                            blackHoodSpriteInBattle.transform.Translate(-285, 0, 0);
                        }
                        else
                        {                            
                            whiteHoodSpriteInBattle.transform.Translate(-285, 0, 0);
                        }
                        pvpCardsAreSet = false;
                        whiteHoodSpriteInBattle.SetActive(false);
                        blackHoodSpriteInBattle.SetActive(false);
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
                _card.transform.SetParent(canvasInBattle.transform, false);
                break;
            case "monster":                
                activeMonsterSprite = _card;
                _card.transform.SetParent(canvasInBattle.transform, false);
                break;
        }
            
    }

    public void DisplayText(string _textType, string _text)
    {
        switch (_textType)
        {            
            case "system":
                system.GetComponent<Text>().text = _text;
                break;
            case "systemInBattle":
                systemInBattle.GetComponent<Text>().text = _text;
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
            case "whiteHood":
                whiteHoodStats.GetComponent<Text>().text = name + "\n" + "HP" + "<color=red>" + _hp + "</color>" + "\n"
                                                                      + "ATK" + "<color=orange>" + _atk + "</color>" + "/"
                                                                      + "DEF" + "<color=blue>" + _def + "</color>" + "/"
                                                                      + "EVS" + "<color=green>" + _evs + "</color>";
                break;
            case "blackHood":
                blackHoodStats.GetComponent<Text>().text = name + "\n" + "HP" + "<color=red>" + _hp + "</color>" + "\n"
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

    public void InitializeBounties()
    {
        monsterBounty.items = 1;
        monsterBounty.artifacts = 1;
        monsterBounty.levels = 1;

        bossBounty.items = 1;
        bossBounty.artifacts = 2;
        bossBounty.levels = 2;

        characterBounty.items = 1;
        characterBounty.artifacts = 1;
        characterBounty.levels = 1;
    }
}