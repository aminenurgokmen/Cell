using UnityEngine;

public class CellScript : MonoBehaviour
{
    public int gridX;
    public int gridZ;
    public bool isOccupied = false;
    public int cellID;
    public bool droped;

    void Update()
    {
        // Sürükleme sırasında kontrol etme, sadece snap sonrası
        if (GameManager.Instance.activeItem != null)
            return;

        Vector3 origin = transform.position + Vector3.down * 1f;
        RaycastHit hit;

        if (Physics.Raycast(origin, Vector3.up, out hit, 3f, 1 << 7))
        {
            Debug.DrawLine(origin, hit.point, Color.red);
            Debug.Log("Raycast hit: " + hit.collider.name);
            ItemScript item = hit.collider.GetComponent<ItemScript>();
            if (item != null)
            {
                isOccupied = true;
                cellID = item.itemID;
                item.currentCell = this;
                return;
            }
        }

    }


}
