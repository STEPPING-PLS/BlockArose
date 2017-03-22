using UnityEngine;
using System.Collections;

public class GameSettingManager : SingletonMonoBehaviour<GameSettingManager> {

    private float seVolume;

    private float bgmVolume;

    public float SEVolume
    {
        get
        {
            return this.seVolume;
        }
        set
        {
            this.seVolume = value;
        }
    }

    public float BGMVolume
    {
        get
        {
            return this.bgmVolume;
        }
        set
        {
            this.bgmVolume = value;
        }
    }

    public void Test()
    {
        Instance.SEVolume = 0.9f;
    }
}
