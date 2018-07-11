using UnityEngine;

public class NetworkInput : BindableMonoBehavior
{

    public static NetworkInput Instance
    {
        get { return _instance; }
    }
    
    private static NetworkInput _instance;
    
    [BindComponent]
    private GameServer server;

    [HideInInspector]
    public bool[] keyMap = new bool[1024];

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool GetKey(KeyCode code)
    {
        int key = (int) code;

        return Instance.keyMap[key];
    }

    public void PressKey(int key)
    {
        keyMap[key] = true;
    }

    public void ReleaseKey(int key)
    {
        keyMap[key] = false;
    }
}