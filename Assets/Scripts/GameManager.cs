using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<Material> materials;
    public GameObject activeItem;   // Şu an sürüklenen obje
    private Camera mainCam;
    private float dragHeight = 0f;  // Objeyi hangi Y seviyesinde taşıyacağız
private int placedObjectCount = 0;
public int maxPlacedCount = 3;

public WaveManager waveManager; // Inspector’dan bağlayacağız

    void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
           Debug.Break();
        }
        // Mouse basınca obje seç
        if (Input.GetMouseButtonDown(0))
        {
            SelectObject();
        }

        // Mouse basılıyken sürükle
        if (Input.GetMouseButton(0) && activeItem != null)
        {
            DragObject();
        }

        // Mouse bırakınca bırak
        if (Input.GetMouseButtonUp(0))
        {
            ReleaseObject();
        }
    }

    void SelectObject()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int layerMask = 1 << 6; // sadece 6. layer

        if (Physics.Raycast(ray, out hit, 100f, layerMask))
        {
            activeItem = hit.collider.gameObject;
            dragHeight = activeItem.transform.position.y;
        }
    }

    void DragObject()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, dragHeight, 0));

        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            activeItem.transform.position = worldPos;
        }
    }

  void ReleaseObject()
{
    if (activeItem == null)
        return;

    Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    int layerMask = ~(1 << 6); // Item layer'ını hariç tut

    if (Physics.Raycast(ray, out hit, 100f, layerMask, QueryTriggerInteraction.Collide))
    {
        CellScript cell = hit.collider.GetComponent<CellScript>();

        if (cell != null && !cell.isOccupied)
        {
            activeItem.transform.position = hit.collider.transform.position;
            cell.isOccupied = true;

            placedObjectCount++;

            if (placedObjectCount >= maxPlacedCount)
            {
                placedObjectCount = 0;
                waveManager.SpawnThreeObjects();
            }
        }
    }

    activeItem = null;
}
}