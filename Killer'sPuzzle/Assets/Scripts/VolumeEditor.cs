using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VolumeEditor : MonoBehaviour {

    private Slider slider;
    // このIDでBGM(0),SE(1)のどちらを編集するつもりかを判断する
    [SerializeField]
    private int id;

    private void Start()
    {
        this.slider = GetComponent<Slider>();
    }

    public void ChangeVolume ( )
    {
        switch (id)
        {
            case 0:
                GameSettingManager.Instance.BGMVolume = this.slider.value;
                break;
            case 1:
                GameSettingManager.Instance.SEVolume = this.slider.value;
                break;
            default:
                print("ERROR");
                break;
        }
    }
    
}
