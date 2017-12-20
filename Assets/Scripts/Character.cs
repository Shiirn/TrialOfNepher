using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character {

    public int boardX;
    public int boardY;
    public GameObject character;
    public GameObject currentPanel;

    public Character(GameObject _character, GameObject _homePanel)
    {
        character = _character;
        boardX = (int)_homePanel.transform.position.x;
        boardY = (int)_homePanel.transform.position.z;
        currentPanel = _homePanel;

        character.transform.position = new Vector3(boardX, 1f, boardY);
    }

    public void Update()
    {

    }
}