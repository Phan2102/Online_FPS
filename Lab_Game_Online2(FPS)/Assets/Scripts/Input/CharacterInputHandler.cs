using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    bool isFireButtonPressed = false;
    bool isGrenadeFireButtonPressed = false;
    bool isRocketLauncherButtonPressed = false;

    LocalCameraHandler localCameraHandler;
    CharacterMovementHandler characterMovementHandler;
    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!characterMovementHandler.Object.HasInputAuthority)
            return;

        if (SceneManager.GetActiveScene().name == "Ready")
            return;

        //view input
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1;

        //move input 
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        //jump
        if (Input.GetButtonDown("Jump"))
            isJumpButtonPressed = true;

        //fire
        if (Input.GetButtonDown("Fire1"))
            isFireButtonPressed = true;

        //fire
        if (Input.GetButtonDown("Fire2"))
            isRocketLauncherButtonPressed = true;

        //throw grenade
        if (Input.GetKeyDown(KeyCode.G))
            isGrenadeFireButtonPressed = true;

        //nut che do camera
        if(Input.GetKeyDown(KeyCode.C))
        {
            NetworkPlayer.Local.is3rdPersonCamera = !NetworkPlayer.Local.is3rdPersonCamera;

            NetworkPlayer.Local.RPC_SetCameraMode(NetworkPlayer.Local.is3rdPersonCamera);
        }

        //set view
        localCameraHandler.SetViewInputVector(viewInputVector);

    }
    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //aim data
        networkInputData.aimForwardVector = localCameraHandler.transform.forward;

        //cam pos
        networkInputData.cameraPosition = localCameraHandler.transform.position;

        //move data
        networkInputData.movementInput = moveInputVector;

        //jump data
        networkInputData.isJumpPressed = isJumpButtonPressed;

        //fire data
        networkInputData.isFireButtonPressed = isFireButtonPressed;

        //rocket data
        networkInputData.isRocketLauncherButtonPressed = isRocketLauncherButtonPressed;

        //grenade fire data
        networkInputData.isGrenadeFireButtonPressed = isGrenadeFireButtonPressed;


        isJumpButtonPressed = false;
        isFireButtonPressed = false;
        isGrenadeFireButtonPressed = false;
        isRocketLauncherButtonPressed = false;

        return networkInputData;
    }
}
