using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

enum FlickDirection: byte
{
    TAP = 0,
    UP,
    DOWN,
    LEFT,
    RIGHT
}
enum FlickState
{
    FLICKING,
    FREE
}
enum StageStatus: byte
{
    CONTROLLABLE = 0,
    CONTROLLDISABLED
}
public partial class MainGameSystem : MonoBehaviour {

    // フリックの状態
    private FlickState flickState;
    // フリックの始点、終点の座標
    private Vector3 beginPoint,endPoint;
    // パズルの盤面を表す配列 [i,j] = [PosX,PosY]
    /*
     * ↓→Y
     * X
     */
    private GameObject[,] stage;
    private bool[,] deleteTable;
    // パズル生成クラス
    private Spawner spawner;

    // 現在選択中のブロック,移動先のブロック
    private Block selectedBlock,destBlock;


	// Use this for initialization
	void Start () {
        stage = new GameObject[StageSize, StageSize];
        deleteTable = new bool[StageSize, StageSize];
        spawner = new Spawner(ref blocks,StageSize,this.SpawnProbWeight);
        spawner.InitStage(ref stage);
    }
	
	// Update is called once per frame
	void Update () {
        Flick();
	}
    // フリック入力関連
    #region
    private void Flick()
    {
        // 左マウス押下時にブロックを取得
        if (Input.GetMouseButtonDown(0) && flickState == FlickState.FREE)
        {
            flickState = FlickState.FLICKING;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 10.0f);
            selectedBlock = null; // 選択中のブロックを一旦nullにしておく
            if (hit.collider != null)
            {
                if (hit.collider.tag.Equals("Block"))
                {
                    selectedBlock = hit.collider.gameObject.GetComponent<Block>();
                    print("selected Block: " + selectedBlock.BlockPosition.X + ":" + selectedBlock.BlockPosition.Y);
                    beginPoint = Input.mousePosition;
                }
            }
        }
        // 左マウスボタン離した時にフリック方向を取得
        if (Input.GetMouseButtonUp(0) && flickState == FlickState.FLICKING)
        {
            endPoint = Input.mousePosition;
            if (selectedBlock != null)
            {
                // 取得した距離からブロックを入れ替え
                OnFlicked(GetDirection(beginPoint, endPoint),selectedBlock);
            }
            // 処理が終わったら選択中のブロックと対象のブロックを初期化
            selectedBlock = null;
            destBlock = null;
            flickState = FlickState.FREE;
        }
    }

    private FlickDirection GetDirection(Vector3 begin, Vector3 end)
    {
        float dirX, dirY;
        dirX = end.x - begin.x;
        dirY = end.y - begin.y;
        if (Mathf.Abs(dirX) < 135.0f && Mathf.Abs(dirY) < 135.0f)
        {
            print("tapped : dirX : " + dirX + "dirY : " + dirY);
            return FlickDirection.TAP;
        } 
        // 横方向のフリックの場合
        if (Mathf.Abs(dirX) > Mathf.Abs(dirY))
        {
            // 左方向への入力
            if (dirX < 0)
            {
                return FlickDirection.LEFT;
            }
            else
            {
                return FlickDirection.RIGHT;
            }
        }
        // 縦方向のフリックの場合
        if (Mathf.Abs(dirX) < Mathf.Abs(dirY))
        {
            // 下方向への入力
            if (dirY < 0)
            {
                return FlickDirection.DOWN;
            }
            else
            {
                return FlickDirection.UP;
            }

        }
        else
        {
            return FlickDirection.TAP;
        }
    }
    #endregion

    /// <summary>
    /// 入れ替え先のブロックを取得する関数
    /// 取得できたらSwtichBlock関数を呼ぶ
    /// </summary>
    /// <param name="dir"></param>
    private void OnFlicked(FlickDirection dir,Block selectedBlock)
    {
        print("dir?" + dir);
        switch (dir)
        {
            case FlickDirection.UP:
                // 配列の外ならreturn
                if (selectedBlock.BlockPosition.X - 1 < 0 ||
                    stage[selectedBlock.BlockPosition.X - 1, selectedBlock.BlockPosition.Y] == null) return;

                destBlock = stage[selectedBlock.BlockPosition.X - 1, selectedBlock.BlockPosition.Y].GetComponent<Block>();
                break;
            case FlickDirection.LEFT:
                if (selectedBlock.BlockPosition.Y - 1 < 0 ||
                    stage[selectedBlock.BlockPosition.X, selectedBlock.BlockPosition.Y - 1] == null) return;

                destBlock = stage[selectedBlock.BlockPosition.X, selectedBlock.BlockPosition.Y - 1].GetComponent<Block>();
                break;
            case FlickDirection.DOWN:
                if (selectedBlock.BlockPosition.X + 1 >= stage.GetLength(0) ||
                    stage[selectedBlock.BlockPosition.X + 1, selectedBlock.BlockPosition.Y] == null) return;

                destBlock = stage[selectedBlock.BlockPosition.X + 1, selectedBlock.BlockPosition.Y].GetComponent<Block>();
                break;
            case FlickDirection.RIGHT:
                if (selectedBlock.BlockPosition.Y + 1 >= stage.GetLength(1) ||
                     stage[selectedBlock.BlockPosition.Y + 1, selectedBlock.BlockPosition.Y] == null) return;

                destBlock = stage[selectedBlock.BlockPosition.X, selectedBlock.BlockPosition.Y + 1].GetComponent<Block>();
                break;
            default:
                print("tap" + "dest?" + destBlock);
                return;
        }
        print("destBlock:" + destBlock.BlockPosition.X + ":" + destBlock.BlockPosition.Y);
        // 盤面(stage[,])上でのGameObjectを入れ替えて
        SwitchBlock(selectedBlock, destBlock);
        // 縦横にselectedBlock,destBlockを基準にして走査
        if (TraceBlocks())
        {
            // 揃っている箇所があるならブロックを削除
            DeleteBlock();
        }
        else {
            // 揃っていないならブロックを元に戻す
            SwitchBlock(destBlock, selectedBlock);
        }


    }
    private void SwitchBlock(Block selected,Block dest)
    {        
        // 配列内のGameObjectを入れ替え
        GameObject tmp = selected.BlockObject;
        stage[selected.BlockPosition.X, selected.BlockPosition.Y] = dest.BlockObject;
        stage[dest.BlockPosition.X, dest.BlockPosition.Y] = tmp;

        // 配列上での位置を入れ替え
        BlockPosition tmpPos = selected.BlockPosition;
        selected.BlockPosition = dest.BlockPosition;
        dest.BlockPosition = tmpPos;
        // ※ブロックの座標を入れ替える
        Vector3 selectedPos = selected.BlockTransform.anchoredPosition;
        Vector3 destPos = dest.BlockTransform.anchoredPosition;
        selected.BlockTransform.anchoredPosition = destPos;
        dest.BlockTransform.anchoredPosition = selectedPos;

        /**********この辺でパズル入れ替えアニメーションなどを挟む*************/

    }


    #region
    // 両隣が同じ色のブロックを探索する
    private bool TraceBlocks()
    {
        bool existMatch = false;
        // 比較元となるブロックの色
        BlockType originType;
        for (int i = 0; i < stage.GetLength(0); i++) {
            for (int j = 0; j < stage.GetLength(1); j++) {
                if (stage[i, j] != null)
                {
                    originType = stage[i, j].GetComponent<Block>().BlockType;
                    // 配列の外を探索しない
                    if (j + 1 < StageSize && j > 0)
                    {
                        if(stage[i,j+1] != null && stage[i,j - 1] != null)
                        {
                            // 左右のブロックが同じ色である場合,削除フラグを立てる
                            if (stage[i, j + 1].GetComponent<Block>().BlockType == originType && stage[i, j - 1].GetComponent<Block>().BlockType == originType)
                            {
                                existMatch = true;
                                deleteTable[i, j] = true;
                                deleteTable[i, j + 1] = true;
                                deleteTable[i, j - 1] = true;
                            }
                        }
                    }
                    // 配列の外を探索しない
                    if (i + 1 < StageSize && i > 0)
                    {
                        if (stage[i + 1, j] != null && stage[i - 1, j] != null)
                        {
                            // 上下のブロックが同じ色である場合,削除フラグを立てる
                            if (stage[i + 1, j].GetComponent<Block>().BlockType == originType && stage[i - 1, j].GetComponent<Block>().BlockType == originType)
                            {
                                existMatch = true;
                                deleteTable[i, j] = true;
                                deleteTable[i + 1, j] = true;
                                deleteTable[i - 1, j] = true;
                            }
                        }
                    }
                }
            }
        }
        return existMatch;
    }
    // 削除フラグが立っているマス目のブロクを削除
    private void DeleteBlock()
    {
        Block deleteTarget;
        for (int i = 0; i < StageSize; i++) {
            for (int j = 0; j < StageSize; j++) {
                // フラグの立っているマス目のブロックを削除
                if (deleteTable[i, j]) {
                    deleteTarget = stage[i, j].GetComponent<Block>();
                    // 盤面のブロック数の情報を更新
                    spawner.BlockInfoProp.UpdateBlockInfo(deleteTarget.BlockType);
                    Destroy(stage[i, j]);
                    // 削除後はフラグを元に戻す
                    deleteTable[i, j] = false;
                }
            }
        }
    }

    #endregion
}