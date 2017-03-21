using UnityEngine;
using System.Collections;

// 主にステージ編集に使う変数などを定義
public partial class MainGameSystem
{
    // 生成するブロック BlockTypeの値を添え字を一致させる
    [SerializeField]
    private GameObject[] blocks;
    // ステージの縦、横の長さを表す
    [SerializeField]
    private int StageSize = 8;
    // ステージのブロックの状態
    private BlockStatus[,] blockStatus;


    // ブロックの入れ替えにかかる時間
    [SerializeField]
    private float SwapTime = 2.0f;
    [SerializeField]
    // ブロック1マス落下にかかる時間
    private float FallTime = 1.0f;
    [SerializeField]
    // ブロック削除にかかる時間
    private float DeleteTime = 1.0f;
    // パズル生成確率計算に用いる重みの値
    /*
     * 生成確率Pは
     * P = 50 - N*(各色ブロックの数 - 各色ブロック数の平均値)
     * N = SpawnProbWeightとなる
     */
    [SerializeField]
    private int SpawnProbWeight = 25;

    [SerializeField]
    // コンボ継続時間
    private float ComboTimer = 0.0f;
    [SerializeField]
    // 落下終了後からコンボリセットまでの猶予時間
    private float ResetDelay = 0.1f;
}
