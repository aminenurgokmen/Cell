using UnityEngine;

public class CellScript : MonoBehaviour
{
    public int gridX;
    public int gridZ;
    public bool isOccupied = false;
    public int cellID = -1;
    public bool droped;
    public GameObject currentItem; // Hücredeki item referansı

    // Item tespiti artık GameManager.ReleaseObject() üzerinden yapılıyor.
    // CellScript sadece veri tutucu olarak çalışıyor.


}
