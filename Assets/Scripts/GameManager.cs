﻿using System.Collections;
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
    public bool artifactCardsShown = false;
    public bool itemCardsShown = false;
    public bool initialSetupDone = false;
    public bool initialSetupInBattleDone = false;
    //Battle
    public bool fightPlayer = false;
    public bool fightBoss = false;
    public bool fightFinalBoss = false;
    public bool ignoreFight = false;
    public bool enemyIsDefendingOrEvading = false;
    public bool isChoosingToFightOpponent = false;
    public bool isChoosingToFightBoss = false;
    public bool isChoosingToFightFinalBoss = false;
    //Die
    public int diceRoll;
    public bool buttonRollPressed = false;
    public bool canRollDice = true;    
    public bool rollingInBattle = false;
    public bool rollingForItem = false;
    public bool wasDiceRolled = false;

    public TurnPhases currentPhase;
    public TurnPhases previousPhase;
    public BattlePhases currentBattlePhase;
    public InitialSubPhases currentInitialSubPhase;
    public EndSubPhases currentEndSubPhase;

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
        INITIAL,
        PLAYERATTACK,
        WAITFORTARGET,
        OPPONENTATTACK,
        WAITFORTARGET2,
        ENDOFBATTLE,
        USINGITEM
    }

    public enum InitialSubPhases
    {
        SETUP,
        STANDBY,
        USINGITEM
    }

    public enum EndSubPhases
    {
        DISCARD,
        RESET
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

    //Monster Scripts
    public ActiveFighter monsterScript;
    public ActiveFighter bossScript;
    public ActiveFighter finalBossScript;

    //Board Objects
    public GameObject board;
    public List<GameObject> homePanels;
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
    public Canvas canvasFightChoice;
    public Canvas canvasInBattle;
    public Canvas canvasInitial;
    public Canvas canvasArtifactCards;
    public Canvas canvasItemCards;
    public Canvas canvasPickStat;
    //Buttons        
    public Button DefendButton;
    public Button EvadeButton;
    public Button EquipArtifactButton;
    public Button MorphBallAttackButton;
    public Button MorphBallDefenseButton;
    public Button MorphBallEvasionButton;
    public Button UseItemButton;
    public Button UseItemInBattleButton;
    public Button DontUseItemInBattleButton;
    public Button RollButton;
    public Button FightPlayerButton;
    public Button FightBossButton;
    public Button fightFinalBossButton;
    public Button IgnoreFightButton;
    //UI Surrogate Vars
    public GameObject selectedItemCard;
    public GameObject selectedArtifactCard;
    public bool hoveringOntoCard = false;

    //Cards
    public GameObject[] monsterSprites;
    public GameObject activeMonsterSprite;
    public GameObject[] bossSprites;
    public GameObject activeBossSprite;
    public GameObject[] finalBossSprites;
    public GameObject activeFinalBossSprite;
    public GameObject blackHoodSprite;
    public GameObject whiteHoodSprite;
    public GameObject blackHoodSpriteInBattle;
    public GameObject whiteHoodSpriteInBattle;
    public GameObject[] artifactCardSprites;
    public GameObject[] itemCardSprites;    
    //Text    
    public GameObject system;
    public GameObject systemInBattle;
    public GameObject cardDescription;
    public GameObject blackHoodStats;
    public GameObject whiteHoodStats;
    public GameObject attackerStats;
    public GameObject enemyStats;

    //Battle Vars
    public bool mustFightOpponent = false;
    bool pvpCardsAreSet = false;
    bool isDefending = false;
    bool isEvading = false;
    int currentAttack = 0;
    int currentDefense = 0;
    int currentEvasion = 0;
    public int currentPlayerPickingCards = 0;
    public bool isFleeing = false;
    public bool wasFinalBossFightIgnored = false;

    //Temporary Battle Scripts
    public FighterCard attackerCard;
    public FighterCard targetCard;

    //Item Vars
    public string pickedStat = "";
    public bool pickingStat;
    public int tempBuffModifier = 0;
    public int tempMovementBuff = 0;

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

        SpawnCharacters();
        InitializeBounties();

        dieScript = die.GetComponent<DisplayDieValue>();

        boardMap = board.GetComponent<BoardMap>();
        raycastScript = raycaster.GetComponent<RaycastFromCamera>();

        DefendButton.onClick.AddListener(DefendPicked);
        EvadeButton.onClick.AddListener(EvadePicked);
        EquipArtifactButton.onClick.AddListener(EquipSelectedArtifact);
        UseItemButton.onClick.AddListener(UseSelectedItem);
        UseItemInBattleButton.onClick.AddListener(UseSelectedItem);
        RollButton.onClick.AddListener(GoToRoll);
        FightPlayerButton.onClick.AddListener(FightPlayerPicked);
        FightBossButton.onClick.AddListener(FightBossPicked);
        fightFinalBossButton.onClick.AddListener(FightFinalBossPicked);
        IgnoreFightButton.onClick.AddListener(IgnoreFightPicked);
        MorphBallAttackButton.onClick.AddListener(PickAttackStat);
        MorphBallDefenseButton.onClick.AddListener(PickDefenseStat);
        MorphBallEvasionButton.onClick.AddListener(PickEvasionStat);
        DontUseItemInBattleButton.onClick.AddListener(ProceedInBattle);

        canvasInPlay.GetComponent<Canvas>().enabled = true;
        canvasPickStat.GetComponent<Canvas>().enabled = false;
        canvasInBattle.GetComponent<Canvas>().enabled = false;        
        canvasFightChoice.GetComponent<Canvas>().enabled = false;
        DefendButton.gameObject.SetActive(false);
        EvadeButton.gameObject.SetActive(false);        
        EquipArtifactButton.gameObject.SetActive(false);
        UseItemButton.gameObject.SetActive(false);
        RollButton.gameObject.SetActive(false);
        DontUseItemInBattleButton.gameObject.SetActive(false);

        currentPhase = TurnPhases.INITIAL;
        currentInitialSubPhase = InitialSubPhases.SETUP;
        currentBattlePhase = BattlePhases.INITIAL;
    }

    void Update() {

        if(selectedItemCard == null && selectedArtifactCard == null && !hoveringOntoCard)
        {
            cardDescription.SetActive(false);
        }
        else
        {
            cardDescription.SetActive(true);
        }

        if (selectedItemCard == null)
        {
            UseItemButton.gameObject.SetActive(false);
            UseItemInBattleButton.gameObject.SetActive(false);
        }
        else
        {
            if (currentPhase != TurnPhases.END)
            {
                if (currentPhase != TurnPhases.BATTLE)
                {
                    UseItemButton.gameObject.transform.position = new Vector3(selectedItemCard.transform.position.x,
                                                                                selectedItemCard.transform.position.y - 100,
                                                                                selectedItemCard.transform.position.z);
                    UseItemButton.gameObject.transform.SetAsLastSibling();
                }
                else
                {
                    UseItemInBattleButton.gameObject.SetActive(true);
                    UseItemInBattleButton.gameObject.transform.position = new Vector3(selectedItemCard.transform.position.x,
                                                                                selectedItemCard.transform.position.y - 100,
                                                                                selectedItemCard.transform.position.z);
                    UseItemInBattleButton.gameObject.transform.SetAsLastSibling();
                }
            }

            int selectedItemIndex = selectedItemCard.GetComponent<InHandCardScript>().inHandIndex;
            DisplayText("cardDescription",
                        characters[currentPlayerPickingCards % 2].GetComponent<Character>().itemsOwned[selectedItemIndex].itemName +
                        "\n" +
                        characters[currentPlayerPickingCards % 2].GetComponent<Character>().itemsOwned[selectedItemIndex].description);
        }

        if (selectedArtifactCard != null)
        {
            if (characterScript.artifactsOwned[selectedArtifactCard.GetComponent<InHandCardScript>().inHandIndex].nature == ArtifactNature.Passive)
            {
                EquipArtifactButton.gameObject.SetActive(false);
            }
            else if (characterScript.artifactsOwned[selectedArtifactCard.GetComponent<InHandCardScript>().inHandIndex].nature == ArtifactNature.Equippable)
            {
                EquipArtifactButton.gameObject.SetActive(true);

                EquipArtifactButton.gameObject.transform.position = new Vector3(selectedArtifactCard.transform.position.x,
                                                                                    selectedArtifactCard.transform.position.y + 100,
                                                                                    selectedArtifactCard.transform.position.z);
                EquipArtifactButton.gameObject.transform.SetAsLastSibling();
            }

            int selectedArtifactIndex = selectedArtifactCard.GetComponent<InHandCardScript>().inHandIndex;
            DisplayText("cardDescription", characters[currentPlayerPickingCards % 2].GetComponent<Character>().artifactsOwned[selectedArtifactIndex].artifactName +
                        "\n" +
                        characters[currentPlayerPickingCards % 2].GetComponent<Character>().artifactsOwned[selectedArtifactIndex].description);
        }
        else
        {
            EquipArtifactButton.gameObject.SetActive(false);
        }

        switch (currentPhase)
        {
            case TurnPhases.INITIAL:
                StartCoroutine(InitialPhase());

                if(canvasPickStat.isActiveAndEnabled)
                {
                    canRollDice = false;
                }

                if (!characterScript.card.isAlive)
                {
                    EquipArtifactButton.gameObject.SetActive(false);

                    if (selectedItemCard != null)
                    {
                        if (!characterScript.itemsOwned[selectedItemCard.GetComponent<InHandCardScript>().inHandIndex].function.Contains("revive"))
                            UseItemButton.gameObject.SetActive(false);
                        else
                            UseItemButton.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (selectedItemCard != null)
                    {
                        UseItemButton.gameObject.SetActive(true);
                    }
                }

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
                else if (isChoosingToFightFinalBoss)
                {
                    StartCoroutine(FightFinalBossChoice());
                }
                break;

            case TurnPhases.PANEL:
                StartCoroutine(PanelPhase());
                break;

            case TurnPhases.BATTLE:
                StartCoroutine(BattlePhase());

                if (currentBattlePhase == BattlePhases.USINGITEM)
                {
                    UseItemInBattleButton.gameObject.SetActive(false);
                    DontUseItemInBattleButton.gameObject.SetActive(false);
                }

                if (currentBattlePhase == BattlePhases.INITIAL)
                {
                    if (mustFightOpponent)
                    {
                        if (currentPlayerPickingCards == activePlayer)
                        {
                            if (characterScript.itemsOwned.Count > 0)
                            {
                                DontUseItemInBattleButton.gameObject.SetActive(true);
                                
                                ShowItems(characterScript);
                            }
                            else
                            {
                                currentPlayerPickingCards += 1;
                            }
                        }
                        else if (currentPlayerPickingCards == (activePlayer + 1))
                        {
                            if (opponentScript.itemsOwned.Count > 0)
                            {
                                DontUseItemInBattleButton.gameObject.SetActive(true);
                                
                                ShowItems(opponentScript);
                            }
                            else
                            {
                                currentPlayerPickingCards += 1;
                            }
                        }
                        else if (currentPlayerPickingCards > (activePlayer + 1))
                        {
                            DontUseItemInBattleButton.gameObject.SetActive(true);
                            
                            if(!isFleeing)
                                currentBattlePhase = BattlePhases.PLAYERATTACK;
                        }
                    }
                    else
                    {
                        if (characterScript.itemsOwned.Count > 0 && currentPlayerPickingCards == activePlayer)
                        {
                            DontUseItemInBattleButton.gameObject.SetActive(true);
                                
                            ShowItems(characterScript);
                        }
                        else
                        {
                            UseItemInBattleButton.gameObject.SetActive(false);
                            DontUseItemInBattleButton.gameObject.SetActive(false);
                            currentBattlePhase = BattlePhases.PLAYERATTACK;
                        }
                    }
                }
                break;

            case TurnPhases.END:
                StartCoroutine(EndPhase());
                break;
            case TurnPhases.ROLLINGDIE:
                StartCoroutine(RollingDie(previousPhase));

                UseItemInBattleButton.gameObject.SetActive(false);
                DontUseItemInBattleButton.gameObject.SetActive(false);

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
        if (Input.GetKeyDown("r"))
        {
            characterScript.DrawItemCards(1);
            characterScript.DrawArtifactCards(1);
        }
        if (Input.GetKeyDown("1"))
        {
            diceRoll += 1;
        }
        if (Input.GetKeyDown("2"))
        {
            diceRoll -= 1;
        }
        //DEBUGGING

        if(currentPhase != TurnPhases.BATTLE)
        {
            if (characterScript.card.fighterName == "White Hood")
            {
                blackHoodSprite.GetComponent<CanvasGroup>().alpha = 0.2f;
                whiteHoodSprite.GetComponent<CanvasGroup>().alpha = 1f;
                DisplayStats(characterScript.card, "whiteHood");
                DisplayStats(opponentScript.card, "blackHood");
            }
            else
            {
                blackHoodSprite.GetComponent<CanvasGroup>().alpha = 1f;
                whiteHoodSprite.GetComponent<CanvasGroup>().alpha = 0.2f;
                DisplayStats(characterScript.card, "blackHood");
                DisplayStats(opponentScript.card, "whiteHood");
            }
        }

        if(currentPhase != TurnPhases.BATTLE)
        {
            currentPlayerPickingCards = activePlayer;
        }

        if (canRollDice)
            RollButton.gameObject.SetActive(true);
        else
            RollButton.gameObject.SetActive(false);
    }

    public void SpawnCharacters()
    {
        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            characters.Add(Instantiate(characterPrefabs[i]));
            characterScript = characters[i].GetComponent<Character>();      
            
            GameObject currentHomePanel = GameObject.Find(homePanels[i].name + "(Clone)");

            panelScript = currentHomePanel.GetComponent<Panel>();
            characterScript.boardX = (int)currentHomePanel.transform.position.x;
            characterScript.boardY = (int)currentHomePanel.transform.position.z;
            characters[i].transform.position = new Vector3(
                characterScript.boardX, 0.9f, characterScript.boardY);

            characterScript.SetCurrentPanel(currentHomePanel);
            characterScript.card = new CharacterCard(i);
        }
    }

    IEnumerator InitialPhase()
    {
        switch (currentInitialSubPhase)
        {
            case InitialSubPhases.SETUP:

                activePlayer = turn % characters.Count;
                characterScript = characters[activePlayer].GetComponent<Character>();
                foreach (GameObject character in characters)
                {
                    if (characters[activePlayer].name != character.name)
                    {
                        opponentScript = character.GetComponent<Character>();
                    }
                }

                DisplayText("system", "You can change equipped artifact or use a item before rolling. Press Right Click to roll.");

                EquipArtifactButton.gameObject.SetActive(true);
                UseItemButton.gameObject.SetActive(true);
                diceRoll = 0;

                canRollDice = true;
                wasDiceRolled = false;
                initialSetupDone = false;
                
                ShowArtifacts();
                ShowItems(characterScript);                               

                currentInitialSubPhase = InitialSubPhases.STANDBY;
                previousPhase = TurnPhases.INITIAL;

                break;

            case InitialSubPhases.STANDBY:

                if (!initialSetupDone)
                {                    
                    yield return null;
                }
                else if (characterScript.card.isAlive)
                {
                    currentEndSubPhase = EndSubPhases.DISCARD;
                    Debug.Log("spipo");
                    DieRollState();
                    currentPhase = TurnPhases.MOVEMENT;                                      
                }
                else if (!characterScript.card.isAlive)
                {
                    DisplayText("system", "Roll to revive");

                    if (!wasDiceRolled)
                    {
                        DieRollState();
                    }
                    else
                    {
                        int reviveDiceRoll = diceRoll;
                        if (reviveDiceRoll >= 4)
                        {
                            characterScript.card.isAlive = true;
                            characterScript.card.hp = characterScript.card.GetCurrentStats().maxHp;

                            Debug.Log("You rolled a " + reviveDiceRoll + "! You successfully revived!");
                            DisplayText("system", "You rolled a " + reviveDiceRoll + "! You successfully revived!");

                            currentPhase = TurnPhases.END;
                            currentEndSubPhase = EndSubPhases.DISCARD;
                        }
                        else
                        {
                            Debug.Log("You rolled a " + reviveDiceRoll + ". You couldn't revive.");
                            DisplayText("system", "You rolled a " + reviveDiceRoll + ". You couldn't revive.");

                            currentPhase = TurnPhases.END;
                            currentEndSubPhase = EndSubPhases.DISCARD;
                        }
                        yield return null;
                    }
                }
                break;

            case InitialSubPhases.USINGITEM:
                if(!rollingForItem)
                {
                    currentInitialSubPhase = InitialSubPhases.STANDBY;
                }
                else
                {
                    UseSelectedItem();
                    yield return null;
                }
                break;
        }
    }

    IEnumerator MovementPhase()
    {
        ResetInitialObjects();

        if (!wasDiceRolled && !waitingForMovement && !characterScript.isMoving && !isChoosingToFightFinalBoss)
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

        if (characterScript.card.fighterName == "White Hood")
        {
            whiteHoodSpriteInBattle.SetActive(true);
        }
        else
            blackHoodSpriteInBattle.SetActive(true);

        if(fightFinalBoss && !mustFightOpponent)
        {
            DisplayStats(characterScript.card, "attacker");
            DisplayStats(finalBossScript.finalBossCard, "enemy");

            attackerStats.SetActive(true);
            enemyStats.SetActive(true);
            activeFinalBossSprite.SetActive(true);

            switch (currentBattlePhase)
            {
                case BattlePhases.INITIAL:

                    attackerCard = characterScript.card;
                    targetCard = finalBossScript.finalBossCard;

                    if (!itemCardsShown)
                    {
                        ResetInitialObjects();
                        ShowItems(characterScript);
                    }

                    break;

                case BattlePhases.PLAYERATTACK:

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

                        if (!wasDiceRolled)
                        {
                            currentAttack = characterScript.card.GetCurrentStats().attack + diceRoll;

                            DisplayText("systemInBattle", characterScript.card.fighterName + " attacks with " + currentAttack);

                            if (diceRoll <= 2)
                            {
                                ResetBattleCounters();
                                wasDiceRolled = false;
                                currentBattlePhase = BattlePhases.OPPONENTATTACK;
                            }
                            else
                            {
                                DisplayText("systemInBattle", finalBossScript.finalBossCard.fighterName + " is rollling for defense.");

                                DieRollState();
                                GoToRoll();
                            }
                        }
                        else
                        {
                            currentDefense = finalBossScript.finalBossCard.GetCurrentStats().defense + diceRoll;
                            DisplayText("systemInBattle", finalBossScript.finalBossCard.fighterName + " defends with: " + currentDefense);

                            if (currentDefense < currentAttack)
                            {
                                finalBossScript.finalBossCard.GetDamaged(currentAttack - currentDefense);
                            }

                            else
                            {
                                finalBossScript.finalBossCard.GetDamaged(1);
                            }

                            CheckThornmail();

                            DisplayText("systemInBattle", finalBossScript.finalBossCard.fighterName + " is left with " + finalBossScript.finalBossCard.hp + " HPs");

                            ResetBattleCounters();
                            wasDiceRolled = false;

                            if (finalBossScript.finalBossCard.isAlive)
                            {
                                currentBattlePhase = BattlePhases.OPPONENTATTACK;
                            }
                            else if (!finalBossScript.finalBossCard.isAlive)
                            {
                                currentBattlePhase = BattlePhases.ENDOFBATTLE;
                            }
                        }
                    }

                    break;

                case BattlePhases.OPPONENTATTACK:

                    if (!wasDiceRolled)
                    {
                        DisplayText("systemInBattle", "Roll for boss attack");
                        DieRollState();
                        GoToRoll();
                    }
                    else
                    {
                        currentAttack = finalBossScript.finalBossCard.GetCurrentStats().attack + diceRoll;
                        DisplayText("systemInBattle", finalBossScript.finalBossCard.fighterName + " attacks with " + currentAttack);

                        wasDiceRolled = false;
                        DefendButton.gameObject.SetActive(true);
                        EvadeButton.gameObject.SetActive(true);
                        currentBattlePhase = BattlePhases.WAITFORTARGET2;
                    }

                    break;

                case BattlePhases.WAITFORTARGET2:

                    DisplayText("systemInBattle", "Pick Defend or Evade");

                    if (isDefending || isEvading)
                    {
                        if (!wasDiceRolled)
                        {
                            attackerCard = finalBossScript.finalBossCard;
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
                                currentDefense = characterScript.card.GetCurrentStats().defense + diceRoll;
                                DisplayText("systemInBattle", characterScript.card.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    characterScript.card.GetDamaged(currentAttack - currentDefense);
                                }
                                else
                                {
                                    characterScript.card.GetDamaged(1);
                                }

                                CheckThornmail();
                            }

                            if (isEvading)
                            {
                                currentEvasion = characterScript.card.GetCurrentStats().evasion + diceRoll;
                                DisplayText("systemInBattle", characterScript.card.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    characterScript.card.GetDamaged(currentAttack);
                                    CheckThornmail();
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

                    if (!finalBossScript.finalBossCard.isAlive)
                    {
                        Debug.Log("ZOMG U WONNE");
                    }

                    ResetInitialObjects();
                    ResetBattleCounters();

                    if (!isFleeing)
                    {
                        characterScript.card.ResetBuffs();
                        isFleeing = false;
                    }

                    canvasInPlay.GetComponent<Canvas>().enabled = true;
                    canvasInBattle.GetComponent<Canvas>().enabled = false;
                    DefendButton.gameObject.SetActive(false);
                    EvadeButton.gameObject.SetActive(false);
                    activeFinalBossSprite.SetActive(false);
                    whiteHoodSpriteInBattle.SetActive(false);
                    blackHoodSpriteInBattle.SetActive(false);
                    attackerStats.SetActive(false);
                    enemyStats.SetActive(false);

                    fightFinalBoss = false;

                    rollingInBattle = false;
                    enemyIsDefendingOrEvading = false;

                    currentBattlePhase = BattlePhases.INITIAL;

                    currentPhase = TurnPhases.END;
                    currentEndSubPhase = EndSubPhases.DISCARD;
                    break;
            }
        }
        else if (characterScript.currentPanel.name.Contains("Boss") && !mustFightOpponent && !fightFinalBoss)
        {
            DisplayStats(characterScript.card, "attacker");
            DisplayStats(bossScript.bossCard, "enemy");

            attackerStats.SetActive(true);
            enemyStats.SetActive(true);
            activeBossSprite.SetActive(true);

            switch (currentBattlePhase)
            {
                case BattlePhases.INITIAL:

                    attackerCard = characterScript.card;
                    targetCard = bossScript.bossCard;
                    if(!itemCardsShown)
                    {
                        ResetInitialObjects();
                        ShowItems(characterScript);
                    }

                    break;

                case BattlePhases.PLAYERATTACK:

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

                        if (!wasDiceRolled)
                        {
                            currentAttack = characterScript.card.GetCurrentStats().attack + diceRoll;

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
                            GoToRoll();
                        }
                        else
                        {
                            if (bossScript.bossCard.nature == Nature.Defender)
                            {
                                currentDefense = bossScript.bossCard.GetCurrentStats().defense + diceRoll;
                                DisplayText("systemInBattle", bossScript.bossCard.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    bossScript.bossCard.GetDamaged(currentAttack - currentDefense);
                                }

                                else
                                {
                                    bossScript.bossCard.GetDamaged(1);
                                }

                                CheckThornmail();
                            }
                            if (bossScript.bossCard.nature == Nature.Evader)
                            {
                                currentEvasion = bossScript.bossCard.GetCurrentStats().evasion + diceRoll;

                                DisplayText("systemInBattle", bossScript.bossCard.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    bossScript.bossCard.GetDamaged(currentAttack);
                                    CheckThornmail();
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
                    }

                    break;

                case BattlePhases.OPPONENTATTACK:

                    if (!wasDiceRolled)
                    {
                        DisplayText("systemInBattle", "Roll for boss attack");
                        DieRollState();
                        GoToRoll();
                    }
                    else
                    {
                        currentAttack = bossScript.bossCard.GetCurrentStats().attack + diceRoll;
                        DisplayText("systemInBattle", bossScript.bossCard.fighterName + " attacks with " + currentAttack);

                        wasDiceRolled = false;
                        DefendButton.gameObject.SetActive(true);
                        EvadeButton.gameObject.SetActive(true);
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
                                currentDefense = characterScript.card.GetCurrentStats().defense + diceRoll;
                                DisplayText("systemInBattle", characterScript.card.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    characterScript.card.GetDamaged(currentAttack - currentDefense);
                                }
                                else
                                {
                                    characterScript.card.GetDamaged(1);
                                }

                                CheckThornmail();
                            }

                            if (isEvading)
                            {
                                currentEvasion = characterScript.card.GetCurrentStats().evasion + diceRoll;
                                DisplayText("systemInBattle", characterScript.card.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    characterScript.card.GetDamaged(currentAttack);
                                    CheckThornmail();
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

                    ResetInitialObjects();
                    ResetBattleCounters();

                    if (!isFleeing)
                    {
                        characterScript.card.ResetBuffs();
                        isFleeing = false;
                    }

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

                    currentBattlePhase = BattlePhases.INITIAL;

                    currentPhase = TurnPhases.END;
                    currentEndSubPhase = EndSubPhases.DISCARD;
                    break;
            }
        }
        else if (characterScript.currentPanel.name.Contains("Monster") && !mustFightOpponent && !fightFinalBoss)
        {
            DisplayStats(characterScript.card, "attacker");
            DisplayStats(monsterScript.monsterCard, "enemy");

            attackerStats.SetActive(true);
            enemyStats.SetActive(true);
            activeMonsterSprite.SetActive(true);


            switch (currentBattlePhase)
            {
                case BattlePhases.INITIAL:

                    attackerCard = characterScript.card;
                    targetCard = monsterScript.monsterCard;

                    if (!itemCardsShown)
                    {
                        ResetInitialObjects();
                        ShowItems(characterScript);
                    }

                    break;

                case BattlePhases.PLAYERATTACK:

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

                        if (!wasDiceRolled)
                        {
                            attackerCard = characterScript.card;
                            targetCard = monsterScript.monsterCard;

                            currentAttack = characterScript.card.GetCurrentStats().attack + diceRoll;

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
                            GoToRoll();
                        }
                        else
                        {
                            if (monsterScript.monsterCard.nature == Nature.Defender)
                            {
                                currentDefense = monsterScript.monsterCard.GetCurrentStats().defense + diceRoll;
                                DisplayText("systemInBattle", monsterScript.monsterCard.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    monsterScript.monsterCard.GetDamaged(currentAttack - currentDefense);
                                }

                                else
                                {
                                    monsterScript.monsterCard.GetDamaged(1);
                                }

                                CheckThornmail();
                            }
                            if (monsterScript.monsterCard.nature == Nature.Evader)
                            {
                                currentEvasion = monsterScript.monsterCard.GetCurrentStats().evasion + diceRoll;
                                DisplayText("systemInBattle", monsterScript.monsterCard.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    monsterScript.monsterCard.GetDamaged(currentAttack);
                                    CheckThornmail();
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
                    }

                    break;

                case BattlePhases.OPPONENTATTACK:

                    if (!wasDiceRolled)
                    {
                        DisplayText("systemInBattle", "Roll for monster attack");
                        DieRollState();
                        GoToRoll();
                    }
                    else
                    {
                        attackerCard = monsterScript.monsterCard;
                        targetCard = characterScript.card;

                        currentAttack = monsterScript.monsterCard.GetCurrentStats().attack + diceRoll;
                        DisplayText("systemInBattle", monsterScript.monsterCard.fighterName + " attacks with " + currentAttack);

                        wasDiceRolled = false;
                        DefendButton.gameObject.SetActive(true);
                        EvadeButton.gameObject.SetActive(true);
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
                                currentDefense = characterScript.card.GetCurrentStats().defense + diceRoll;

                                DisplayText("systemInBattle", characterScript.card.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    characterScript.card.GetDamaged(currentAttack - currentDefense);
                                }
                                else
                                {
                                    characterScript.card.GetDamaged(1);
                                }
                                CheckThornmail();
                            }

                            if (isEvading)
                            {
                                currentEvasion = characterScript.card.GetCurrentStats().evasion + diceRoll;
                                DisplayText("systemInBattle", characterScript.card.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    characterScript.card.GetDamaged(currentAttack);
                                    CheckThornmail();
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

                    ResetInitialObjects();
                    ResetBattleCounters();

                    if (!isFleeing)
                    {
                        characterScript.card.ResetBuffs();
                        isFleeing = false;
                    }

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

                    currentBattlePhase = BattlePhases.INITIAL;

                    currentPhase = TurnPhases.END;
                    currentEndSubPhase = EndSubPhases.DISCARD;
                    break;
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
                
                attackerCard = characterScript.card;
                targetCard = opponentScript.card;

                pvpCardsAreSet = true;
            }

            DisplayStats(characterScript.card, "attacker");
            DisplayStats(opponentScript.card, "enemy");

            attackerStats.SetActive(true);
            enemyStats.SetActive(true);           


            switch (currentBattlePhase)
            {
                case BattlePhases.INITIAL:

                    attackerCard = characterScript.card;
                    targetCard = opponentScript.card;

                    break;

                case BattlePhases.PLAYERATTACK:                                       

                    if (!wasDiceRolled && !rollingInBattle)
                    {
                        DisplayText("systemInBattle", "Roll to attack");
                        DieRollState();
                    }

                    if (wasDiceRolled)
                    {
                        currentAttack = characterScript.card.GetCurrentStats().attack + diceRoll;
                        DisplayText("systemInBattle", characterScript.card.fighterName + " attacks with " + currentAttack);
                        wasDiceRolled = false;
                        DefendButton.gameObject.SetActive(true);
                        EvadeButton.gameObject.SetActive(true);
                        currentBattlePhase = BattlePhases.WAITFORTARGET;
                    }

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
                                currentDefense = opponentScript.card.GetCurrentStats().defense + diceRoll;

                                DisplayText("systemInBattle", opponentScript.card.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    opponentScript.card.GetDamaged(currentAttack - currentDefense);
                                }
                                else
                                {
                                    opponentScript.card.GetDamaged(1);
                                }

                                CheckThornmail();
                            }
                            if (isEvading)
                            {
                                currentEvasion = opponentScript.card.GetCurrentStats().evasion + diceRoll;
                                DisplayText("systemInBattle", opponentScript.card.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    opponentScript.card.GetDamaged(currentAttack);
                                    CheckThornmail();
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

                        currentAttack = opponentScript.card.GetCurrentStats().attack + diceRoll;
                        DisplayText("systemInBattle", opponentScript.card.fighterName + " attacks with " + currentAttack);

                        currentDefense = 0;
                        currentEvasion = 0;
                        isDefending = false;
                        isEvading = false;
                        wasDiceRolled = false;

                        DefendButton.gameObject.SetActive(true);
                        EvadeButton.gameObject.SetActive(true);

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
                                currentDefense = characterScript.card.GetCurrentStats().defense + diceRoll;

                                DisplayText("systemInBattle", characterScript.card.fighterName + " defends with: " + currentDefense);

                                if (currentDefense < currentAttack)
                                {
                                    characterScript.card.GetDamaged(currentAttack - currentDefense);
                                }
                                else
                                {
                                    characterScript.card.GetDamaged(1);
                                }
                                CheckThornmail();
                            }

                            if (isEvading)
                            {
                                currentEvasion = characterScript.card.GetCurrentStats().evasion + diceRoll;
                                DisplayText("systemInBattle", characterScript.card.fighterName + " evades with: " + currentEvasion);

                                if (currentAttack >= currentEvasion)
                                {
                                    characterScript.card.GetDamaged(currentAttack);
                                    CheckThornmail();
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
                        if (artifactPileScript.cards.Count > 0)
                        {
                            characterScript.DrawArtifactCards(characterBounty.artifacts);
                        }
                        else
                        {
                            StealArtifactFromOpponent();
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
                        if (artifactPileScript.cards.Count > 0)
                        {
                            opponentScript.DrawArtifactCards(characterBounty.artifacts);
                        }
                        else
                        {
                            LoseArtifactToOpponent();
                        }

                        opponentScript.DrawItemCards(characterBounty.items);

                        opponentScript.card.LevelUp(characterBounty.levels);

                        DisplayText("systemInBattle", "Your current level is " + opponentScript.card.level + "\n" + "Your current stats are "
                             + (opponentScript.card.levelCounters.attack + opponentScript.card.buffCounters.attack)
                             + (opponentScript.card.levelCounters.defense + +opponentScript.card.buffCounters.defense)
                             + (opponentScript.card.levelCounters.evasion + opponentScript.card.buffCounters.evasion));
                    }

                    ResetInitialObjects();
                    ResetBattleCounters();

                    if (!isFleeing)
                    {
                        characterScript.card.ResetBuffs();
                        opponentScript.card.ResetBuffs();
                        isFleeing = false;
                    }

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

                    currentBattlePhase = BattlePhases.INITIAL;

                    if (characterScript.card.isAlive)
                    {
                        currentPhase = TurnPhases.PANEL;
                    }
                    else
                    {
                        currentPhase = TurnPhases.END;
                        currentEndSubPhase = EndSubPhases.DISCARD;
                    }

                    break;
            }
            yield return null;
        }
    }

    IEnumerator EndPhase()
    {
        switch(currentEndSubPhase)
        {
            case EndSubPhases.DISCARD:

                if(artifactCardsShown)
                {
                    selectedArtifactCard = null;
                    artifactCardsShown = false;

                    foreach (Transform child in canvasArtifactCards.transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }

                int extraCardsToHold = 0;

                foreach (string ability in characterScript.card.abilities)
                {
                    if(ability != "")
                    {
                        if (ability.Split(' ')[0] == "maxCards")
                        {
                            extraCardsToHold += System.Convert.ToInt32(ability.Split(' ')[1]);
                        }
                    }
                }

                if (characterScript.itemsOwned.Count > 3 + extraCardsToHold)
                {
                    
                    ShowItems(characterScript);
                    

                    if (selectedItemCard != null)
                    {
                        
                        ShowItems(characterScript);
                        

                        Debug.Log("Spipolo" + selectedItemCard.name);

                        characterScript.DiscardCardAt(selectedItemCard.GetComponent<InHandCardScript>().inHandIndex);
                        ResetInitialObjects();
                    }
                }
                else
                {
                    currentEndSubPhase = EndSubPhases.RESET;
                }

                extraCardsToHold = 0;

                break;

            case EndSubPhases.RESET:

                hasMovementStarted = false;
                wasDiceRolled = false;
                isChoosingToFightOpponent = false;
                isChoosingToFightBoss = false;

                ResetInitialObjects();
                ResetBattleCounters();

                turn++;

                canvasInPlay.GetComponent<Canvas>().enabled = true;
                canvasInBattle.GetComponent<Canvas>().enabled = false;
                canvasFightChoice.GetComponent<Canvas>().enabled = false;
                DefendButton.gameObject.SetActive(false);
                EvadeButton.gameObject.SetActive(false);
                EquipArtifactButton.gameObject.SetActive(false);
                UseItemButton.gameObject.SetActive(false);
                RollButton.gameObject.SetActive(false);

                tempMovementBuff = 0;
                initialSetupDone = false;

                currentPhase = TurnPhases.INITIAL;
                currentInitialSubPhase = InitialSubPhases.SETUP;
                currentBattlePhase = BattlePhases.INITIAL;

                DisplayText("system", "Moving on to the Initial Phase");
                break;
        }
        
        yield return null;
    }    

    IEnumerator RollingDie(TurnPhases previousPhase)
    {
        if (!buttonRollPressed)
            canRollDice = true;

        if (!wasDiceRolled)
        {
            yield return null;
        }
        else
        {
            buttonRollPressed = false;

            if (previousPhase == TurnPhases.MOVEMENT)
                CheckAndActivateAbility("movement");
            
            if (previousPhase != currentPhase)
            {
                currentPhase = previousPhase;
            }
        }           
    }

    void ResetBattleCounters()
    {
        currentAttack = 0;
        currentDefense = 0;
        currentEvasion = 0;
        isDefending = false;
        isEvading = false;
        fightPlayer = false;
        fightBoss = false;
        ignoreFight = false;
    }

    public void ResetInitialObjects()
    {
        EquipArtifactButton.gameObject.SetActive(false);
        UseItemButton.gameObject.SetActive(false);
        RollButton.gameObject.SetActive(false);

        selectedItemCard = null;
        itemCardsShown = false;

        selectedArtifactCard = null;
        artifactCardsShown = false;
        
        foreach (Transform child in canvasArtifactCards.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in canvasItemCards.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    void RollForMovement()
    {
        if(tempMovementBuff > 0)
        {
            diceRoll += tempMovementBuff;
            tempMovementBuff = 0;
        }

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

        if (diceRoll > 0)
        {
            diceRoll--;
        }

        if (characterScript.currentPanel.name.Contains("Boss"))
        {
            if (characterScript.artifactsOwned.Count >= 4)
            {
                if (diceRoll > 0)
                {
                    DisplayText("system", "Press Mouse Wheel to fight the final Boss, Right click to ignore");
                    isChoosingToFightFinalBoss = true;
                }
                else if (diceRoll <= 0)
                {
                    DisplayText("system", "Press Mouse Wheel to fight the final Boss, Right click to ignore");

                    isChoosingToFightFinalBoss = true;

                    characterScript.isMoving = false;
                }
            }
            else
            {
                if (diceRoll > 0)
                {
                    DisplayText("system", "Press Mouse Wheel to fight a boss monster, Right click to ignore");
                    isChoosingToFightBoss = true;
                }
                else if (diceRoll <= 0)
                {
                    characterScript.isMoving = false;
                    currentPhase = TurnPhases.PANEL;
                }
            }            
        }

        if (diceRoll > 0 && !isChoosingToFightOpponent && !isChoosingToFightBoss && !isChoosingToFightFinalBoss)
        {
            RollForMovement();
        }

        if (diceRoll <= 0 && !isChoosingToFightOpponent && !isChoosingToFightBoss && !isChoosingToFightFinalBoss)
        {
            characterScript.isMoving = false;
            currentPhase = TurnPhases.PANEL;
        }
    }

    IEnumerator FightOpponentChoice()
    {
        canvasFightChoice.GetComponent<Canvas>().enabled = true;
        FightPlayerButton.gameObject.SetActive(true);
        FightBossButton.gameObject.SetActive(false);
        fightFinalBossButton.gameObject.SetActive(false);

        if (fightPlayer)
        {
            canvasFightChoice.GetComponent<Canvas>().enabled = false;
            isChoosingToFightOpponent = false;
            mustFightOpponent = true;
            diceRoll = 0;
            wasDiceRolled = false;
            characterScript.isMoving = false;
            currentPhase = TurnPhases.BATTLE;

        }
        else if (ignoreFight)
        {
            ignoreFight = false;
            canvasFightChoice.GetComponent<Canvas>().enabled = false;
            isChoosingToFightOpponent = false;

            if (characterScript.artifactsOwned.Count >= 4)
            {
                if (characterScript.currentPanel.name.Contains("Boss") || characterScript.currentPanel.name.Contains("Monster"))
                {
                    if (diceRoll > 0)
                    {
                        isChoosingToFightFinalBoss = true;
                    }
                    else
                    {

                    }
                }
                else
                {
                    //diceRoll--;

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
            }
            else
            {
                if (characterScript.currentPanel.name.Contains("Boss") && diceRoll > 0)
                {
                    isChoosingToFightBoss = true;
                    FightBossChoice();
                }
                else
                {
                    //diceRoll--;

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
            }
        }
        else
        {
            yield return null;
        }
    }

    IEnumerator FightBossChoice()
    {
        canvasFightChoice.GetComponent<Canvas>().enabled = true;
        FightPlayerButton.gameObject.SetActive(false);
        FightBossButton.gameObject.SetActive(true);
        fightFinalBossButton.gameObject.SetActive(false);

        if (fightBoss)
        {
            canvasFightChoice.GetComponent<Canvas>().enabled = false;
            isChoosingToFightBoss = false;
            diceRoll = 0;
            wasDiceRolled = false;

            characterScript.isMoving = false;
            currentPhase = TurnPhases.PANEL;

        }
        else if (ignoreFight)
        {
            ignoreFight = false;
            canvasFightChoice.GetComponent<Canvas>().enabled = false;
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

    public IEnumerator FightFinalBossChoice()
    {
        canvasFightChoice.GetComponent<Canvas>().enabled = true;
        FightPlayerButton.gameObject.SetActive(false);
        FightBossButton.gameObject.SetActive(false);
        fightFinalBossButton.gameObject.SetActive(true);

        if (fightFinalBoss)
        {
            canvasFightChoice.GetComponent<Canvas>().enabled = false;
            isChoosingToFightFinalBoss = false;
            diceRoll = 0;
            wasDiceRolled = false;

            characterScript.isMoving = false;
            currentPhase = TurnPhases.PANEL;
        }
        else if (ignoreFight)
        {
            ignoreFight = false;
            canvasFightChoice.GetComponent<Canvas>().enabled = false;
            isChoosingToFightFinalBoss = false;
            wasFinalBossFightIgnored = true;

            if (diceRoll > 0)
            {
                isChoosingToFightBoss = true;
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
        DefendButton.gameObject.SetActive(false);
        EvadeButton.gameObject.SetActive(false);
        isDefending = true;
        isEvading = false;
    }
    
    void EvadePicked()
    {
        DefendButton.gameObject.SetActive(false);
        EvadeButton.gameObject.SetActive(false);
        isEvading = true;
        isDefending = false;
    }    

    void FightPlayerPicked()
    {
        fightPlayer = true;
    }

    void FightBossPicked()
    {
        fightBoss = true;
    }

    void FightFinalBossPicked()
    {
        fightFinalBoss = true;
    }

    void IgnoreFightPicked()
    {
        ignoreFight = true;
    }

    public void ShowArtifacts()
    {
        for (int i = 0; i < characterScript.artifactsOwned.Count; i++)
        {
            foreach (GameObject artifactCard in artifactCardSprites)
            {
                if (artifactCard.name == characterScript.artifactsOwned[i].spriteName)
                {
                    GameObject currentCard = Instantiate(artifactCard);
                    currentCard.transform.SetParent(canvasArtifactCards.transform, false);
                    currentCard.GetComponent<InHandCardScript>().inHandIndex = i;
                    currentCard.transform.Translate(135 * i, 0, 0);

                }
            }

            artifactCardsShown = true;
        }
    }

    void EquipSelectedArtifact()
    {
        if(selectedArtifactCard != null)
        {
            int selectedArtifactIndex = selectedArtifactCard.GetComponent<InHandCardScript>().inHandIndex;

            Debug.Log(characterScript.artifactsOwned[selectedArtifactIndex].ability);

            if (characterScript.artifactsOwned[selectedArtifactIndex] != characterScript.equippedArtifact ||
                characterScript.equippedArtifact == null)
            {
                if (characterScript.artifactsOwned[selectedArtifactIndex].ability != "")
                {
                    string abilityName = characterScript.artifactsOwned[selectedArtifactIndex].ability.Split(' ')[0];

                    if (abilityName == "morph")
                    {
                        canvasPickStat.GetComponent<Canvas>().enabled = true;
                    }
                    else
                    {
                        characterScript.Equip(characterScript.artifactsOwned[selectedArtifactIndex]);
                    }
                }
                else
                {
                    characterScript.Equip(characterScript.artifactsOwned[selectedArtifactIndex]);
                }
            }
        }
    }

    public void ShowItems(Character character)
    {
        if (!itemCardsShown && character.itemsOwned.Count > 0)
        {
            for (int i = 0; i < character.itemsOwned.Count; i++)
            {
                foreach (GameObject itemCard in itemCardSprites)
                {
                    if (itemCard.name == character.itemsOwned[i].spriteName)
                    {
                        GameObject currentCard = Instantiate(itemCard);
                        currentCard.transform.SetParent(canvasItemCards.transform, false);
                        currentCard.GetComponent<InHandCardScript>().inHandIndex = i;
                        currentCard.transform.Translate(135 * i, 0, 0);
                    }
                }
            }
            itemCardsShown = true;
        }
    }

    void UseSelectedItem()
    {
        if (selectedItemCard != null)
        {
            int selectedItemIndex = selectedItemCard.GetComponent<InHandCardScript>().inHandIndex;

            if (currentPhase == TurnPhases.INITIAL &&
                characters[currentPlayerPickingCards % 2].GetComponent<Character>().itemsOwned[selectedItemIndex].nature == ItemNature.Turn)
            {
                ActivateItem(selectedItemIndex);
            }
            else if (currentPhase == TurnPhases.BATTLE &&
                    characters[currentPlayerPickingCards % 2].GetComponent<Character>().itemsOwned[selectedItemIndex].nature == ItemNature.Battle)
            {
                
                if(characters[currentPlayerPickingCards % 2].GetComponent<Character>().itemsOwned[selectedItemIndex].function.Contains("smite"))
                {
                    if (!mustFightOpponent)
                    {
                        ActivateItem(selectedItemIndex);
                    }
                    else
                    {
                        Debug.Log("You can only use this card against monsters and bosses.");
                    }
                }
                else
                {
                    ActivateItem(selectedItemIndex);
                }
            }
            else
            {
                Debug.Log("This card is not meant for this turn phase.");
            }
        }
    }

    void ActivateItem(int _selectedItemIndex)
    {
        Character playerUsingItemScript = characters[currentPlayerPickingCards % 2].GetComponent<Character>();

        UseItemButton.gameObject.SetActive(false);

        FunctionScript.ActivateItemFunction(playerUsingItemScript.itemsOwned[_selectedItemIndex].function);

        if (!rollingForItem)
        {
            UseItemButton.gameObject.SetActive(false);
            playerUsingItemScript.DiscardCardAt(selectedItemCard.GetComponent<InHandCardScript>().inHandIndex);
            foreach (Transform child in canvasItemCards.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            ShowItems(playerUsingItemScript);
        }

        if(currentPhase == TurnPhases.BATTLE && currentBattlePhase == BattlePhases.INITIAL && !canvasPickStat.isActiveAndEnabled)
        {
            ResetInitialObjects();
            currentPlayerPickingCards += 1;
        }
    }

    void GoToRoll()
    {
        buttonRollPressed = true;
        canRollDice = false;

        if (previousPhase == TurnPhases.INITIAL && !rollingForItem)
            initialSetupDone = true;

        if (dieScript.rollComplete && canRollDice)
        {
            wasDiceRolled = true;
            dieScript.rollComplete = false;
        }

        die.GetComponent<ApplyRandomForce>().RollDie();        
    }

    public void DieRollState()
    {
        if (previousPhase != TurnPhases.ROLLINGDIE)
        {
            previousPhase = currentPhase;
        }

        currentPhase = TurnPhases.ROLLINGDIE;
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
            case "finalBoss":
                activeFinalBossSprite = _card;
                _card.transform.SetParent(canvasInBattle.transform, false);
                break;
        }
            
    }

    public void DisplayText(string textType, string text)
    {
        switch (textType)
        {            
            case "system":
                system.GetComponent<Text>().text = text;
                break;
            case "systemInBattle":
                systemInBattle.GetComponent<Text>().text = text;
                break;
            case "cardDescription":
                cardDescription.GetComponent<Text>().text = text;
                break;
        }
    }

    public void DisplayStats(FighterCard card, string textToEdit)
    {
        string textToDraw = card.fighterName + "\n" + "HP" + "<color=red>" + card.hp + "</color>" + "\n"
                                + "ATK" + "<color=orange>" + card.GetCurrentStats().attack+ "</color>" + "/"
                                + "DEF" + "<color=blue>" + card.GetCurrentStats().defense + "</color>" + "/"
                                + "EVS" + "<color=green>" + card.GetCurrentStats().evasion + "</color>";

        switch (textToEdit)
        {
            case "attacker":
                attackerStats.GetComponent<Text>().text = textToDraw;
                break;
            case "enemy":
                enemyStats.GetComponent<Text>().text = textToDraw;
                break;
            case "whiteHood":
                whiteHoodStats.GetComponent<Text>().text = textToDraw;
                break;
            case "blackHood":
                blackHoodStats.GetComponent<Text>().text = textToDraw;
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

    void CheckThornmail()
    {
        if (targetCard.abilities.Count > 0)
        {
            Debug.Log(targetCard.fighterName);
            foreach (string ability in targetCard.abilities)
            {
                string[] separator = new string[] { " " };

                string abilityName = ability.Split(separator, System.StringSplitOptions.None)[0];
                int modifier = System.Convert.ToInt32(ability.Split(separator, System.StringSplitOptions.None)[1]);

                if (abilityName == "thornmail")
                {
                    AbilityScript.ActivateArtifactAbility(ability);
                }
            }
        }
    }

    public void LoseArtifactToOpponent()
    {
        if (characterScript.artifactsOwned.Count > 0 && characterScript.artifactsOwned.Count != 4)
        {
            int lostCardIndex = Random.Range(0, characterScript.artifactsOwned.Count);

            opponentScript.artifactsOwned.Add(characterScript.artifactsOwned[lostCardIndex]);
            if (characterScript.artifactsOwned[lostCardIndex].nature == ArtifactNature.Passive)
            {
                opponentScript.card.abilities.Add(characterScript.artifactsOwned[lostCardIndex].ability);
            }
            characterScript.LoseArtifact(lostCardIndex);
        }
    }

    public void StealArtifactFromOpponent()
    {
        if (opponentScript.artifactsOwned.Count > 0 && opponentScript.artifactsOwned.Count != 4)
        {
            int lostCardIndex = Random.Range(0, opponentScript.artifactsOwned.Count);

            characterScript.artifactsOwned.Add(opponentScript.artifactsOwned[lostCardIndex]);
            if (opponentScript.artifactsOwned[lostCardIndex].nature == ArtifactNature.Passive)
            {
                characterScript.card.abilities.Add(opponentScript.artifactsOwned[lostCardIndex].ability);
            }
            opponentScript.LoseArtifact(lostCardIndex);
        }
    }

    public void PickAttackStat()
    {
        if (currentPhase == TurnPhases.INITIAL)
        {
            int morphIndex = selectedArtifactCard.GetComponent<InHandCardScript>().inHandIndex;

            characterScript.artifactsOwned[morphIndex].stats.attack = 1;
            characterScript.artifactsOwned[morphIndex].stats.defense = 0;
            characterScript.artifactsOwned[morphIndex].stats.evasion = 0;
            characterScript.Equip(characterScript.artifactsOwned[morphIndex]);

            canvasPickStat.GetComponent<Canvas>().enabled = false;
            canRollDice = true;
        }
        else if (currentPhase == TurnPhases.BATTLE)
        {
            Stats buff;
            buff.attack = tempBuffModifier;
            buff.defense = 0;
            buff.evasion = 0;
            buff.maxHp = 0;

            characters[currentPlayerPickingCards % 2].GetComponent<Character>().card.Buff(buff);

            if(characters[currentPlayerPickingCards % 2].GetComponent<Character>().card == attackerCard)
            {
                DisplayStats(characters[currentPlayerPickingCards % 2].GetComponent<Character>().card, "attacker");
            }
            else
            {
                DisplayStats(characters[currentPlayerPickingCards % 2].GetComponent<Character>().card, "enemy");
            }

            currentPlayerPickingCards += 1;
            ResetInitialObjects();

            tempBuffModifier = 0;

            currentBattlePhase = BattlePhases.INITIAL;

            canvasPickStat.GetComponent<Canvas>().enabled = false;
        }
    }

    public void PickDefenseStat()
    {
        if (currentPhase == TurnPhases.INITIAL)
        {
            int morphIndex = selectedArtifactCard.GetComponent<InHandCardScript>().inHandIndex;

            characterScript.artifactsOwned[morphIndex].stats.attack = 0;
            characterScript.artifactsOwned[morphIndex].stats.defense = 1;
            characterScript.artifactsOwned[morphIndex].stats.evasion = 0;
            characterScript.Equip(characterScript.artifactsOwned[morphIndex]);

            canvasPickStat.GetComponent<Canvas>().enabled = false;
            canRollDice = true;
        }
        else if (currentPhase == TurnPhases.BATTLE)
        {
            Stats buff;
            buff.attack = 0;
            buff.defense = tempBuffModifier;
            buff.evasion = 0;
            buff.maxHp = 0;

            characters[currentPlayerPickingCards % 2].GetComponent<Character>().card.Buff(buff);

            if (characters[currentPlayerPickingCards % 2].GetComponent<Character>().card == attackerCard)
            {
                DisplayStats(characters[currentPlayerPickingCards % 2].GetComponent<Character>().card, "attacker");
            }
            else
            {
                DisplayStats(characters[currentPlayerPickingCards % 2].GetComponent<Character>().card, "enemy");
            }

            currentPlayerPickingCards += 1;
            ResetInitialObjects();

            tempBuffModifier = 0;

            currentBattlePhase = BattlePhases.INITIAL;

            canvasPickStat.GetComponent<Canvas>().enabled = false;
        }
    }

    public void PickEvasionStat()
    {
        if (currentPhase == TurnPhases.INITIAL)
        {            
            int morphIndex = selectedArtifactCard.GetComponent<InHandCardScript>().inHandIndex;

            characterScript.artifactsOwned[morphIndex].stats.attack = 0;
            characterScript.artifactsOwned[morphIndex].stats.defense = 0;
            characterScript.artifactsOwned[morphIndex].stats.evasion = 1;
            characterScript.Equip(characterScript.artifactsOwned[morphIndex]);

            canvasPickStat.GetComponent<Canvas>().enabled = false;
            canRollDice = true;
        }
        else if (currentPhase == TurnPhases.BATTLE)
        {
            Stats buff;
            buff.attack = 0;
            buff.defense = 0;
            buff.evasion = tempBuffModifier;
            buff.maxHp = 0;

            characters[currentPlayerPickingCards % 2].GetComponent<Character>().card.Buff(buff);

            if (characters[currentPlayerPickingCards % 2].GetComponent<Character>().card == attackerCard)
            {
                DisplayStats(characters[currentPlayerPickingCards % 2].GetComponent<Character>().card, "attacker");
            }
            else
            {
                DisplayStats(characters[currentPlayerPickingCards % 2].GetComponent<Character>().card, "enemy");
            }

            currentPlayerPickingCards += 1;
            ResetInitialObjects();

            tempBuffModifier = 0;

            currentBattlePhase = BattlePhases.INITIAL;

            canvasPickStat.GetComponent<Canvas>().enabled = false;
        }
    }

    public void ProceedInBattle()
    {
        DontUseItemInBattleButton.gameObject.SetActive(false);

        currentPlayerPickingCards += 1;
        ResetInitialObjects();
    }
}