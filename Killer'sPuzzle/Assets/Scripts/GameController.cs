using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameController : MonoBehaviour {

    [SerializeField]
    private GameObject OptionBoard;

	public void ShiftState(int id)
    {

        // 状態変更によるシーン切り替えなど
        switch (id)
        {
            // GAME STATE -> TITLE
            case 0:
                SceneManager.LoadSceneAsync(0);
                break;
            // GAME STATE -> OPTION
            case 1:
                Instantiate(this.OptionBoard);
                break;
            // GAME STATE -> RANKING
            case 2:
                SceneManager.LoadSceneAsync(1);
                break;
            // GAME STATE -> MAIN
            case 3:
                SceneManager.LoadSceneAsync(2);
                break;
            // GAME STATE -> RESULT
            case 4:
                SceneManager.LoadSceneAsync(3);
                break;
            default:
                break;
        }

        // 状態変更
        GameManager.Instance.State = (GameState)id;
    }

}
