using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour {


    [SerializeField]
    // チェイン継続時間
    private float ChainTimer = 0.0f;
    [SerializeField]
    // 落下終了後からチェイン終了までの猶予時間
    private float ChainDelay = 0.1f;

    // Update is called once per frame
    void Update() {
        // チェイン継続時間を減らしていく
        if (ChainTimer > 0.0f) ChainTimer -= Time.deltaTime;
        // チェイン継続時間が無くなったらターン終了
        else { ChainTimer = 0.0f; }
    }

    // チェインタイマーに値をセットする
    public void SetChainTimer(float duration) {
        // duration(DeleteTime + FallTime)の値にチェイン終了までの猶予時間の値を加算
        duration += this.ChainDelay;
        if (duration > this.ChainTimer) this.ChainTimer = duration;
    }

    public float ChainTimerProp {
        get { return this.ChainTimer; }
        set { this.ChainTimer = value; }
    }
}
