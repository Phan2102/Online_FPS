using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

public class GrenadeHandler : NetworkBehaviour
{
    public GameObject explosionParticleSystemPrefab;

    public LayerMask collisionLayers;

    //throw by info
    PlayerRef throwByPlayerRef;
    string throwByPlayerName;

    //timing
    TickTimer explodeTickTimer = TickTimer.None;

    //hits info
    List<LagCompensatedHit> hits = new List<LagCompensatedHit>();


    //cpmponents
    NetworkObject networkObject;
    NetworkRigidbody3D networkRigidbody;
    public void Throw(Vector3 throwForce, PlayerRef throwByPlayerRef, string throwByPlayerName)
    {
        networkObject = GetComponent<NetworkObject>();
        networkRigidbody = GetComponent<NetworkRigidbody3D>();

        networkRigidbody.Rigidbody.AddForce(throwForce, ForceMode.Impulse);

        this.throwByPlayerRef = throwByPlayerRef;
        this.throwByPlayerName = throwByPlayerName;

        explodeTickTimer = TickTimer.CreateFromSeconds(Runner, 2);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (explodeTickTimer.Expired(Runner))
            {
                int hitCount = Runner.LagCompensation.OverlapSphere(transform.position, 4, throwByPlayerRef, hits, collisionLayers);

                for(int i = 0; i < hitCount; i++)
                {
                    HPHandler hPHandler = hits[i].Hitbox.transform.root.GetComponent<HPHandler>();

                    if(hPHandler != null)
                    {
                        hPHandler.OnTakeDamage(throwByPlayerName, 100);
                    }
                }


                Runner.Despawn(networkObject);

                //stop the explode timer for being triggered again
                explodeTickTimer = TickTimer.None;
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        MeshRenderer granademesh = GetComponent<MeshRenderer>();

        Instantiate(explosionParticleSystemPrefab, granademesh.transform.position, Quaternion.identity);
    }

}
