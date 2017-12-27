using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour {

    bool ascending = true;
    
    public int boardX;
    public int boardY;
    public string direction;

    void Start()
    {
        boardX = (int)gameObject.transform.position.x;
        boardY = (int)gameObject.transform.position.z;
    }

    void Update()
    {
        Floaterino();
    }

    void Floaterino()
    {
        if (ascending)
        {
            gameObject.transform.Translate(0, +0.0008f, 0);
            if (gameObject.transform.position.y > 0.1f)
                ascending = false;
        }
        if (!ascending)
        {
            gameObject.transform.Translate(0, -0.0008f, 0);
            if (gameObject.transform.position.y < 0)
                ascending = true;
        }
    }

    public virtual void PanelEffect()
    {

    }
}

