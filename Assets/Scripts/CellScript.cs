using UnityEngine;

public class CellScript : MonoBehaviour
{
    public int gridX;
    public int gridZ;
    public bool isOccupied = false;

    //void Update()
    //{
    //    // Sürükleme sırasında kontrol etme, sadece snap sonrası
    //    if (GameManager.Instance.activeItem != null)
    //        return;

    //    Vector3 origin = transform.position + Vector3.down * 1f;
    //    RaycastHit hit;

    //    if (Physics.Raycast(origin, Vector3.up, out hit, 3f))
    //    {
    //        ItemScript item = hit.collider.GetComponent<ItemScript>();
    //        if (item != null)
    //        {
    //            isOccupied = true;
    //            item.currentCell = this;
    //            return;
    //        }
    //    }

    //    isOccupied = false;
    //}

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Item") && GameManager.Instance.droped)
        {
            isOccupied = true;
         //  ItemScript item = other.GetComponent<ItemScript>();
         //  if (item != null)
         //  {
         //      item.currentCell = this;
         //  }
        }
    }
}
