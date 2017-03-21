using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum BlockType : byte
{
    BLACK,
    BLUE,
    GREEN,
    PURPLE,
    RED,
    YELLOW
}
public enum BlockStatus : byte
{
    NORMAL,
    SWTCHING,
    DESTROY,
    DELETED,
    FALLING,
    DISABLED
}

// ブロックの配列内での座標を表す構造体
public struct BlockPosition
{
    public BlockPosition(int x, int y)
    {
        X = x;Y = y;
    }
    public int X { get; set; }
    public int Y { get; set; }
}

public class Block :MonoBehaviour{

    // ブロックのインデックス
    private BlockPosition blockPosition;
    // ブロックの種類
    private BlockType type;
    // ブロックの状態
    private BlockStatus status;
    // ブロックの座標
    private RectTransform pos;
    // GameObject
    private GameObject obj;

    // 指定した座標にブロックを移動させる
    public IEnumerator MoveBlock(Vector2 dest, float duration)
    {
        Vector2 startPos = this.BlockTransform.anchoredPosition;
        // 移動開始時間
        float startTime = Time.time;
        // 移動してからの経過時間
        float diff = Time.time - startTime;
        // 移動量
        float rate;
        while (this.BlockTransform.anchoredPosition != dest)
        {
            diff = Time.time - startTime;
            // 経過時間が指定した時間を超過したら移動終了
            if (diff >= duration)
            {
                this.BlockTransform.anchoredPosition = dest;
                break;
            }
            rate = diff / duration;
            this.BlockTransform.anchoredPosition = Vector2.Lerp(startPos, dest, rate);
            yield return new WaitForEndOfFrame();
        }
        // ブロックを入れ替えた時点で一旦ブロックの状態を戻す
        this.BlockStatus = BlockStatus.NORMAL;
        yield return 0;
    }

    public IEnumerator DeleteBlock(float duration)
    {
        Vector3 initScale = this.BlockTransform.localScale;
        // 開始時間
        float startTime = Time.time;
        // 経過時間
        float diff = Time.time - startTime;
        // 変化率
        float rate;
        while (this.BlockTransform.localScale != Vector3.zero) {
            diff = Time.time - startTime;
            // 経過時間が指定した時間を超過したら移動終了
            if (diff >= duration)
            {
                this.BlockTransform.localScale = Vector3.zero;
                break;
            }
            rate = diff / duration;
            this.BlockTransform.localScale = Vector3.Lerp(initScale,Vector3.zero, rate);
            yield return new WaitForEndOfFrame();
        }
        this.BlockStatus = BlockStatus.DELETED;
        yield return 0;
    }

    // プロパティ群
    #region
    public BlockPosition BlockPosition
    {
        get { return this.blockPosition; }
        set { this.blockPosition = value; }
    }
    public RectTransform BlockTransform
    {
        get { return this.pos; }
        set { this.pos = value; }
    }
    public BlockType BlockType
    {
        get { return this.type; }
        set { this.type = value; }
    }

    public BlockStatus BlockStatus
    {
        get { return this.status; }
        set { this.status = value; }
    }

    public GameObject BlockObject
    {
        get { return this.obj; }
        set { this.obj = value; }
    }
    #endregion
}
