using UnityEngine;

public enum FlickDirection : byte
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
public class PlayerController : MonoBehaviour {

    // フリックの状態
    private FlickState flickState;

    // 現在選択中のブロック,移動先のブロック
    private Block selectedBlock, destBlock;

    // フリックの始点、終点の座標
    private Vector3 beginPoint, endPoint;

    // MainGameSystemへの参照を取得
    [SerializeField]
    private MainGameSystem mainGameSystem;
    [SerializeField,Range(20.0f,135.0f)]
    // フリック判定となる距離
    private float FlickLength;

    // Use this for initialization
    void Start () {
        if(mainGameSystem == null)
            this.mainGameSystem = GameObject.Find("GameController").GetComponent<MainGameSystem>();
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
                this.mainGameSystem.OnFlicked(GetDirection(beginPoint, endPoint),ref selectedBlock,ref destBlock);
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
        if (Mathf.Abs(dirX) < FlickLength && Mathf.Abs(dirY) < FlickLength)
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

}
