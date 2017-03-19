using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
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

    // 現在のチェイン数
    private int Chain = 0;
    // 現在の最大チェイン数
    private int MaxChain = 0;
    // Textコンポーネント
    [SerializeField]
    private Text ChainText, MaxChainText;

    // Use this for initialization
    void Start() {
        stage = new GameObject[StageSize, StageSize];
        deleteTable = new bool[StageSize, StageSize];
        spawner = new Spawner(ref blocks, StageSize, this.SpawnProbWeight);
        spawner.InitStage(ref stage);
        // 初期の盤面で揃っているブロックが無くなるまで盤面を修正
        for (; CheckStage();) {
            CheckStage();
        }
    }

    // Update is called once per frame
    void Update() {
        Flick();
        //CheckFallBlocks();
    }
    // 盤面初期化時の処理
    #region
    // 初期の盤面で揃っている所があるかチェック
    private bool CheckStage() {
        bool existMatch = false;
        // 比較元となるブロックの色
        Block origin;
        for (int i = 0; i < stage.GetLength(0); i++)
        {
            for (int j = 0; j < stage.GetLength(1); j++)
            {
                if (stage[i, j] != null)
                {
                    origin = stage[i, j].GetComponent<Block>();
                    // 配列の外を探索しない
                    if (j + 1 < StageSize && j > 0)
                    {
                        if (stage[i, j + 1] != null && stage[i, j - 1] != null)
                        {
                            // 左右のブロックが同じ色である場合,削除フラグを立てる
                            if (stage[i, j + 1].GetComponent<Block>().BlockType == origin.BlockType && stage[i, j - 1].GetComponent<Block>().BlockType == origin.BlockType)
                            {
                                existMatch = true;
                                ReplaceBlock(origin);
                            }
                        }
                    }
                    // 配列の外を探索しない
                    if (i + 1 < StageSize && i > 0)
                    {
                        if (stage[i + 1, j] != null && stage[i - 1, j] != null)
                        {
                            // 上下のブロックが同じ色である場合,削除フラグを立てる
                            if (stage[i + 1, j].GetComponent<Block>().BlockType == origin.BlockType && stage[i - 1, j].GetComponent<Block>().BlockType == origin.BlockType)
                            {
                                existMatch = true;
                                ReplaceBlock(origin);
                            }
                        }
                    }
                }
            }
        }
        return existMatch;
    }

    private void ReplaceBlock(Block target) {
        BlockPosition pos = target.BlockPosition;
        BlockType type = target.BlockType;
        Destroy(stage[pos.X, pos.Y]);
        spawner.BlockInfoProp.UpdateBlockInfoOnDelete(type);
        stage[pos.X, pos.Y] = spawner.GenerateBlock(BlockStatus.NORMAL, spawner.BlockInfoProp.CalcBlockType(type), pos.X, pos.Y);
    }
    #endregion

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
                     stage[selectedBlock.BlockPosition.X, selectedBlock.BlockPosition.Y + 1] == null) return;

                destBlock = stage[selectedBlock.BlockPosition.X, selectedBlock.BlockPosition.Y + 1].GetComponent<Block>();
                if (destBlock.BlockStatus != BlockStatus.NORMAL) return;
                break;
            default:
                return;
        }
        // 盤面(stage[,])上でのGameObjectを入れ替え
        if (selectedBlock.BlockStatus == BlockStatus.NORMAL && destBlock.BlockStatus == BlockStatus.NORMAL)
            SwitchBlock(selectedBlock, destBlock);
    }
    #endregion

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
        selected.BlockTransform.DOLocalMove(destPos, SwapTime / 60.0f);
        dest.BlockTransform.DOLocalMove(selectedPos, SwapTime / 60.0f).OnComplete(() => {
            // ブロックを入れ替えた時点で一旦ブロックの状態を戻す
            selected.BlockStatus = BlockStatus.NORMAL;
            dest.BlockStatus = BlockStatus.NORMAL;

            // 縦横にselectedBlock,destBlockを基準にして走査
            if (TraceBlocks())
            {
                // 揃っている箇所があるならブロックを削除
                CheckDeleteBlocks(selected, dest);
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
    private void RevertBlock(Block selected, Block dest) {
        // ブロックの状態を移動中のものに変える
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
        selected.BlockTransform.DOLocalMove(destPos, SwapTime / 60.0f);
        dest.BlockTransform.DOLocalMove(selectedPos, SwapTime / 60.0f).OnComplete(() => {
            // 移動終了後ブロックの状態を元に戻す
            selected.BlockStatus = BlockStatus.NORMAL;
            dest.BlockStatus = BlockStatus.NORMAL;
            // ブロックの位置を戻した時に落下するかどうか判別
            CheckFallBlocks();
        });
    }
    #endregion

    // ブロック走査、削除処理
    #region
    // 両隣が同じ色のブロックを探索する
    private bool TraceBlocks()
    {
        bool existMatch = false;
        // 比較元となるブロック
        Block origin;
        for (int i = 0; i < stage.GetLength(0); i++) {
            for (int j = 0; j < stage.GetLength(1); j++) {
                if (stage[i, j] != null)
                {
                    origin = stage[i, j].GetComponent<Block>();
                    // 配列の外を探索しない
                    if (j + 1 < StageSize && j > 0 && origin.BlockStatus == BlockStatus.NORMAL)
                    {
                        if (stage[i, j + 1] != null && stage[i, j - 1] != null)
                        {
                            Block right = stage[i, j + 1].GetComponent<Block>();
                            Block left = stage[i, j - 1].GetComponent<Block>();
                            if (right.BlockStatus == BlockStatus.NORMAL && left.BlockStatus == BlockStatus.NORMAL)
                            {
                                // 左右のブロックが同じ色である場合,削除フラグを立てる
                                if ((left.BlockType == origin.BlockType && right.BlockType == origin.BlockType))
                                {
                                    existMatch = true;
                                    deleteTable[i, j] = true;
                                    deleteTable[i, j + 1] = true;
                                    deleteTable[i, j - 1] = true;
                                }
                            }
                        }
                    }
                    // 真ん中のブロックの状態がNORMALでなければ次のブロックの探索を行う
                    else if (origin.BlockStatus != BlockStatus.NORMAL) continue;

                    // 配列の外を探索しない
                    if (i + 1 < StageSize && i > 0 && origin.BlockStatus == BlockStatus.NORMAL)
                    {
                        if (stage[i + 1, j] != null && stage[i - 1, j] != null)
                        {
                            Block up = stage[i - 1, j].GetComponent<Block>();
                            Block down = stage[i + 1, j].GetComponent<Block>();
                            if (up.BlockStatus == BlockStatus.NORMAL && down.BlockStatus == BlockStatus.NORMAL)
                            {
                                // 上下のブロックが同じ色である場合,削除フラグを立てる
                                if (up.BlockType == origin.BlockType && down.BlockType == origin.BlockType)
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
        }
        return existMatch;
    }
    // 削除フラグが立っているマス目のブロックを削除
    private void CheckDeleteBlocks()
    {
        Block[] targets = new Block[0];
        int x = 0;
        for (int i = StageSize - 1; i + 1 > 0; i--) {
            for (int j = 0; j < StageSize; j++) {
                // フラグの立っているマス目のブロックを削除
                if (deleteTable[i, j]) {
                    Array.Resize(ref targets, x + 1);
                    targets[x] = stage[i, j].GetComponent<Block>();
                    deleteTable[i, j] = false;
                    x++;
                }
            }
        }
        // 削除対象のブロックが存在するなら削除
        if (targets.Length > 2) {
            StartCoroutine(DeleteBlock(targets));
        }
    }
    // ブロック入れ替え時の削除処理
    private void CheckDeleteBlocks(Block selected, Block dest)
    {
        Block[] targets = new Block[0];
        int x = 0;
        for (int i = StageSize - 1; i + 1 > 0; i--)
        {
            for (int j = 0; j < StageSize; j++)
            {
                // フラグの立っているマス目のブロックを削除
                if (deleteTable[i, j])
                {
                    Array.Resize(ref targets, x + 1);
                    targets[x] = stage[i, j].GetComponent<Block>();
                    deleteTable[i, j] = false;
                    x++;
                }
            }
        }
        // 削除対象のブロックが存在するなら削除
        if (targets.Length > 2)
        {
            StartCoroutine(DeleteBlock(targets, selected, dest));
        }
    }
    // ブロック削除アニメーション
    IEnumerator DeleteBlock(Block[] deleteTargets)
    {
        BlockPosition[] empty = new BlockPosition[deleteTargets.Length];
        bool deleted = false;
        for (int i = 0; i < deleteTargets.Length; i++) {
            deleteTargets[i].BlockStatus = BlockStatus.DESTROY;
            empty[i] = deleteTargets[i].BlockPosition;
            spawner.BlockInfoProp.UpdateBlockInfoOnDelete(deleteTargets[i].BlockType);
            deleteTargets[i].BlockTransform.DOScale(Vector3.zero, DeleteTime).OnComplete(() => {
                deleted = true;
            });
        }
        // チェイン数を加算
        CurrentChainProp++;
        ChainText.text = "Chain " + this.CurrentChainProp.ToString();
        // MAXチェイン数が更新された場合の処理
        if (MaxChainProp < CurrentChainProp)
        {
            MaxChainProp = CurrentChainProp;
            MaxChainText.text = "MaxChain " + this.MaxChainProp.ToString();
        }

        // ブロック削除が完了するまで待機
        while (!deleted) { yield return new WaitForEndOfFrame(); }

        // ブロック削除が完了しているのでステージの状態を書き換える
        for (int i = 0; i < deleteTargets.Length; i++) {
            // 削除後はフラグを元に戻す
            //deleteTable[deleteTargets[i].BlockPosition.X, deleteTargets[i].BlockPosition.Y] = false;
            stage[deleteTargets[i].BlockPosition.X, deleteTargets[i].BlockPosition.Y] = null;
            Destroy(deleteTargets[i].BlockObject);
        }
        // 削除処理終了後ブロックの落下判定を行う
        CheckFallBlocks();
    }
    // ブロック削除アニメーション(ブロック入れ替え時)
    IEnumerator DeleteBlock(Block[] deleteTargets, Block selected, Block dest)
    {
        BlockPosition[] empty = new BlockPosition[deleteTargets.Length];
        bool deleted = false;
        for (int i = 0; i < deleteTargets.Length; i++)
        {
            deleteTargets[i].BlockStatus = BlockStatus.DESTROY;
            empty[i] = deleteTargets[i].BlockPosition;
            spawner.BlockInfoProp.UpdateBlockInfoOnDelete(deleteTargets[i].BlockType);
            deleteTargets[i].BlockTransform.DOScale(Vector3.zero, DeleteTime).OnComplete(() => {
                deleted = true;
            });
        }
        // チェイン数を加算
        CurrentChainProp++;
        ChainText.text = "Chain " + this.CurrentChainProp.ToString();
        // MAXチェイン数が更新された場合の処理
        if (MaxChainProp < CurrentChainProp)
        {
            MaxChainProp = CurrentChainProp;
            MaxChainText.text = "MaxChain " + this.MaxChainProp.ToString();
        }

        // ブロック削除が完了するまで待機
        while (!deleted) { yield return new WaitForEndOfFrame(); }

        // ブロック削除が完了しているのでステージの状態を書き換える
        for (int i = 0; i < deleteTargets.Length; i++)
        {
            // 削除後はフラグを元に戻す
            //deleteTable[deleteTargets[i].BlockPosition.X, deleteTargets[i].BlockPosition.Y] = false;
            stage[deleteTargets[i].BlockPosition.X, deleteTargets[i].BlockPosition.Y] = null;
            Destroy(deleteTargets[i].BlockObject);
        }

        //ブロックの状態を元に戻す
        if (selected != null)
            selected.BlockStatus = BlockStatus.NORMAL;
        if (dest != null)
            dest.BlockStatus = BlockStatus.NORMAL;
        // 削除処理終了後ブロックの落下判定を行う
        CheckFallBlocks();

    }
    #endregion

    // ブロック落下処理
    #region
    // 下のマスが空の箇所を探し、ブロックを落下させる
    private void CheckFallBlocks()
    {
        // ブロックの補充が必要な列を記録する配列
        bool[] ignoreSupply = new bool[StageSize];
        Block target;
        // 左の列から調べる
        for (int j = 0; j < StageSize; j++) {
            // 下の行から調べる
            for (int i = StageSize - 2; i + 1 > 0; i--) {
                // 下のマス目が空のブロックを探す
                if (stage[i, j] != null && stage[i + 1, j] == null) {
                    target = stage[i, j].GetComponent<Block>();
                    if (target.BlockStatus == BlockStatus.NORMAL)
                    {
                        target.BlockStatus = BlockStatus.FALLING;
                        // 下のブロックが空で無くなるか配列の一番下まで
                        for (int x = i; x + 1 < StageSize; x++)
                        {
                            if (stage[x + 1, j] == null)
                            {
                                stage[x + 1, j] = stage[x, j];
                                stage[x, j] = null;
                                target.BlockPosition = new BlockPosition(x + 1, j);
                            }
                        }

                    }
                    // ブロックの状態がNORMALでないものが見つかったら次の列を探索する
                    else
                    {
                        // この列にはブロックを補充しない
                        ignoreSupply[j] = true;
                        break;
                    }
                }
            }
        }


        // 次にブロックの補充が必要な位置にブロックを補充する
        CheckEmptyBlocks(ignoreSupply);


        for (int i = StageSize - 1; i + 1 > 0; i--) {
            for (int j = 0; j < StageSize; j++)
            {
                if (stage[i, j] != null) {
                    target = stage[i, j].GetComponent<Block>();
                    if (target.BlockStatus == BlockStatus.FALLING)
                    {
                        //stage[i,j] = spawner.SupplyBlock(BlockStatus.FALLING,spawner.BlockInfoProp.CalcBlockType(),i,j,)
                        //print("Fall target" + i + " : " + j);
                        FallBlock(target);
                    }
                }
            }
        }            
        /*全ての落下中のブロックの移動が完了するまで待機 いい方法が見つからない*/

    }

    // 指定したブロックを適切な位置まで落下させる
    private void FallBlock(Block target)
    {
        float nextPosY = spawner.GetInitPos.y -spawner.GetBlockSize.y*target.BlockPosition.X;
        int distance = (int)(Mathf.Abs(nextPosY - target.BlockTransform.anchoredPosition.y) / spawner.GetBlockSize.y);
        //print(distance + "マス落下");
        target.BlockTransform.DOLocalMoveY(nextPosY, FallTime*distance).OnComplete(() =>
        {
            // 移動後、状態を元に戻してやる
            target.BlockStatus = BlockStatus.NORMAL;


            // もし削除するブロックが存在するなら削除
            if (TraceBlocks())
            {
                CheckDeleteBlocks();
            }
            // これ以上削除するブロックが存在しない場合チェイン数を初期化
            else
            {
                CurrentChainProp = 0;
                ChainText.text = "";
            }
        });
    }
    #endregion

    // ブロック補充処理
    #region
    // ブロック補充位置を調べ、ブロックを補充する
    private void CheckEmptyBlocks(bool[] ignore) {
        // 縦方向にいくつブロックが連なっているか
        int connection= 0;
        // 左の列から探索
        for (int j = 0; j < StageSize; j++) {
            // ブロックを補充しない列は飛ばす
            if (!ignore[j])
            {
                // 下の行から探索
                for (int i = StageSize - 1; i + 1 > 0; i--)
                {
                    // ブロックが空の位置を見つけたら上方向に探索
                    if (stage[i, j] == null)
                    {
                        // 縦にいくつブロックが連なっているか調べる
                        for (int x = i; x + 1 > 0; x--)
                        {
                            // ブロックが空ならconnectionのカウントを増やす
                            if (stage[x, j] == null)
                            {
                                connection++;
                            }
                        }
                        // 縦に連なっているブロックの数だけ座標をずらしてブロックを補充
                        for (int x = i; x + 1 > 0; x--)
                        {
                            if (stage[x, j] == null)
                            {
                                stage[x, j] = spawner.SupplyBlock(BlockStatus.FALLING, spawner.BlockInfoProp.CalcBlockType(), x, j, connection);
                            }
                        }
                        // 生成が終わったらconnectionの値を初期化
                        connection = 0;
                    }
                }
            }
        }
    }
    #endregion


    public int CurrentChainProp
    {
        get { return this.Chain; }
        set { this.Chain = value; }
    }
    public int MaxChainProp
    {
        get { return this.MaxChain; }
        set { this.MaxChain = value; }
    }
}