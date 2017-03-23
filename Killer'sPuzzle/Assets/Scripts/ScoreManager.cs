using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

    [SerializeField]
    // 基礎得点(ブロック3つ同時消しの得点)
    private int BaseScore = 10;
    [SerializeField]
    private int SyncBonus = 2;
    [SerializeField]
    private int MatingBonus;

    /*
     * 得点計算ルール
     * (1)4つ以上同時消しなら基礎得点*2
     * (2)(1)の得点にチェイン数掛けた値
     * (3)ステージの難易度に合わせて点数の倍率を変化させる？
     */

    // 合計得点
    private int TotalScore;
    [SerializeField]
    private Text ScoreText;

	// Use this for initialization
	void Start () {
        if (ScoreText == null)
            GameObject.Find("ScoreText").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // 現在のチェイン数と消したブロック数に応じて得点を加算
    public void AddScore(int ChainNum,int blockNum)
    {
        if (blockNum > 3) this.ScoreProp += BaseScore * ChainNum * SyncBonus;
        else this.ScoreProp += BaseScore * ChainNum;
        ScoreText.text = "Score " + this.ScoreProp;
    }

    // 詰み状態になったときのボーナス得点を加算
    public void AddBonusScore()
    {
        this.ScoreProp += MatingBonus;
    }

    public int ScoreProp
    {
        get { return this.TotalScore; }
        set { this.TotalScore = value; }
    }
}
