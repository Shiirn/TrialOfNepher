using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    //Manager Variables
    int turn;
    int activePlayer = 0;

    //Characters Objects
    public GameObject[] characterPrefabs;
    List<GameObject> characters = new List<GameObject>();

    //Board Objects
    public GameObject board;
    List<GameObject> homePanels = new List<GameObject>();

    //Character Scripts
    Character characterScript;
    HomePanelIdentifier homePanelIdentifier;

    //Board Scripts
    BoardMap boardMap;
    Panel panelScript;
    
    void Start ()
    {
        boardMap = board.GetComponent<BoardMap>();
        GetHomePanels();
        SpawnCharacters();
        RollForMovement(150);
        

        //Temporarily moving the character in Start

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
            characters.Add(Instantiate(characterPrefabs[i]));
            homePanelIdentifier = characters[i].GetComponent<HomePanelIdentifier>();
            characterScript = characters[i].GetComponent<Character>();

            foreach(GameObject panel in boardMap.board)
            {
                if (panel.name.Contains(homePanelIdentifier.homePanel.name))
                {
                    panelScript = panel.GetComponent<Panel>();
                    characterScript.boardX = panelScript.boardX;
                    characterScript.boardY = panelScript.boardY;
                    characters[i].transform.position = new Vector3(
                        characterScript.boardX, 0.9f, characterScript.boardY);
                }
            }
        }
    }

    void RollForMovement(int diceRoll)
    {
        characterScript = characters[activePlayer].GetComponent<Character>();

        GameObject currentPanel = boardMap.GetPanel(characterScript.boardX,
                                                characterScript.boardY);

        panelScript = currentPanel.GetComponent<Panel>();
            
        switch (panelScript.direction)
        {
            case "right":
                characterScript.boardX--;
                StartCoroutine(Move(-1, 0, diceRoll));
                break;
            case "downRight":
                //goes right
                characterScript.boardX--;
                StartCoroutine(Move(-1, 0, diceRoll));
                break;
            case "down":
                characterScript.boardY++;
                StartCoroutine(Move(0, +1, diceRoll));
                break;
            case "up":
                characterScript.boardY--;
                StartCoroutine(Move(0, -1, diceRoll));
                break;
            case "upRight":
                //goes up
                characterScript.boardY--;
                StartCoroutine(Move(0, -1, diceRoll));
                break;
            case "upLeft":
                //goes left
                characterScript.boardX++;
                StartCoroutine(Move(+1, 0, diceRoll));
                break;
            case "left":
                characterScript.boardX++;
                StartCoroutine(Move(+1, 0, diceRoll));
                break;
            case "downLeft":
                //goes down
                characterScript.boardY++;
                StartCoroutine(Move(0, +1, diceRoll));
                break;
        }        
    }

    IEnumerator Move(int x, int y, int _diceRoll)
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

        if (_diceRoll > 1)
        {
            RollForMovement(_diceRoll - 1);
        }
        else
        {
            characterScript.isMoving = false;
        }
    }
}
