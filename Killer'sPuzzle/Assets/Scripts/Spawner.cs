using UnityEngine;
using System;

// ブロックの生成確率の計算に用いる
public class BlockInfo {
    private const float parameter = 50.0f;
    // 現在のステージ上のブロック数を各色ごとに記録しておく
    private byte[] blockNums;
    // ステージに生成されているブロックの合計数
    private int totalNum;
    private int blockTypes;
    // (totalNum / ブロックの種類)
    private float average;
    // 確率計算に用いる重みの値 GameControllerのスクリプトから取得
    private int weight;

    public BlockInfo(int probWeight) {
        blockTypes = Enum.GetNames(typeof(BlockType)).Length;
        blockNums = new byte[blockTypes];
        for (int i = 0; i < blockNums.Length; i++)
        {
            blockNums[i] = 0;
        }
        totalNum = 0;
        average = 0;
        weight = probWeight;
    }

    // 次に生成するブロックの種類を求める
    public BlockType CalcBlockType() {
        int total = 0;
        int rand = 0;
        // totalNumは現在生成しようとしているブロックの数に合わせる
        this.totalNum++;
        average = (float)totalNum / blockTypes;
        // 各色の確率を求めて閾値を格納する
        float[] probs = new float[blockTypes];
        for (int i = 0; i < probs.Length; i++) {
            if(i == 0) probs[i] = parameter - weight * ((float)blockNums[i] - average);
            else probs[i] = probs[i - 1] + parameter - weight * ((float)blockNums[i] - average);
            total += (int)probs[i];
            //Debug.Log((BlockType)i + "prob? " + probs[i]);
        }

        rand = UnityEngine.Random.Range(0, (int)probs[blockTypes - 1]);
        // 閾値を見て生成した乱数がどこに該当するか検索
        for (int i = 0; i < probs.Length; i++) {
            // 生成された色のカウントを1つ増やす
            if (i == 0)
            {
                if (0 <= rand && rand < (int)probs[i])
                {
                    this.blockNums[i]++;
                    return (BlockType)i;
                }
            }
            else if ((int)probs[i - 1] <= rand && rand < (int)probs[i]) {
                this.blockNums[i]++;
                return (BlockType)i;
            }
        }
        Debug.LogError("確率決まらなかった　コード間違ってるで");
        return BlockType.BLACK;
    }

    /// <summary>
    /// ブロック削除時に呼ぶ関数
    /// typeで指定したブロックのカウントを1つ減らし
    /// ステージ上のブロックの総数も1つ減らす
    /// </summary>
    /// <param name="type"></param>
    public void UpdateBlockInfo(BlockType type) {
        blockNums[(int)type]--;
        totalNum--;
    }
}
public class Spawner {

    // 生成するブロックのprefab
    private GameObject[] prefabs;
    // 親オブジェクトとなるCanvas
    private Transform parentCanvas;
    // ブロック生成の始点[0,0] ブロック1枚の大きさ ステージ1枚分の大きさ
    private Vector2 initPos, offset, stageScale;
    // ブロックの生成状況 次に生成するブロックを求めるのに使用
    private BlockInfo blockInfo;

    private int StageSize;
    // MainGameSystemからprefabの参照先を取得
    public Spawner(ref GameObject[] objs,int StageSize,int weight) {
        prefabs = objs;
        parentCanvas = GameObject.Find("Canvas").GetComponent<Transform>();
        // ステージの大きさなどの変数を計算
        initPos = new Vector2(-472.0f, 492.0f);
        offset = new Vector2(Screen.width / StageSize, Screen.width / StageSize);
        this.blockInfo = new BlockInfo(weight);
    }

    // 盤面初期化関数
    #region
    public void InitStage(ref GameObject[,] stage,ref BlockStatus[,] status)
    {
        for (int i = 0; i < stage.GetLength(0); i++) {
            for (int j = 0; j < stage.GetLength(1); j++) {
                stage[i, j] = GenerateBlock(status[i, j], blockInfo.CalcBlockType(), i, j);
            }
        }
    }
    // FORDEBUG 使用できないマスがない場合
    public void InitStage(ref GameObject[,] stage)
    {
        for (int i = 0; i < stage.GetLength(0); i++)
        {
            for (int j = 0; j < stage.GetLength(1); j++)
            {
                stage[i, j] = GenerateBlock(BlockStatus.NORMAL, blockInfo.CalcBlockType(), i, j);
            }
        }
    }
    #endregion

    /// <summary>
    /// ブロックのインスタンスを生成し
    /// 各パラメータを設定する関数
    /// </summary>
    /// <param name="status">盤面の状態</param>
    /// <param name="type">ブロックの色</param>
    /// <param name="posX">ブロックの位置X</param>
    /// <param name="posY">ブロックの位置Y</param>
    /// <returns></returns>
    private GameObject GenerateBlock(BlockStatus status,BlockType type,int posX,int posY)
    {
        Vector3 rectScale = new Vector3(1.0f, 1.0f, 1.0f);
        // 使用できないマスはダミーBlockを格納
        if (status == BlockStatus.DISABLED)
        {
            GameObject dummy = new GameObject("Disabled");
            Block block = dummy.AddComponent<Block>();
            block.BlockStatus = status;
            block.BlockPosition = new BlockPosition(posX, posY);
            block.BlockObject = dummy;
            return dummy;
        }
        // BlockTypeに応じたブロックを生成
        GameObject obj = UnityEngine.Object.Instantiate(this.prefabs[(int)type]);
        // Canvas内での位置と大きさを調整
        obj.transform.SetParent(parentCanvas);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.localScale = rectScale;
        rect.localPosition = Vector3.zero;
        rect.anchoredPosition = initPos + new Vector2(offset.x*posY,-offset.y*posX);
        /* Blockコンポーネントの値を適切な値にする
         * 配列内の座標,ブロックの種類,GameObjectの参照先を保存しておく
         */
        Block b = obj.AddComponent<Block>();
        b.BlockStatus = status;
        b.BlockType = type;
        b.BlockPosition = new BlockPosition(posX, posY);
        b.BlockTransform = rect;
        b.BlockObject = obj;
        return obj;
    }



    public BlockInfo BlockInfoProp {
        get { return this.blockInfo; }
    }
}
