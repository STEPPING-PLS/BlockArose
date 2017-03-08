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
    DESTROY,
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

public class Block {

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

    // コンストラクタでブロックの位置とType,Statusを設定
    public Block(int x,int y,BlockType type,BlockStatus status)
    {
        this.blockPosition.X = x;
        this.blockPosition.Y = y;
        this.BlockType = type;
        this.BlockStatus = status;
    }
    public Block(BlockPosition pos,BlockType type,BlockStatus status)
    {
        this.BlockPosition = pos;
        this.BlockType = type;
        this.BlockStatus = status;
    }
    // ブロックの位置のみを指定した場合はStatusをDISABLEDにする
    public Block(int x, int y)
    {
        this.blockPosition.X = x;
        this.blockPosition.Y = y;
        this.status = BlockStatus.DISABLED;
    }
	// Update is called once per frame
	void Update () {
	
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
