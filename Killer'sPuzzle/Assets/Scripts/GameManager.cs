using UnityEngine;

public enum GameState :byte
{
    TITLE = 0,
    OPTION,
    RANKING,
    MAIN,
    RESULT
}

public class GameManager : SingletonMonoBehaviour<GameManager> {

    private GameState state;

    private new void Awake()
    {
        this.state = GameState.TITLE;
    }

    public GameState State
    {
        get
        {
            return this.state;
        }
        set
        {
            this.state = value;
        }
    }
    
}
