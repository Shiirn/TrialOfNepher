using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {

    bool ascending = false;
    int floaterinoCounter = 50;

    void Update()
    {
        Floaterino();
    }

    void Floaterino()
    {
        if (ascending)
        {
            gameObject.transform.Translate(+0.0015f,0, 0);
            floaterinoCounter++;
            if (floaterinoCounter > 50)
                ascending = false;
        }
        if (!ascending)
        {
            gameObject.transform.Translate(-0.0015f, 0, 0);
            floaterinoCounter--;
            if (floaterinoCounter < 0)
                ascending = true;
        }
    }
}
