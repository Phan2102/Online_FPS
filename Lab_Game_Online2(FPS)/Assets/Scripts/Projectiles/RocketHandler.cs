using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RocketHandler : NetworkBehaviour
{
    [Header("Prefabs")]
    public GameObject explosionParticleSystemPrefab;

    [Header("Collision detection")]
    public Transform checkForImpactPoint;
    public LayerMask collisionLayers;

    TickTimer maxLiveDurationTickTimer = TickTimer.None;

    int rocketSpeed = 15;
    
    List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

    PlayerRef firedByPlayerRef;
    string firedByPlayerName;
    NetworkObject fireByNetworkObject;

    NetworkObject networkObject;

    public void Fire(PlayerRef firedByPlayerRef, NetworkObject fireByNetworkObject, string firedByPlayerName)
    {
        this.firedByPlayerRef = firedByPlayerRef;
        this.firedByPlayerName = firedByPlayerName;
        this.fireByNetworkObject = fireByNetworkObject;

        networkObject = GetComponent<NetworkObject>();
       

        maxLiveDurationTickTimer = TickTimer.CreateFromSeconds(Runner, 2.5f);
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += transform.forward * Runner.DeltaTime * rocketSpeed;
        if (Object.HasStateAuthority)
        {
            if (maxLiveDurationTickTimer.Expired(Runner))
            {
                Runner.Despawn(networkObject);
                return;
            }

            //check ten lua co dam vao vat the nao khong
            int hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, 0.5f, firedByPlayerRef, hits, collisionLayers, HitOptions.IncludePhysX);
            
            bool isValidHit = false;

            //da hit vao 1 vat the nao do, vi vay hit co the hop le
            if (hitCount > 0)
                isValidHit = true;

            for (int i = 0; i < hitCount; i++)
            {
                //check xem co va vao hitbox khong
                if(hits[i].Hitbox != null)
                {
                    //check co ban ten lua va tu ban trung minh` ko. deu nay co the xay ra neu do lag hoi cao
                    if (hits[i].Hitbox.Root.GetComponent<NetworkObject>() == fireByNetworkObject)
                        isValidHit = false;
                }
            }

            if (isValidHit)
            {
                hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, 4, firedByPlayerRef, hits, collisionLayers, HitOptions.None);

                for(int i = 0;i < hitCount; i++)
                {
                    HPHandler hPHandler = hits[i].Hitbox.transform.root.GetComponent<HPHandler>();
                    if (hPHandler != null)
                    {
                        hPHandler.OnTakeDamage(firedByPlayerName, 100);
                    }
                }

                Runner.Despawn(networkObject);
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Instantiate(explosionParticleSystemPrefab, checkForImpactPoint.position, Quaternion.identity);
    }
}
