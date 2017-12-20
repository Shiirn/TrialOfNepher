using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel {

    bool ascending = true;
    
    public int boardX;
    public int boardY;
    public GameObject panel;
    public string direction;

    public Panel(int x, int y, GameObject _panel, string _direction)
    {
        panel = _panel;
        boardX = x;
        boardY = y;
        direction = _direction;
    }

    public void Update()
    {
        Floaterino();
    }

    void Floaterino()
    {
        if (ascending)
        {
            panel.transform.Translate(0, +0.0008f, 0);
            if (panel.transform.position.y > 0.1f)
                ascending = false;
        }
        if (!ascending)
        {
            panel.transform.Translate(0, -0.0008f, 0);
            if (panel.transform.position.y < 0)
                ascending = true;
        }
    }
}

