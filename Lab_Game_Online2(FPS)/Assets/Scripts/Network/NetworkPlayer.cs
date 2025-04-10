using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public TextMeshProUGUI playerNickNameTM;

    public static NetworkPlayer Local { get; set; }
    public Transform playerModel;

    [Networked]
    public NetworkString<_16> nickName { get; set; }


    // remote client token hash 
    [Networked] public int token { get; set; }

    ChangeDetector changeDetector;

    bool isPublicJoinMessageSent = false;

    public LocalCameraHandler localCameraHandler;
    public GameObject localUI;

    //AI
    public bool isBot = false;

    //camera mode
    public bool is3rdPersonCamera { get; set; }

    [Networked]
    public int score { get; set; }

    public TextMeshProUGUI scoreText; 

    //component
    NetworkInGameMessages networkInGameMessages;

    void Awake()
    {
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(nickName):
                    OnNickNameChanged();
                    break;
            }
        }
       
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        bool isReadyScene = SceneManager.GetActiveScene().name == "Ready";

        if (Object.HasInputAuthority)
        {
            Local = this;

            if (isReadyScene)
            {
                Camera.main.transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);

                localCameraHandler.gameObject.SetActive(false);

                localUI.SetActive(false);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                //thiết lập lớp của mô hình người chơi
                Utils.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

                //tat camera chinh
                if (Camera.main != null)
                    Camera.main.gameObject.SetActive(false);


                //khoi tao camera
                localCameraHandler.localCamera.enabled = true;
                localCameraHandler.gameObject.SetActive(true);

                //
                localCameraHandler.transform.parent = null;

                localUI.SetActive(true);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
           

            RPC_SetNickName(GameManager.instance.playerNickName);

            playerNickNameTM.gameObject.SetActive(true);

            Debug.Log("Spawned local player");
        }
        else
        {
            if(Object.HasStateAuthority && isBot)
                nickName = $"Bot{Random.Range(0,100)}";

            localCameraHandler.localCamera.enabled = false;
            localCameraHandler.gameObject.SetActive(false);

            localUI.SetActive(false);

            Debug.Log("Spawned remote player");
        }

        Runner.SetPlayerObject(Object.InputAuthority, Object);
        
        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {
        if(Object.HasStateAuthority)
        {
           if(Runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject))
           { 
                if(playerLeftNetworkObject == Object)
                    Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(playerLeftNetworkObject.GetComponent<NetworkPlayer>().nickName.ToString(), "da thoat game");    
           }
        }
        

        if (player == Object.InputAuthority)
            Runner.Despawn(Object);

    }

    
    private void OnNickNameChanged()
    {
        //Debug.Log($"Nick name changed for player to {nickName} for player {gameObject.name}");

        playerNickNameTM.text = nickName.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this.nickName = nickName;

        if(!isPublicJoinMessageSent)
        {
            networkInGameMessages.SendInGameRPCMessage(nickName, "da vao game");

            isPublicJoinMessageSent = true;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCameraMode(bool is3rdPersonCamera, RpcInfo info = default)
    {
        this.is3rdPersonCamera = is3rdPersonCamera;
    }

    void OnDestroy()
    {
        if(localCameraHandler != null)
            Destroy(localCameraHandler.gameObject);

        SceneManager.sceneLoaded -= OnSceneLoaded;

    }


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"{Time.time} OnSceneLoaded: " + scene.name);

        if (scene.name != "Ready")
        {

            if (Object.HasStateAuthority && Object.HasInputAuthority)
                Spawned();

            if (Object.HasStateAuthority)
                GetComponent<CharacterMovementHandler>().RequestRespawn();
        }
    }

    public void UpdateScoreUI()
    {
        if (Object.HasInputAuthority && scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
}
