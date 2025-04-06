using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class LocalCameraHandler : MonoBehaviour
{
    public Transform cameraAnchorPoint;
    public Camera localCamera;
    public GameObject localGun;

    //input
    Vector2 viewInput;

    //camera rotation
    float cameraRotationX = 0;
    float cameraRotationY = 0;

    //component
    NetworkCharacterController networkCharacterController;
    CinemachineVirtualCamera cinemachineVirtualCamera;

    private void Awake()
    {
        localCamera = GetComponent<Camera>();
        networkCharacterController = GetComponentInParent<NetworkCharacterController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        cameraRotationX = GameManager.instance.cameraViewRotation.x;
        cameraRotationY = GameManager.instance.cameraViewRotation.y;
    }

    void LateUpdate()
    {
        if (cameraAnchorPoint == null)
            return; 

        if(!localCamera.enabled)
            return;

        //tim cinemachine camera neu chua co
        if (cinemachineVirtualCamera == null)
            cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        else
        {
            if (NetworkPlayer.Local.is3rdPersonCamera)
            {
                if (!cinemachineVirtualCamera.enabled)
                {
                    cinemachineVirtualCamera.Follow = NetworkPlayer.Local.playerModel;
                    cinemachineVirtualCamera.LookAt = NetworkPlayer.Local.playerModel;
                    cinemachineVirtualCamera.enabled = true;

                    Utils.SetRenderLayerInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("Default"));

                    localGun.SetActive(false);
                }

                return;
            }
            else
            {
                if (cinemachineVirtualCamera.enabled)
                { 
                    cinemachineVirtualCamera.enabled = false;

                    Utils.SetRenderLayerInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

                    localGun.SetActive(true);
                }
            }
        }


        //di chuyen cam dden ng choi
        localCamera.transform.position = cameraAnchorPoint.position;

        //tinh phep quay
        cameraRotationX += viewInput.y * Time.deltaTime * 40;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);

        cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterController.rotationSpeed;

        //ap dung quay
        localCamera.transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);

    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }

    private void OnDestroy()
    {
        if(cameraRotationX != 0 && cameraRotationY != 0)
        {
            GameManager.instance.cameraViewRotation.x = cameraRotationX;
            GameManager.instance.cameraViewRotation.y = cameraRotationY;
        }
    }
}
