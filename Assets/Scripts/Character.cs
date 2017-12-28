using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Fighter {

    public int boardX;
    public int boardY;
    public GameObject currentPanel;
    public bool isMoving = false;
    public GameObject board;
    new public CharacterCard card;

    void Start()
    {

    }

    public void Update()
    {
        
    }

    public void SetCurrentPanel(GameObject _currentPanel)
    {
        currentPanel = _currentPanel;
    }
}