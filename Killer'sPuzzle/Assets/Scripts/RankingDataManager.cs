using UnityEngine;
using System.Collections;

public class RankingDataManager : SingletonMonoBehaviour<RankingDataManager> {

    private int[] scores;

    public int[] TopScores
    {
        get
        {
            return this.scores;
        }
        set
        {
            this.scores = value;
        }
    }
}
