using UnityEngine;
using System.Collections;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T:SingletonMonoBehaviour<T>{

    protected static T instance;

    public static T Instance
    {
        get
        {
            instance = instance ?? (T)FindObjectOfType(typeof(T));
            return instance;
        }
    }

    protected bool CheckInstance()
    {
        if(instance == null)
        {
            //Cast
            instance = (T)this;
            return true;
        }
        else if(Instance == this)
        {
            return true;
        }
        else
        {
            Destroy(this);
            return false;
        }
    }

    // Use this for initialization
    protected void Awake()
    {
        CheckInstance();
    }
}
