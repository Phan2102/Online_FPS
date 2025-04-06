using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance = null;

    byte[] connectionToken;

    public Vector2 cameraViewRotation = Vector2.zero;
    public string playerNickName = "";

    private void Awake()
    {
        if (instance == null)
            instance = this;
        
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        //kiem tra token co hop le ko, neu ko thi lay token moi
        if (connectionToken == null)
        {
            connectionToken = ConnectionTokeUtils.NewToken();
            Debug.Log($"player connection token {ConnectionTokeUtils.HashToken(connectionToken)}");
        }

        
    }

    public void SetConnectionToken(byte[] connectionToken)
    {
        this.connectionToken = connectionToken;
    }

    public byte[] GetConnectionToken()
    {
        return connectionToken;
    }
}
