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
    public GameObject arrowPrefab;
    public List<GameObject> board = new List<GameObject>();
    public List<GameObject> arrows = new List<GameObject>();
    Panel panelScript;

    private void Awake()
    {
        SpawnPanels();
        SpawnArrows();
    }

    void SpawnPanels()
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
                    if (subStrings[x] == parsingPanel.text)
                    {
                        board.Add(Instantiate(parsingPanel.prefab,
                                        new Vector3(-x, Random.Range(0.0f, 0.1f), y),
                                        Quaternion.identity,
                                        transform));
                        panelScript = board[board.Count - 1].GetComponent<Panel>();
                        panelScript.direction = subDirectionStrings[x];
                    }                    
                }
            }
        }
    }

    void SpawnArrows()
    {
        foreach (GameObject panel in board)
        {
            panelScript = panel.GetComponent<Panel>();
            if (panelScript.direction == "downRight")
            {
                //Spawning arrow to the right
                SpawnArrowRight(panel);
                SpawnArrowDown(panel);
            }
            else if (panelScript.direction == "downLeft")
            {
                SpawnArrowLeft(panel);
                SpawnArrowDown(panel);
            }
            else if (panelScript.direction == "upLeft")
            {
                SpawnArrowLeft(panel);
                SpawnArrowUp(panel);
            }
            else if (panelScript.direction == "upRight")
            {
                SpawnArrowUp(panel);
                SpawnArrowRight(panel);
            }
        }
    }

    void SpawnArrowRight(GameObject panel)
    {
        arrows.Add(Instantiate(arrowPrefab, new Vector3(panel.transform.position.x,
                                                        0.4f,
                                                        panel.transform.position.z),
                                                        Quaternion.identity));

        arrows[arrows.Count - 1].transform.Translate(-0.9f, 0, 0);
        arrows[arrows.Count - 1].transform.Rotate(Vector3.right * 90);
        arrows[arrows.Count - 1].transform.Rotate(Vector3.forward * 180);
    }

    void SpawnArrowDown(GameObject panel)
    {
        arrows.Add(Instantiate(arrowPrefab, new Vector3(panel.transform.position.x,
                                                        0.4f,
                                                        panel.transform.position.z),
                                                        Quaternion.identity));

        arrows[arrows.Count - 1].transform.Translate(0, 0, +0.9f);
        arrows[arrows.Count - 1].transform.Rotate(Vector3.right * 90);
        arrows[arrows.Count - 1].transform.Rotate(Vector3.forward * 90);
    }

    void SpawnArrowLeft(GameObject panel)
    {
        arrows.Add(Instantiate(arrowPrefab, new Vector3(panel.transform.position.x,
                                                        0.4f,
                                                        panel.transform.position.z),
                                                        Quaternion.identity));

        arrows[arrows.Count - 1].transform.Translate(+0.9f, 0, 0);
        arrows[arrows.Count - 1].transform.Rotate(Vector3.right * 90);
    }

    void SpawnArrowUp(GameObject panel)
    {
        arrows.Add(Instantiate(arrowPrefab, new Vector3(panel.transform.position.x,
                                                        0.4f,
                                                        panel.transform.position.z),
                                                        Quaternion.identity));

        arrows[arrows.Count - 1].transform.Translate(0, 0, -0.9f);
        arrows[arrows.Count - 1].transform.Rotate(Vector3.right * 90);
        arrows[arrows.Count - 1].transform.Rotate(Vector3.forward * -90);
    }

    public GameObject GetPanel(string name)
    {
        foreach (GameObject panel in board)
        {
            if (name == panel.name)
            {
                return panel;
            }
        }
        return null;
    }

    public GameObject GetPanel(int x, int y)
    {
        //Debug.Log("From " + x + " " + y);
        foreach (GameObject panel in board)
        {
            panelScript = panel.GetComponent<Panel>();
            //Debug.Log(panelScript.boardX + " " + panelScript.boardY);

            if (x == panelScript.boardX && y == panelScript.boardY)
            {
                //Debug.Log("found at " + (panelScript.boardX) + " " + panelScript.boardY);
                return panel;
            }
        }
        return null;
    }
}