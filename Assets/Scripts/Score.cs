using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject[] scoreObjects;
    private Text[] textComponents = new Text[4];
    private int[] scores = new int[4] { 0, 0, 0, 0 };
    void Start()
    {
        for (int i = 0; i < scores.Length; i++)
        {
            textComponents[i] = scoreObjects[i].GetComponent<Text>();
        }
        UpdateScores();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void setScores(int[] scores)
    {
        this.scores = scores;
        UpdateScores();
    }
    public void UpdateScores()
    {
        for (int i = 0; i < scores.Length; i++)
        {
            textComponents[i].text = scores[i].ToString();
        }
    }
}
