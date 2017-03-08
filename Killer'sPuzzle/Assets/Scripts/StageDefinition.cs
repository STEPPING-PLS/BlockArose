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

}
