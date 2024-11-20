using System;
using UnityEngine;

public class WallController: MonoBehaviour
{
    public int direction;
    public GameObject fish;
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("fish"))
        {
            if (other.GetComponent<EEWordFish>().Direction == direction)
            {
                other.GetComponent<EEWordFish>().ResetFishPosition();
            }
        }
        else if(other.CompareTag("Tail"))
        {
            other.gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<EEWordFish>()
                .EnableCollider = true;
        }
    }
}
