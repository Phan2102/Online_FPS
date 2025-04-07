using Fusion;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CharacterOutfitHandler : NetworkBehaviour
{
    [Header("character parts")]
    public GameObject playerHead;

    [Header("Ready UI")]
    public Image readyCheckboxImage;

    [Header("Animator")]
    public Animator characterAnimator;

    //list of body
    List<GameObject> headPrefabs = new List<GameObject>();

    //components
    NetworkPlayer networkPlayer;
    struct NetworkOutfit : INetworkStruct
    {
        public byte headPrefabID;
    }

    [Networked]
    NetworkOutfit networkOutfit { get; set; }

    [Networked]
    public NetworkBool isDoneWithCharacterSelection { get; set; }

    ChangeDetector changeDetector;

    private void Awake()
    {
        headPrefabs = Resources.LoadAll<GameObject>("Bodyparts/heads/").ToList();
        headPrefabs = headPrefabs.OrderBy(n => n.name).ToList();

        networkPlayer = GetComponent<NetworkPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        characterAnimator.SetLayerWeight(1, 0.0f);

        if (SceneManager.GetActiveScene().name != "Ready" && !networkPlayer.isBot)
            return;

        NetworkOutfit newOutfit = networkOutfit;

        //random outfit
        newOutfit.headPrefabID = (byte)Random.Range(0, headPrefabs.Count);

        //cho phep layer animation hien thi
        characterAnimator.SetLayerWeight(1, 1.0f);

        //
        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);

        //bot
        if (networkPlayer.isBot && Object.HasStateAuthority)
        {
            networkOutfit = newOutfit;
            ReplaceBodyParts();
        }

    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(networkOutfit):
                    OnOutfitChanged();
                    break;
                case nameof(isDoneWithCharacterSelection):
                    OnIsDoneWithCharacterSelectionChanged();
                    break;
            }
        }
    }

    GameObject ReplaceBodyPart(GameObject currentBodyPart, GameObject prefabNewBodyPart)
    {
        GameObject newPart = Instantiate(prefabNewBodyPart, currentBodyPart.transform.position, currentBodyPart.transform.rotation);
        newPart.transform.parent = currentBodyPart.transform.parent;
        Utils.SetRenderLayerInChildren(newPart.transform, currentBodyPart.layer);
        Destroy(currentBodyPart);

        return newPart;
    }

    void ReplaceBodyParts()
    {
        //replace head
        playerHead = ReplaceBodyPart(playerHead, headPrefabs[networkOutfit.headPrefabID]);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestOutfitChange(NetworkOutfit newNetworkOutfit, RpcInfo info = default)
    {
        Debug.Log($"Recevived RPC_RequestOutfitChange for player {transform.name}. HeadID{newNetworkOutfit.headPrefabID}");

        networkOutfit = newNetworkOutfit;
    }

   
    private void OnOutfitChanged()
    {
        ReplaceBodyParts();
    }

    public void OnCycleHead()
    {
        NetworkOutfit newOutfit = networkOutfit;

        newOutfit.headPrefabID++;

        if(newOutfit.headPrefabID > headPrefabs.Count - 1)
            newOutfit.headPrefabID = 0;

        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    public void OnReady(bool isReady)
    {
        if (Object.HasInputAuthority)
        {
            RPC_SetReady(isReady);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReady(NetworkBool isReady, RpcInfo info = default)
    {
        isDoneWithCharacterSelection = isReady;
    }

   
    private void OnIsDoneWithCharacterSelectionChanged()
    {
        if(isDoneWithCharacterSelection)
        {
            characterAnimator.SetTrigger("Ready");
            readyCheckboxImage.gameObject.SetActive(true);
        }
        else readyCheckboxImage.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Ready")
            readyCheckboxImage.gameObject.SetActive(false);
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

}