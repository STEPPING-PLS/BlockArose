using UnityEngine;
using System.Collections;
using System.IO;

public class RankingScoreManager : SingletonMonoBehaviour<RankingScoreManager>{

    [SerializeField]
    private string DataPath;

    private int[] score;

    public int[] TopScores
    {
        get
        {
            return this.score;
        }
        set
        {
            this.score = value;
        }
    }

	new void Awake()
    {
        this.score = new int[10];
        ReadScoresFromCSV(Application.dataPath + "/" + DataPath);
    }

    private void ReadScoresFromCSV(string path)
    {
        StreamReader sr = new StreamReader(path);
        string stream   = sr.ReadToEnd();

        char []option = new char[2] { '\n', '\r' };
        string []data = stream.Split(option);

        for(int i = 0; i < data.Length; i++)
        {
            this.score[i] = int.Parse(data[i]);
            print(i + ":" + this.score[i]);
        }
    }
}
