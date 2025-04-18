using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public Vector3 aimForwardVector;
    public Vector3 cameraPosition;
    public NetworkBool isJumpPressed;
    public NetworkBool isFireButtonPressed;
    public NetworkBool isGrenadeFireButtonPressed;
    public NetworkBool isRocketLauncherButtonPressed;
}
