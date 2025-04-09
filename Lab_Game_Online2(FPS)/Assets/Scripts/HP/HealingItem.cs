using UnityEngine;

public class HealingItem : MonoBehaviour
{
    public byte healAmount = 1; 

    private void OnTriggerEnter(Collider other)
    {
        HPHandler hpHandler = other.GetComponent<HPHandler>();
        if (hpHandler != null && !hpHandler.isDead)
        {
            hpHandler.HP = (byte)Mathf.Min(hpHandler.HP + healAmount, 5); 
            Destroy(gameObject); 
        }
    }
}