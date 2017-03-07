using UnityEngine;
using System.Collections;

enum FlickDirection: byte
{
    TAP,
    UP = 0,
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
public class MainGameSystem : MonoBehaviour {

    // フリックの状態
    private FlickState flickState;
    // フリックの始点、終点の座標
    private Vector3 beginPoint,endPoint;

    private Block[,] stage;

	// Use this for initialization
	void Start () {
        stage = new Block[0, 0];
        stage[0, 0] = new Block(0, 0, BlockType.BLUE, BlockStatus.DISABLED);
        Spawner sp = new Spawner();
    }
	
	// Update is called once per frame
	void Update () {
        Flick();
	}

    #region
    private void Flick()
    {
        if (Input.GetMouseButtonDown(0) && flickState == FlickState.FREE)
        {
            flickState = FlickState.FLICKING;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 10.0f);
            print("hit!" + hit.collider);
            if (hit.collider.tag == "Block")
            {
                beginPoint = Input.mousePosition;
            }
        }
        if (Input.GetMouseButtonUp(0) && flickState == FlickState.FLICKING)
        {
            endPoint = Input.mousePosition;
            GetDirection(beginPoint, endPoint);

            flickState = FlickState.FREE;
        }
    }

    private FlickDirection GetDirection(Vector3 begin, Vector3 end)
    {
        float dirX, dirY;
        dirX = end.x - begin.x;
        dirY = end.y - begin.x;
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
            return FlickDirection.DOWN;
        }
    }
    #endregion
}
