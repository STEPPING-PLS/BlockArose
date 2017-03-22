using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameController : MonoBehaviour {

    [SerializeField]
    private GameObject OptionBoard;

	public void ShiftState(GameState st)
    {
        // 状態変更
        GameManager.Instance.State = st;

        // 状態変更によるシーン切り替えなど
        switch (st)
        {
            case GameState.TITLE:
                SceneManager.LoadSceneAsync(0);
                break;
            case GameState.OPTION:
                Instantiate(this.OptionBoard);
                break;
            case GameState.RANKING:
                SceneManager.LoadSceneAsync(1);
                break;
            case GameState.MAIN:
                SceneManager.LoadSceneAsync(2);
                break;
            case GameState.RESULT:
                SceneManager.LoadSceneAsync(3);
                break;
            default:
                break;
        }
    }
}
