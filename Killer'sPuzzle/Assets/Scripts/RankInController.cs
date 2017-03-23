using UnityEngine;
using System.Collections;

public class RankInController : MonoBehaviour {

    public bool UpdateRanking(int score)
    {
        int[] rankScores = RankingScoreManager.Instance.TopScores;

        int rank = -1;

        // ランキングを塗り替える必要性が出てくるか確認する
        // 途中でbreakしたら塗り替える案件が発生する
        for(int i = 0; i < rankScores.Length; i++)
        {
            if (rankScores[i] < score)
            {
                // 上書きし始めるポイント(ランク)を記録
                rank = i;
                break;
            }
        }
        // 塗り替える必要がないなら偽の値を返す
        if (rank == rankScores.Length - 1)
        {
            return false;
        }
        
        // スコアの上書き(上書き箇所まで下から順に値を塗り替える)
        for(int i= rankScores.Length; i < rank; i--)
        {
            rankScores[i] = rankScores[i-1];
        }
        rankScores[rank] = score;
        // ランキングスコアデータバンクに上書き
        RankingScoreManager.Instance.TopScores = rankScores;
        // 返り値は真
        return true;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
