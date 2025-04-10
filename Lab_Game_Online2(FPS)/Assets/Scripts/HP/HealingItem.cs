/*using UnityEngine;

public class HealingItem : MonoBehaviour
{
    public byte healAmount = 2; 

    private void OnTriggerEnter(Collider other)
    {
        HPHandler hpHandler = other.GetComponent<HPHandler>();
        if (hpHandler != null && !hpHandler.isDead)
        {
            hpHandler.HP = (byte)Mathf.Min(hpHandler.HP + healAmount, 5); 
            Destroy(gameObject); 
        }
    }
}*/

using UnityEngine;

public class HealingItem : MonoBehaviour
{
    public byte healAmount = 2;
    private Spawner spawner;

    public void Initialize(Spawner spawner)
    {
        this.spawner = spawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        HPHandler hpHandler = other.GetComponent<HPHandler>();
        if (hpHandler != null && !hpHandler.isDead)
        {
            hpHandler.HP = (byte)Mathf.Min(hpHandler.HP + healAmount, 5);
            spawner.RemoveHealingItem(gameObject); // Notify the spawner
            Destroy(gameObject);
        }
    }
}
