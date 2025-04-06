using UnityEngine;
using TMPro;

public class AmmoRefillStation : MonoBehaviour
{
    public GameObject ammoHintUI;
    private WeaponHandler weaponHandler;

    void Start()
    {
        if (ammoHintUI != null)
            ammoHintUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            weaponHandler = other.GetComponent<WeaponHandler>();
            if (ammoHintUI != null)
                ammoHintUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            weaponHandler = null;

            if (ammoHintUI != null)
            {
                ammoHintUI.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (weaponHandler != null && Input.GetKeyDown(KeyCode.E))
        {
            weaponHandler.RefillRocketAmmo();
        }
    }
}
