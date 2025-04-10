using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class HPHandler : NetworkBehaviour
{
    [Networked]
    public byte HP { get; set; }

    [Networked]
    public bool isDead { get; set; }

    bool isInitialized = false;

    const byte startingHP = 5;

    public Color uiOnHitColor;
    public Image uiOnHitImage;

    public Animator characterAnimator;
    private int _animIDHit;

    /*public MeshRenderer bodyMeshRenderer;
    Color defaulMeshBodyColor;*/

    public GameObject playerModel;
    public GameObject deathGameObjectPrefabs;

    public bool SkipSettingStartValues = false;

    ChangeDetector changeDetector;

    public event System.Action<byte> OnHealthChanged;


    //components
    HitboxRoot hitboxRoot;
    CharacterMovementHandler characterMovementHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;

    private void Awake()
    {
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
        hitboxRoot = GetComponentInChildren<HitboxRoot>();
        AssignAnimationIDs();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!SkipSettingStartValues)
        {
            HP = startingHP;
            isDead = false;
        }

        //defaulMeshBodyColor = bodyMeshRenderer.material.color;

        isInitialized = true;
    }

    public override void Render()
    {
        foreach(var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                 case nameof(HP):
                    var byteReader = GetPropertyReader<byte>(nameof(HP));
                    var (previousByte, cunrrentByte) = byteReader.Read(previousBuffer, currentBuffer);
                    OnHPChanged(previousByte, cunrrentByte);
                    break;

                 case nameof(isDead):
                    var boolReader = GetPropertyReader<bool>(nameof(isDead));
                    var (previousBool, cunrrentBool) = boolReader.Read(previousBuffer, currentBuffer);
                    OnStateChanged(previousBool, cunrrentBool);
                    break;
            }

        }
    }

    private void AssignAnimationIDs()
    {
        _animIDHit = Animator.StringToHash("Hit");
    }
    IEnumerator OnHitCO()
    {
        //bodyMeshRenderer.material.color = Color.white;

        if (Object.HasInputAuthority)
            uiOnHitImage.color = uiOnHitColor;

        yield return new WaitForSeconds(0.2f);

        //bodyMeshRenderer.material.color = defaulMeshBodyColor;

        if (Object.HasInputAuthority && !isDead)
            uiOnHitImage.color = new Color(0, 0, 0, 0);
    }

    IEnumerator ServerReviveCO()
    {
        yield return new WaitForSeconds(2.0f);

        characterMovementHandler.RequestRespawn();
    }

    public void OnTakeDamage(string damageCausedByPlayerNickName, byte damageAmount)
    {
        if(isDead)
           return;

        if(damageAmount > HP)
            damageAmount = HP;

        HP -= damageAmount;

        //Debug.Log($"{Time.time} {transform.name} took damage got {HP} left ");

        if (HP <= 0)
        {
            networkInGameMessages.SendInGameRPCMessage(damageCausedByPlayerNickName, $"đã giết <b>{networkPlayer.nickName.ToString()}</b>");

            StartCoroutine(ServerReviveCO());

            isDead = true;

            foreach (var player in FindObjectsOfType<NetworkPlayer>())
            {
                if (player.nickName.ToString() == damageCausedByPlayerNickName)
                {
                    if (networkPlayer.isBot) 
                    {
                        player.score += 10; 
                        player.UpdateScoreUI(); 
                    }

                    break;
                }
            }
        }
    }
    

    void OnHPChanged(byte previous, byte current)
    {
        if (current < previous)
        {
            OnHPReduced();
        }

        OnHealthChanged?.Invoke(current);
    }
    
    private void OnHPReduced()
    {
        if (!isInitialized)
            return;

        characterAnimator.SetTrigger(_animIDHit);

        StartCoroutine(OnHitCO());
    }
    void OnStateChanged(bool previous, bool current)
    {
        if (current)
            OnDeath();
        else if(!current && previous)
            OnRevive();
    }
    private void OnDeath()
    {
        playerModel.gameObject.SetActive(false);
        hitboxRoot.HitboxRootActive = false;
        characterMovementHandler.SetCharacterControllerEnabled(false);

        Instantiate(deathGameObjectPrefabs, transform.position, Quaternion.identity);
    }

    private void OnRevive()
    {
        if (Object.HasInputAuthority)
            uiOnHitImage.color = new Color(0, 0, 0, 0);

        playerModel.gameObject.SetActive(true);
        hitboxRoot.HitboxRootActive = true;
        characterMovementHandler.SetCharacterControllerEnabled(true);
    }

    public void OnRespawned()
    {
        HP = startingHP;
        isDead = false; 
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
}
