using UnityEngine;
using UnityEngine.UI;

public class RankingDisplay : MonoBehaviour {

    [SerializeField]
    private int rank;

    private void DisplayTopScores()
    {
        GetComponent<Text>().text
            = RankingScoreManager.Instance.TopScores[this.rank].ToString();
    }

	// Use this for initialization
	void Start () {
        DisplayTopScores();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
