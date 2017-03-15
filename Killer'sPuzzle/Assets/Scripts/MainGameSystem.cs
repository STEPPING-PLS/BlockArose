using UnityEngine;
using DG.Tweening;
using System.Collections;

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
    private Vector3 beginPoint, endPoint;
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
    private Block selectedBlock, destBlock;

    private Tween tween;

    // Use this for initialization
    void Start() {
        stage = new GameObject[StageSize, StageSize];
        deleteTable = new bool[StageSize, StageSize];
        spawner = new Spawner(ref blocks, StageSize, this.SpawnProbWeight);
        spawner.InitStage(ref stage);
        // 初期の盤面で揃っているブロックが無くなるまで盤面を修正
        for(; CheckStage();) {
            CheckStage();
        }
    }

    // Update is called once per frame
    void Update() {
        Flick();
        CheckFallBlocks();
    }
    // 初期の盤面で揃っている所があるかチェック
    private bool CheckStage() {
        bool existMatch = false;
        // 比較元となるブロックの色
        BlockType originType;
        for (int i = 0; i < stage.GetLength(0); i++)
        {
            for (int j = 0; j < stage.GetLength(1); j++)
            {
                if (stage[i, j] != null)
                {
                    originType = stage[i, j].GetComponent<Block>().BlockType;
                    // 配列の外を探索しない
                    if (j + 1 < StageSize && j > 0)
                    {
                        if (stage[i, j + 1] != null && stage[i, j - 1] != null)
                        {
                            // 左右のブロックが同じ色である場合,削除フラグを立てる
                            if (stage[i, j + 1].GetComponent<Block>().BlockType == originType && stage[i, j - 1].GetComponent<Block>().BlockType == originType)
                            {
                                existMatch = true;
                                Destroy(stage[i, j]);
                                stage[i, j] = spawner.GenerateBlock(BlockStatus.NORMAL, spawner.BlockInfoProp.CalcBlockType(originType), i, j);
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
                                Destroy(stage[i, j]);
                                stage[i, j] = spawner.GenerateBlock(BlockStatus.NORMAL, spawner.BlockInfoProp.CalcBlockType(originType), i, j);
                            }
                        }
                    }
                }
            }
        }
        return existMatch;
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
                    print("selected Block: " + selectedBlock.BlockPosition.X + ":" + selectedBlock.BlockPosition.Y + " BlockStatus : " + selectedBlock.BlockStatus);
                    beginPoint = Input.mousePosition;
                }
            }
        }
        // 左マウスボタン離した時にフリック方向を取得
        if (Input.GetMouseButtonUp(0) && flickState == FlickState.FLICKING)
        {
            endPoint = Input.mousePosition;
            // 選択中のブロックが存在し、BlockStatusがNORMALなら
            if (selectedBlock != null && selectedBlock.BlockStatus == BlockStatus.NORMAL)
            {
                // 取得した距離からブロックを入れ替え
                OnFlicked(GetDirection(beginPoint, endPoint), selectedBlock);
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
    private void OnFlicked(FlickDirection dir, Block selectedBlock)
    {
        print("dir?" + dir);
        switch (dir)
        {
            case FlickDirection.UP:
                // 配列の外,またはブロックが存在しない場合return
                if (selectedBlock.BlockPosition.X - 1 < 0 ||
                    stage[selectedBlock.BlockPosition.X - 1, selectedBlock.BlockPosition.Y] == null) return;

                destBlock = stage[selectedBlock.BlockPosition.X - 1, selectedBlock.BlockPosition.Y].GetComponent<Block>();
                // BlockStatusがNORMALじゃなければreturn
                if (destBlock.BlockStatus != BlockStatus.NORMAL) return;
                break;
            case FlickDirection.LEFT:
                if (selectedBlock.BlockPosition.Y - 1 < 0 ||
                    stage[selectedBlock.BlockPosition.X, selectedBlock.BlockPosition.Y - 1] == null) return;

                destBlock = stage[selectedBlock.BlockPosition.X, selectedBlock.BlockPosition.Y - 1].GetComponent<Block>();
                if (destBlock.BlockStatus != BlockStatus.NORMAL) return;
                break;
            case FlickDirection.DOWN:
                if (selectedBlock.BlockPosition.X + 1 >= stage.GetLength(0) ||
                    stage[selectedBlock.BlockPosition.X + 1, selectedBlock.BlockPosition.Y] == null) return;

                destBlock = stage[selectedBlock.BlockPosition.X + 1, selectedBlock.BlockPosition.Y].GetComponent<Block>();
                if (destBlock.BlockStatus != BlockStatus.NORMAL) return;
                break;
            case FlickDirection.RIGHT:
                if (selectedBlock.BlockPosition.Y + 1 >= stage.GetLength(1) ||
                     stage[selectedBlock.BlockPosition.Y + 1, selectedBlock.BlockPosition.Y] == null) return;

                destBlock = stage[selectedBlock.BlockPosition.X, selectedBlock.BlockPosition.Y + 1].GetComponent<Block>();
                if (destBlock.BlockStatus != BlockStatus.NORMAL) return;
                break;
            default:
                print("tap" + "dest?" + destBlock);
                return;
        }
        print("destBlock:" + destBlock.BlockPosition.X + ":" + destBlock.BlockPosition.Y);
        // 盤面(stage[,])上でのGameObjectを入れ替え
        if(selectedBlock.BlockStatus == BlockStatus.NORMAL && destBlock.BlockStatus == BlockStatus.NORMAL)
            SwitchBlock(selectedBlock, destBlock);
    }
    // ブロック入れ替え処理
    #region
    private void SwitchBlock(Block selected, Block dest)
    {
        //ブロックの状態を変える
        selected.BlockStatus = BlockStatus.SWTCHING;
        dest.BlockStatus = BlockStatus.SWTCHING;

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
        selected.BlockTransform.DOLocalMove(destPos, SwapTime);
        dest.BlockTransform.DOLocalMove(selectedPos, SwapTime).OnComplete(() => {
            // 縦横にselectedBlock,destBlockを基準にして走査

            if (TraceBlocks())
            {
                // 揃っている箇所があるならブロックを削除
                DeleteBlocks();
                //ブロックの状態を元に戻す
                if (selected != null)
                    selected.BlockStatus = BlockStatus.NORMAL;
                if (dest!= null)
                    dest.BlockStatus = BlockStatus.NORMAL;
            }
            else
            {
                // 揃っていないならブロックを元に戻す
                RevertBlock(selected, dest);
            }
        });

        /**********この辺でパズル入れ替えアニメーションなどを挟む*************/
    }
    // ブロックが揃ってない時元に戻す処理
    private void RevertBlock(Block selected,Block dest) {
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
        selected.BlockTransform.DOLocalMove(destPos, SwapTime);
        dest.BlockTransform.DOLocalMove(selectedPos, SwapTime).OnComplete(() => {
            //ブロックの状態を元に戻す
            selected.BlockStatus = BlockStatus.NORMAL;
            dest.BlockStatus = BlockStatus.NORMAL;
        });
    }
    #endregion

    // ブロック走査、削除処理
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
    // 削除フラグが立っているマス目のブロックを削除
    private void DeleteBlocks()
    {
        for (int i = 0; i < StageSize; i++) {
            for (int j = 0; j < StageSize; j++) {
                // フラグの立っているマス目のブロックを削除
                if (deleteTable[i, j]) {
                    DeleteAnimation(stage[i, j].GetComponent<Block>());
                }
            }
        }
    }
    // ブロック削除アニメーション
    private void DeleteAnimation(Block deleteTarget) {
        deleteTarget.BlockStatus = BlockStatus.DESTROY;
        // 盤面のブロック数の情報を更新
        spawner.BlockInfoProp.UpdateBlockInfo(deleteTarget.BlockType);
        deleteTarget.BlockTransform.DOScale(Vector3.zero, DeleteTime).OnComplete(() =>
        {
            // 削除後はフラグを元に戻す
            deleteTable[deleteTarget.BlockPosition.X, deleteTarget.BlockPosition.Y] = false;
            Destroy(deleteTarget.BlockObject);
        });
    }

    #endregion

    // ブロック落下処理
    #region
    // 下のマスが空の箇所を探し、ブロックを落下させる
    private void CheckFallBlocks()
    {
        Block target;
        for (int i = StageSize - 1; i + 1 > 0; i--) {
            for (int j = 0; j < StageSize; j++) {
                // ブロックが空のマスを探し、その上のマス
                if (stage[i, j] == null && i > 0 && stage[i - 1, j] != null) {
                    target = stage[i - 1, j].GetComponent<Block>();
                    if (target.BlockStatus == BlockStatus.NORMAL) {
                        tween.SetDelay(DelayAfterDeleted);
                        FallBlock(target);
                    }
                }
            }
        }
    }
    // 指定したブロックを適切な位置まで落下させる
    private void FallBlock(Block target)
    {
        BlockPosition nextPos = new BlockPosition(target.BlockPosition.X + 1, target.BlockPosition.Y);
        target.BlockStatus = BlockStatus.FALLING;
        // 下のマスが配列の外でなく、下のマスが空の場合
        for (; nextPos.X < StageSize && stage[nextPos.X, nextPos.Y] == null;) {
            // 次の座標を計算
            float nextPosY = target.BlockTransform.anchoredPosition.y - spawner.GetBlockSize.y;
            stage[nextPos.X, nextPos.Y] = target.BlockObject;
            stage[nextPos.X - 1, nextPos.Y] = null;
            // ブロック1マス分下へ移動(duration=ブロックの大きさ/落下速度)
            target.BlockTransform.DOLocalMoveY(nextPosY, FallSpeed).OnComplete(() => {
                // 移動し終わったら次のマスを見てさらに下へ移動できるか見る
                target.BlockPosition = nextPos;
                nextPos.X++;
            });
        }

        // これ以上下に移動できない場合終了
        target.BlockStatus = BlockStatus.NORMAL;
        // 落下後の盤面で消すブロックがあるかチェック
        if (TraceBlocks())
        {
            DeleteBlocks();
        }
    }
    #endregion

    // ブロック補充処理
    #region
    private void SupplyBlocks() {

    }
    #endregion
}