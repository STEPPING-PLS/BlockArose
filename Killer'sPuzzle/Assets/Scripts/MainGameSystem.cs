using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;


public partial class MainGameSystem : MonoBehaviour {

    // パズルの盤面を表す配列 [i,j] = [PosX,PosY]
    /*
     * ↓→Y
     * X
     */
    private GameObject[,] stage;
    private bool[,] deleteTable;
    // パズル生成クラス
    private Spawner spawner;
    // ターン管理クラス
    [SerializeField]
    private TurnManager turnManager;
    // チェイン管理クラス
    [SerializeField]
    private ChainManager chainManager;

    // Use this for initialization
    void Start() {
        turnManager = this.gameObject.GetComponent<TurnManager>();
        chainManager = this.gameObject.GetComponent<ChainManager>();
        stage = new GameObject[StageSize, StageSize];
        deleteTable = new bool[StageSize, StageSize];
        spawner = new Spawner(ref blocks, StageSize, this.SpawnProbWeight);
        spawner.InitStage(ref stage);
    }

    /// <summary>
    /// 入れ替え先のブロックを取得する関数
    /// 取得できたらSwtichBlock関数を呼ぶ
    /// </summary>
    /// <param name="dir"></param>
    public void OnFlicked(FlickDirection dir,ref Block selectedBlock,ref Block destBlock)
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
            StartCoroutine(SwitchBlock(selectedBlock, destBlock));
    }

    // 盤面初期化時の処理
    #region
    //// 初期の盤面で揃っている所があるかチェック
    //private bool CheckStage() {
    //    bool existMatch = false;
    //    // 比較元となるブロックの色
    //    Block origin;
    //    for (int i = 0; i < stage.GetLength(0); i++)
    //    {
    //        for (int j = 0; j < stage.GetLength(1); j++)
    //        {
    //            if (stage[i, j] != null)
    //            {
    //                origin = stage[i, j].GetComponent<Block>();
    //                // 配列の外を探索しない
    //                if (j + 1 < StageSize && j > 0)
    //                {
    //                    if (stage[i, j + 1] != null && stage[i, j - 1] != null)
    //                    {
    //                        // 左右のブロックが同じ色である場合,削除フラグを立てる
    //                        if (stage[i, j + 1].GetComponent<Block>().BlockType == origin.BlockType && stage[i, j - 1].GetComponent<Block>().BlockType == origin.BlockType)
    //                        {
    //                            existMatch = true;
    //                            ReplaceBlock(origin);
    //                        }
    //                    }
    //                }
    //                // 配列の外を探索しない
    //                if (i + 1 < StageSize && i > 0)
    //                {
    //                    if (stage[i + 1, j] != null && stage[i - 1, j] != null)
    //                    {
    //                        // 上下のブロックが同じ色である場合,削除フラグを立てる
    //                        if (stage[i + 1, j].GetComponent<Block>().BlockType == origin.BlockType && stage[i - 1, j].GetComponent<Block>().BlockType == origin.BlockType)
    //                        {
    //                            existMatch = true;
    //                            ReplaceBlock(origin);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return existMatch;
    //}

    //private void ReplaceBlock(Block target) {
    //    BlockPosition pos = target.BlockPosition;
    //    BlockType type = target.BlockType;
    //    Destroy(stage[pos.X, pos.Y]);
    //    spawner.BlockInfoProp.UpdateBlockInfoOnDelete(type);
    //    stage[pos.X, pos.Y] = spawner.GenerateBlock(BlockStatus.NORMAL, spawner.BlockInfoProp.CalcBlockType(type), pos.X, pos.Y);
    //}
    #endregion

    // ブロック入れ替え処理
    #region
    IEnumerator SwitchBlock(Block selected, Block dest)
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
        Vector2 selectedPos = selected.BlockTransform.anchoredPosition;
        Vector2 destPos = dest.BlockTransform.anchoredPosition;
        selected.StartCoroutine(selected.MoveBlock(destPos,SwapTime));
        dest.StartCoroutine(dest.MoveBlock(selectedPos, SwapTime));
        // ブロック移動終了まで待機
        while (selected.BlockStatus != BlockStatus.NORMAL && dest.BlockStatus != BlockStatus.NORMAL) {
            yield return new WaitForEndOfFrame();
        }
        bool existMatch = (TraceBlocks(selected) | TraceBlocks(dest));
        // 縦横にselectedBlock,destBlockを基準にして走査
        if (existMatch)
        {
            // 揃っている箇所があるならブロックを削除
            CheckDeleteBlocks(selected, dest);
        }
        else
        {
            // 揃っていないならブロックを元に戻す
            StartCoroutine(RevertBlock(selected, dest));
        }
        yield return 0;

        /**********この辺でパズル入れ替えアニメーションなどを挟む*************/
    }
    // ブロックが揃ってない時元に戻す処理
    IEnumerator RevertBlock(Block selected, Block dest) {
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
        selected.StartCoroutine(selected.MoveBlock(destPos,SwapTime));
        dest.StartCoroutine(dest.MoveBlock(selectedPos, SwapTime));
        // 移動終了まで待機
        while (selected.BlockStatus != BlockStatus.NORMAL || dest.BlockStatus != BlockStatus.NORMAL)
        {
            yield return new WaitForEndOfFrame();
        }

        // ブロックの位置を戻した時に落下するかどうか判別
        StartCoroutine(CheckFallBlocks());
        yield return 0;
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
    // ブロック入れ替え時に
    private bool TraceBlocks(Block target)
    {
        bool existMatch = false;
        Block origin;
        // 上下に探索
        for (int i = target.BlockPosition.X - 1; i < target.BlockPosition.X + 2; i++)
        {
            if ((i < StageSize && i + 1 > 0) && stage[i, target.BlockPosition.Y] != null)
            {
                origin = stage[i, target.BlockPosition.Y].GetComponent<Block>();
                // 配列の外を探索しない,ブロックの状態がNORMALの場合
                if (i + 1 < StageSize && i > 0 
                    && origin.BlockStatus == BlockStatus.NORMAL)
                {
                    // 上下のブロックを探す
                    if (stage[i + 1, target.BlockPosition.Y] != null && stage[i - 1, target.BlockPosition.Y] != null)
                    {
                        Block up = stage[i - 1,target.BlockPosition.Y].GetComponent<Block>();
                        Block down = stage[i + 1, target.BlockPosition.Y].GetComponent<Block>();
                        if (up.BlockStatus == BlockStatus.NORMAL && down.BlockStatus == BlockStatus.NORMAL)
                        {
                            // 左右のブロックが同じ色である場合,削除フラグを立てる
                            if ((up.BlockType == origin.BlockType && down.BlockType == origin.BlockType))
                            {
                                existMatch = true;
                                deleteTable[i, target.BlockPosition.Y] = true;
                                deleteTable[i + 1, target.BlockPosition.Y] = true;
                                deleteTable[i - 1, target.BlockPosition.Y] = true;
                            }
                        }
                    }
                }
            }
        }
        // 左右に探索
        for (int j = target.BlockPosition.Y - 1; j < target.BlockPosition.Y + 2; j++)
        {
            if ((j < StageSize && j + 1 > 0) && stage[target.BlockPosition.X, j] != null)
            {
                origin = stage[target.BlockPosition.X, j].GetComponent<Block>();
                // 配列の外を探索しない,ブロックの状態がNORMALの場合
                if (j + 1 < StageSize && j > 0
                    && origin.BlockStatus == BlockStatus.NORMAL)
                {
                    // 左右のブロックを探す
                    if (stage[target.BlockPosition.X,j - 1] != null && stage[target.BlockPosition.Y,j+1] != null)
                    {
                        Block left = stage[target.BlockPosition.X,j-1].GetComponent<Block>();
                        Block right = stage[target.BlockPosition.X,j+1].GetComponent<Block>();
                        if (left.BlockStatus == BlockStatus.NORMAL && right.BlockStatus == BlockStatus.NORMAL)
                        {
                            // 左右のブロックが同じ色である場合,削除フラグを立てる
                            if ((left.BlockType == origin.BlockType && right.BlockType == origin.BlockType))
                            {
                                existMatch = true;
                                deleteTable[target.BlockPosition.X, j] = true;
                                deleteTable[target.BlockPosition.X, j - 1] = true;
                                deleteTable[target.BlockPosition.X, j + 1] = true;
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

        // ターンの継続判定,チェイン数の更新を行う
        if (turnManager.ChainTimerProp > 0)
        {
            // チェイン継続処理
            chainManager.PlusChainNum();
            // タイマーにチェイン継続時間をセットする
            turnManager.SetChainTimer(DeleteTime + FallTime*GetVerticalMatch());
        }
        else
        {
            // チェインリセット処理
            chainManager.ResetChainNum();
            // タイマーにチェイン継続時間をセットする
            turnManager.SetChainTimer(DeleteTime + FallTime * GetVerticalMatch());
        }

        // チェイン判定終了後ブロックの削除を開始する
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

        // ターンの継続判定,チェイン数の更新を行う
        if (turnManager.ChainTimerProp > 0)
        {
            // チェイン継続処理
            chainManager.PlusChainNum();
            // タイマーにチェイン継続時間をセットする
            turnManager.SetChainTimer(DeleteTime + FallTime * GetVerticalMatch());
        }
        else
        {
            // チェインリセット処理
            chainManager.ResetChainNum();
            // タイマーにチェイン継続時間をセットする
            turnManager.SetChainTimer(DeleteTime + FallTime * GetVerticalMatch());
        }
        // チェイン判定終了後ブロックの落下を開始
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

    // 削除するブロックが縦にいくつ繋がっているか数える関数
    private int GetVerticalMatch() {
        int match = 0;
        for (int j = 0; j < StageSize; j++) {
            int cnt = 0;
            for (int i = 0; i < StageSize; i++) {
                if (deleteTable[i, j]) {
                    cnt++;
                    if (match < cnt) match = cnt;
                }
            }
        }
        return match;
    }
    // ブロック削除アニメーション
    IEnumerator DeleteBlock(Block[] deleteTargets)
    {
        BlockPosition[] empty = new BlockPosition[deleteTargets.Length];
        for (int i = 0; i < deleteTargets.Length; i++) {
            deleteTargets[i].BlockStatus = BlockStatus.DESTROY;
            empty[i] = deleteTargets[i].BlockPosition;
            spawner.BlockInfoProp.UpdateBlockInfoOnDelete(deleteTargets[i].BlockType);
            deleteTargets[i].StartCoroutine(deleteTargets[i].DeleteBlock(DeleteTime));
        }
        /*
        // チェイン数を加算
        CurrentChainProp++;
        ChainText.text = "Chain " + this.CurrentChainProp.ToString();
        // MAXチェイン数が更新された場合の処理
        if (MaxChainProp < CurrentChainProp)
        {
            MaxChainProp = CurrentChainProp;
            MaxChainText.text = "MaxChain " + this.MaxChainProp.ToString();
        }
        */

        // ブロック削除が完了するまで待機
        while (deleteTargets[0].BlockStatus != BlockStatus.DELETED) { yield return new WaitForEndOfFrame(); }

        // ブロック削除が完了しているのでステージの状態を書き換える
        for (int i = 0; i < deleteTargets.Length; i++) {
            // 削除後はフラグを元に戻す
            //deleteTable[deleteTargets[i].BlockPosition.X, deleteTargets[i].BlockPosition.Y] = false;
            stage[deleteTargets[i].BlockPosition.X, deleteTargets[i].BlockPosition.Y] = null;
            Destroy(deleteTargets[i].BlockObject);
        }
        // 削除処理終了後ブロックの落下判定を行う
        StartCoroutine(CheckFallBlocks());
    }
    // ブロック削除アニメーション(ブロック入れ替え時)
    IEnumerator DeleteBlock(Block[] deleteTargets, Block selected, Block dest)
    {
        BlockPosition[] empty = new BlockPosition[deleteTargets.Length];
        for (int i = 0; i < deleteTargets.Length; i++)
        {
            deleteTargets[i].BlockStatus = BlockStatus.DESTROY;
            empty[i] = deleteTargets[i].BlockPosition;
            spawner.BlockInfoProp.UpdateBlockInfoOnDelete(deleteTargets[i].BlockType);
            deleteTargets[i].StartCoroutine(deleteTargets[i].DeleteBlock(DeleteTime));
        }

        /*
        // チェイン数を加算
        CurrentChainProp++;
        ChainText.text = "Chain " + this.CurrentChainProp.ToString();
        // MAXチェイン数が更新された場合の処理
        if (MaxChainProp < CurrentChainProp)
        {
            MaxChainProp = CurrentChainProp;
            MaxChainText.text = "MaxChain " + this.MaxChainProp.ToString();
        }
        */

        // ブロック削除が完了するまで待機
        while (deleteTargets[0].BlockStatus != BlockStatus.DELETED) { yield return new WaitForEndOfFrame(); }

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
        StartCoroutine(CheckFallBlocks());

    }
    #endregion

    // ブロック落下処理
    #region
    // 下のマスが空の箇所を探し、ブロックを落下させる
    IEnumerator CheckFallBlocks()
    {
        // ブロックの補充が必要な列を記録する配列
        bool[] ignoreSupply = new bool[StageSize];
        Block target;
        // 落下中のブロック
        Block[] falling = new Block[0];
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
                        // 配列の大きさを調整,落下対象のブロックを格納
                        Array.Resize<Block>(ref falling, falling.Length + 1);
                        falling[falling.Length - 1] = target;
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
        CheckEmptyBlocks(ignoreSupply,ref falling);

        // 落下対象のブロックを落下させる
        for (int i = 0;i < falling.Length;i++) {
            // 移動先の座標と落下時間を計算
            float nextPosY = spawner.GetInitPos.y - spawner.GetBlockSize.y * falling[i].BlockPosition.X;
            Vector2 dest = new Vector2(falling[i].BlockTransform.anchoredPosition.x,nextPosY);
            int distance = (int)(Mathf.Abs(nextPosY - falling[i].BlockTransform.anchoredPosition.y) / spawner.GetBlockSize.y);
            // 指定した座標と時間で落下
            StartCoroutine(falling[i].MoveBlock(dest, FallTime * distance));
        }
        //全ての落下中のブロックの移動が完了するまで待機
        for (int i = 0;i < falling.Length;) {
            // 落下中のブロックを見つけたら待機
            while(falling[i].BlockStatus == BlockStatus.FALLING)
            {
                yield return new WaitForEndOfFrame();
            }
            i++;
        }


        // 落下終了後,もし削除するブロックが存在するなら削除
        if (TraceBlocks())
        {
            CheckDeleteBlocks();
        }
        // 削除するブロックが存在しない場合,詰み判定を行う
        else
        {
            // 詰みなら盤面をリセットする
            if (IsMating()) {
                DestroyAll(ref stage);
                // ブロックの数を初期化し盤面を再生成
                spawner.BlockInfoProp.InitBlockInfo();
                spawner.InitStage(ref stage);
            }
        }
    }
    #endregion

    // ブロック補充処理
    #region
    // ブロック補充位置を調べ、ブロックを補充する
    private void CheckEmptyBlocks(bool[] ignore,ref Block[] falling) {
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
                                // 補充されたブロックを落下中のブロックの配列に格納
                                Array.Resize<Block>(ref falling, falling.Length + 1);
                                falling[falling.Length - 1] = stage[x, j].GetComponent<Block>();
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

    // 詰み判定と詰み時の処理
    #region
    // 詰み状態ならtrueを返す
    private bool IsMating() {
        // ステージのブロックの色を配列に格納
        BlockType[,] colors = new BlockType[StageSize, StageSize];
        for(int i = 0;i < StageSize; i++)
        {
            for (int j = 0; j < StageSize; j++)
            {
                if(stage[i,j] != null)
                    colors[i, j] = stage[i, j].GetComponent<Block>().BlockType;
            }
        }
        for (int i = 0; i < StageSize; i++)
        {
            for (int j = 0; j < StageSize; j++)
            {
                // 上2マスが同じ色の場合
                if (i - 1 > 0 && colors[i - 1, j] == colors[i - 2, j])
                {
                    // 左のマスが上2マスと同じ色の場合
                    if (j > 0 && colors[i - 1, j] == colors[i, j - 1])
                        return false;
                    // 下のマスが上2マスと同じ色
                    else if (i + 1 < StageSize && colors[i - 1, j] == colors[i + 1, j])
                        return false;
                    // 右のマスが上2マスと同じ色
                    else if (j + 1 < StageSize && colors[i - 1, j] == colors[i, j + 1])
                        return false;
                }
                //以下同様に判定を行う
                // 左2マスが同じ色の場合
                if (j - 1 > 0 && colors[i, j - 1] == colors[i, j - 2])
                {
                    if (i > 0 && colors[i, j - 1] == colors[i - 1, j])
                        return false;
                    else if (i + 1 < StageSize && colors[i, j - 1] == colors[i + 1, j])
                        return false;
                    else if (j + 1 < StageSize && colors[i, j - 1] == colors[i, j + 1])
                        return false;
                }
                // 下2マスが同じ色の場合
                if (i + 2 < StageSize && colors[i + 1, j] == colors[i + 2, j])
                {
                    if (i > 0 && colors[i + 1, j] == colors[i - 1, j])
                        return false;
                    else if (j > 0 && colors[i + 1, j] == colors[i, j - 1])
                        return false;
                    else if (j + 1 < StageSize && colors[i + 1, j] == colors[i, j + 1])
                        return false;
                }
                // 右2マスが同じ色の場合
                if (j + 2 < StageSize && colors[i, j + 1] == colors[i, j + 2])
                {
                    if (i > 0 && colors[i, j + 1] == colors[i - 1, j])
                        return false;
                    else if (j > 0 && colors[i, j + 1] == colors[i, j - 1])
                        return false;
                    else if (i + 1 < StageSize && colors[i, j + 1] == colors[i + 1, j])
                        return false;
                }
                // 上下2マスが同じ色の場合
                if ((i > 0 && i + 1 < StageSize) && colors[i - 1, j] == colors[i + 1, j])
                {
                    if (j > 0 && colors[i - 1, j] == colors[i, j - 1])
                        return false;
                    else if (j + 1 < StageSize && colors[i - 1, j] == colors[i, j + 1])
                        return false;
                }
                // 左右2マスが同じ色の場合
                if ((j > 0 && j + 1 < StageSize) && colors[i, j - 1] == colors[i, j + 1])
                {
                    if (i > 0 && colors[i, j - 1] == colors[i - 1, j])
                        return false;
                    else if (i + 1 < StageSize && colors[i, j - 1] == colors[i + 1, j])
                        return false;
                }
            }
        }
        // 上の処理を抜けたら詰み状態
        return true;
    }
    private void DestroyAll(ref GameObject[,] stage)
    {
        for (int i = 0; i < StageSize; i++) {
            for (int j = 0; j < StageSize; j++) {
                Destroy(stage[i, j]);
            }
        }
        print("mated InitializingStage");
    }
    #endregion
}