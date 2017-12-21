using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    //Manager Variables
    int turn;
    int activePlayer = 0;

    //Characters Objects
    public GameObject[] characterPrefabs;
    List<Character> characters = new List<Character>();

    //Board Objects
    public GameObject board;
    List<GameObject> homePanels = new List<GameObject>();

    //Board Scripts
    BoardMap boardMap;
    
    void Start ()
    {
        boardMap = board.GetComponent<BoardMap>();
        GetHomePanels();
        SpawnCharacters();

        //Temporarily moving the character in Start
        RollForMovement();

        if (activePlayer == 0)
            activePlayer++;
        else
            activePlayer--;
    }
	
	void Update () {
        
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
            characters.Add(new Character(Instantiate(characterPrefabs[i]), homePanels[i]));
        }
    }

    void RollForMovement()
    {
        int diceRoll = 5;
        for (int i = 0; i < diceRoll; i++)
        {
            Panel currentPanel = boardMap.GetPanel(characters[activePlayer].boardX, 
                                                   characters[activePlayer].boardY);
            characters[activePlayer].isMoving = true;

            switch (currentPanel.direction)
            {
                case "right":
                    characters[activePlayer].boardX--;
                    MoveCharacterX(-1);
                    break;
                case "downRight":
                    //goes right
                    characters[activePlayer].boardX--;
                    MoveCharacterX(-1);
                    break;
                case "down":
                    characters[activePlayer].boardY++;
                    MoveCharacterY(1);
                    break;
                case "up":
                    characters[activePlayer].boardY--;
                    MoveCharacterY(-1);
                    break;
                case "upRight":
                    //goes up
                    characters[activePlayer].boardY--;
                    MoveCharacterY(-1);
                    break;
                case "upLeft":
                    //goes left
                    characters[activePlayer].boardX++;
                    MoveCharacterX(1);
                    break;
                case "left":
                    characters[activePlayer].boardX++;
                    MoveCharacterX(1);
                    break;
                case "downLeft":
                    //goes down
                    characters[activePlayer].boardY++;
                    MoveCharacterY(1);
                    break;
            }
        }
    }

    void MoveCharacterX(int movement)
    {
        characters[activePlayer].character.transform.Translate(movement, 0, 0);
    }

    void MoveCharacterY(int movement)
    {
        characters[activePlayer].character.transform.Translate(0, 0, movement);
    }
}
