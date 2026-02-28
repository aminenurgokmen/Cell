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
    public CellScript[,] grid;
    public int gridSizeX = 5;
    public int gridSizeZ = 5;
    public WaveManager waveManager; // Inspector’dan bağlayacağız

    void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
    }
    void Start()
    {
        grid = new CellScript[gridSizeX, gridSizeZ];

        CellScript[] allCells = FindObjectsOfType<CellScript>();
        foreach (CellScript cell in allCells)
        {
            grid[cell.gridX, cell.gridZ] = cell;
        }
    }
    void CheckMatch(CellScript centerCell)
    {
        List<CellScript> horizontalMatch = new List<CellScript>();
        List<CellScript> verticalMatch = new List<CellScript>();

        int id = centerCell.cellID;

        // Kendini ekle
        horizontalMatch.Add(centerCell);
        verticalMatch.Add(centerCell);

        // ---- HORIZONTAL ----
        CheckDirection(centerCell, 1, 0, id, horizontalMatch);  // sağ
        CheckDirection(centerCell, -1, 0, id, horizontalMatch); // sol

        // ---- VERTICAL ----
        CheckDirection(centerCell, 0, 1, id, verticalMatch);  // yukarı
        CheckDirection(centerCell, 0, -1, id, verticalMatch); // aşağı

        if (horizontalMatch.Count >= 3)
            DestroyCells(horizontalMatch);

        if (verticalMatch.Count >= 3)
            DestroyCells(verticalMatch);
    }
    void CheckDirection(CellScript startCell, int dirX, int dirZ, int id, List<CellScript> matchList)
    {
        int x = startCell.gridX + dirX;
        int z = startCell.gridZ + dirZ;

        while (x >= 0 && x < gridSizeX && z >= 0 && z < gridSizeZ)
        {
            CellScript cell = grid[x, z];

            if (cell != null && cell.isOccupied && cell.cellID == id)
            {
                matchList.Add(cell);
                x += dirX;
                z += dirZ;
            }
            else
            {
                break;
            }
        }
    }
    void DestroyCells(List<CellScript> cells)
    {
        foreach (CellScript cell in cells)
        {
            if (cell == null) continue;

            if (cell.transform.childCount > 0)
            {
                Destroy(cell.transform.GetChild(0).gameObject);
            }

            cell.isOccupied = false;
            cell.cellID = 0;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
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
                activeItem.GetComponent<Collider>().enabled = false; // Çarpışmayı kapat
                CheckMatch(cell);
                ItemScript itemScript = activeItem.GetComponent<ItemScript>();
                if (itemScript == null)
                    itemScript = activeItem.GetComponentInChildren<ItemScript>();
                if (itemScript == null)
                    itemScript = activeItem.GetComponentInParent<ItemScript>();

                if (itemScript != null)
                    cell.cellID = itemScript.itemID;

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