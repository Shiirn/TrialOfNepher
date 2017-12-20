using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class BoardMap : MonoBehaviour
{
    public TextAsset boardCsv;
    public TextAsset boardDirectionsCsv;
    public TextToPanel[] parsingPanels;
    public List<Panel> board = new List<Panel>();


    private void Awake()
    {
        FillList();
    }

    void FillList()
    {
        string[] stringSeparator = new string[] { "\r\n" };
        string[] strings = boardCsv.text.Split(stringSeparator, System.StringSplitOptions.None);
        string[] directionStrings = boardDirectionsCsv.text.Split(stringSeparator, System.StringSplitOptions.None);

        for (int y = 0; y < strings.Length; y++)
        {
            string[] subStrings = strings[y].Split(',');
            string[] subDirectionStrings = directionStrings[y].Split(',');

            for (int x = 0; x < subStrings.Length; x++)
            {
                foreach(TextToPanel parsingPanel in parsingPanels)
                {
                    if(subStrings[x] == parsingPanel.text)
                    {
                        board.Add(new Panel(x, y, 
                            (GameObject)Instantiate(parsingPanel.prefab, 
                                    new Vector3(-x, Random.Range(0.0f, 0.1f), y), 
                                    Quaternion.identity, transform), subDirectionStrings[x]));
                    }
                }
            }
        }
    }

    private void Update()
    {
        foreach(Panel panel in board)
        {
            panel.Update();
        }
    }


    public GameObject GetPanel(string name)
    {
        foreach (Panel panel in board)
        {
            if (name == panel.panel.name)
            {
                return panel.panel;
            }
        }
        return null;
    }

    public Panel GetPanel(int x, int y)
    {
        //Debug.Log("From " + x + " " + y);
        foreach (Panel panel in board)
        {
            
            if (x == -panel.boardX && y == panel.boardY)
            {
                //Debug.Log("found at " + (-panel.boardX) + " " + panel.boardY);
                return panel;
            }
        }
        return null;
    }
}