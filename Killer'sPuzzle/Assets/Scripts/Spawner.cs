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

    #region
    /// <summary>
    /// ブロックの生成確率を決め、ステージ上のブロックの個数を更新する
    /// </summary>
    /// <returns></returns>
    public BlockType CalcBlockType() {
        int total = 0;
        int rand = 0;
        average = (float)totalNum / blockTypes;
        // 各色の確率を求めて閾値を格納する
        float[] probs = new float[blockTypes];
        for (int i = 0; i < probs.Length; i++) {
            if(i == 0) probs[i] = parameter - weight * ((float)blockNums[i] - average);
            else probs[i] = probs[i - 1] + parameter - weight * ((float)blockNums[i] - average);
            total += (int)probs[i];
            //Debug.Log((BlockType)i + "prob? " + probs[i]);
        }
        // ステージ上のブロック数を増やす
        this.totalNum++;
        if (totalNum > 64) Debug.LogError("overflow");
        rand = UnityEngine.Random.Range(0, (int)probs[blockTypes - 1]);
        // 閾値を見て生成した乱数がどこに該当するか検索
        int type = 0;
        for (type = 0; type < probs.Length; type++) {
            // 生成された色のカウントを1つ増やす
            if (type == 0)
            {
                if (0 <= rand && rand < (int)probs[type])
                {
                    this.blockNums[type]++;
                    break;
                }
            }
            else if ((int)probs[type - 1] <= rand && rand <= (int)probs[type]) {
                this.blockNums[type]++;
                break;
            }
        }
        //Debug.LogError("Error in CalcBlockType" + "  totalNum? " + totalNum);
        return (BlockType)type;
    }

    /// <summary>
    /// exceptで指定したBlockType以外を返す
    /// 決定したBlockTypeよりブロックの個数を更新
    /// </summary>
    /// <param name="except"></param>
    /// <returns></returns>
    public BlockType CalcBlockType(BlockType except) {
        BlockType type = (BlockType)UnityEngine.Random.Range(0, blockTypes - 1);
        if (except != type) {
            totalNum++;
            return type;
        } 
        else return CalcBlockType(except);
    }
    #endregion

    /// <summary>
    /// ブロック削除時に呼ぶ関数
    /// typeで指定したブロックのカウントを1つ減らし
    /// ステージ上のブロックの総数も1つ減らす
    /// </summary>
    /// <param name="type"></param>
    public void UpdateBlockInfoOnDelete(BlockType type) {
        blockNums[(int)type]--;
        totalNum--;
    }

}
public class Spawner {

    // 生成するブロックのprefab
    private GameObject[] prefabs;
    // 親オブジェクトとなるCanvas
    private Transform parentCanvas;
    // ブロック生成の始点[0,0] ブロック1枚の大きさ
    private Vector2 initPos, offset;
    // ブロックの生成状況 次に生成するブロックを求めるのに使用
    private BlockInfo blockInfo;

    private int StageSize;
    // MainGameSystemからprefabの参照先を取得
    public Spawner(ref GameObject[] objs,int StageSize,int weight) {
        prefabs = objs;
        parentCanvas = GameObject.Find("Field").GetComponent<Transform>();
        // ステージの大きさなどの変数を計算
        initPos = new Vector2(-472.0f, 492.0f);
        // ブロック1枚の大きさを取得
        offset = objs[0].GetComponent<RectTransform>().sizeDelta;
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

    // ブロック生成関数
    #region
    /// <summary>
    /// ブロックのインスタンスを生成し
    /// 各パラメータを設定する関数
    /// </summary>
    /// <param name="status">盤面の状態</param>
    /// <param name="type">ブロックの色</param>
    /// <param name="posX">ブロックの位置X</param>
    /// <param name="posY">ブロックの位置Y</param>
    /// <returns></returns>
    public GameObject GenerateBlock(BlockStatus status,BlockType type,int posX,int posY)
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
    /// <summary>
    /// ブロック補充用関数
    /// </summary>
    /// <param name="status">FALLING</param>
    /// <param name="type">random</param>
    /// <param name="posX">補充先配列X</param>
    /// <param name="posY">補充先配列Y</param>
    /// <param name="rectX">フィールド座標X</param>
    /// <param name="rectY">フィールド座標Y</param>
    /// <returns></returns>
    public GameObject SupplyBlock(BlockStatus status, BlockType type, int posX, int posY,int connection)
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
        rect.anchoredPosition = initPos + new Vector2(offset.x * posY, -offset.y * posX + offset.y*connection);
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
    #endregion
    public BlockInfo BlockInfoProp {
        get { return this.blockInfo; }
    }

    public Vector2 GetInitPos {
        get { return this.initPos; }
    }

    public Vector2 GetBlockSize
    {
        get { return this.offset; }
    }
}
