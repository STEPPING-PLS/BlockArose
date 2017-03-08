using UnityEngine;
using System.Collections;

public partial class Spawner {

    // 生成するブロックのprefab
    private GameObject[] prefabs;
    // 親オブジェクトとなるCanvas
    private Transform parentCanvas;
    // ブロック生成の始点[0,0] ブロック1枚の大きさ ステージ1枚分の大きさ
    private Vector2 initPos, offset, stageScale;
    private int StageSize;
    // MainGameSystemからprefabの参照先を取得
    public Spawner(ref GameObject[] objs,int StageSize) {
        prefabs = objs;
        parentCanvas = GameObject.Find("Canvas").GetComponent<Transform>();
        // ステージの大きさなどの変数を計算
        initPos = new Vector2(-472.0f, 492.0f);
        offset = new Vector2(Screen.width / StageSize, Screen.width / StageSize);
    }

    // 盤面初期化関数
    #region
    public void InitStage(ref Block[,] stage,ref BlockStatus[,] status)
    {
        for (int i = 0; i < stage.GetLength(0); i++) {
            for (int j = 0; j < stage.GetLength(1); j++) {
                stage[i, j] = GenerateBlock(status[i, j], (BlockType)Random.Range(0, prefabs.Length), i, j);
            }
        }
    }
    // FORDEBUG 使用できないマスがない場合
    public void InitStage(ref Block[,] stage)
    {
        for (int i = 0; i < stage.GetLength(0); i++)
        {
            for (int j = 0; j < stage.GetLength(1); j++)
            {
                stage[i, j] = GenerateBlock(BlockStatus.NORMAL, (BlockType)Random.Range(0, prefabs.Length), i, j);
            }
        }
    }
    #endregion

    private Block GenerateBlock(BlockStatus status,BlockType type,int posX,int posY)
    {
        Vector3 rectScale = new Vector3(1.0f, 1.0f, 1.0f);
        // 使用できないマスはダミーBlockを格納
        if (status == BlockStatus.DISABLED) return new Block(posX, posY);
        GameObject obj = Object.Instantiate(this.prefabs[(int)type]);
        obj.transform.SetParent(parentCanvas);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.localScale = rectScale;
        rect.localPosition = Vector3.zero;
        rect.anchoredPosition = initPos + new Vector2(offset.x*posX,-offset.y*posY);
        // ブロックの位置と型、ステータスを設定して返す
        Block b = new Block(posX, posY, type, status);
        b.BlockObject = obj;
        return b;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
