using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    void CheckMatchAll()
    {
        // Tüm grid'i tara, eşleşen hücreleri topla
        HashSet<CellScript> cellsToDestroy = new HashSet<CellScript>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                CellScript cell = grid[x, z];
                if (cell == null || !cell.isOccupied || cell.cellID < 0) continue;

                // Her hücreden 6 hex yönünde komşulara bak
                // Aynı renkte 2+ yanyana = eşleşme (kendisi dahil 2)
                // 3 ana eksen kontrol et: yön 1-4 (sağ-sol), yön 0-3 (sağüst-solalt), yön 5-2 (solüst-sağalt)
                int[][] axisPairs = new int[][] {
                    new int[] { 1, 4 },  // sağ - sol
                    new int[] { 0, 3 },  // sağ-üst - sol-alt
                    new int[] { 5, 2 }   // sol-üst - sağ-alt
                };

                int id = cell.cellID;

                foreach (int[] axis in axisPairs)
                {
                    List<CellScript> line = new List<CellScript>();
                    line.Add(cell);

                    // İlk yönde ilerle
                    CollectInDirection(cell, axis[0], id, line);
                    // Ters yönde ilerle
                    CollectInDirection(cell, axis[1], id, line);

                    if (line.Count >= 2)
                    {
                        foreach (CellScript c in line)
                            cellsToDestroy.Add(c);
                    }
                }
            }
        }

        if (cellsToDestroy.Count > 0)
        {
            Debug.Log($"[MATCH] {cellsToDestroy.Count} hücre silinecek:");
            foreach (CellScript c in cellsToDestroy)
                Debug.Log($"  Cell ({c.gridX},{c.gridZ}) cellID={c.cellID} currentItem={c.currentItem}");
            DestroyCells(cellsToDestroy);
        }
    }

    void CollectInDirection(CellScript startCell, int direction, int id, List<CellScript> matchList)
    {
        CellScript current = startCell;
        while (true)
        {
            CellScript neighbor = GridManager.Instance.GetNeighbor(current.gridX, current.gridZ, direction);
            if (neighbor != null && neighbor.isOccupied && neighbor.cellID == id)
            {
                matchList.Add(neighbor);
                current = neighbor;
            }
            else
            {
                break;
            }
        }
    }

    void DestroyCells(HashSet<CellScript> cells)
    {
        foreach (CellScript cell in cells)
        {
            if (cell == null) continue;

            if (cell.currentItem != null)
            {
                GameObject item = cell.currentItem;
                Vector3 originalScale = item.transform.localScale;

                // Önce büyü, sonra küçülerek kaybol
                item.transform.DOScale(originalScale * 1.1f, 0.1f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        item.transform.DOScale(Vector3.zero, 0.2f)
                            .SetEase(Ease.InBack)
                            .OnComplete(() =>
                            {
                                Destroy(item);
                            });
                    });

                cell.currentItem = null;
            }

            cell.isOccupied = false;
            cell.cellID = -1;
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
                // Parçayı hücre pozisyonuna taşı
                activeItem.transform.position = hit.collider.transform.position;

                // Tüm child item'ları bul
                ItemScript[] items = activeItem.GetComponentsInChildren<ItemScript>();

                // Her child item'ı en yakın cell'e ayrı ayrı kaydet
                bool placed = false;
                foreach (ItemScript item in items)
                {
                    CellScript nearestCell = GridManager.Instance.GetNearestCell(item.transform.position);
                    if (nearestCell != null && !nearestCell.isOccupied)
                    {
                        // Item'ı parent'tan kopar ve cell pozisyonuna taşı
                        item.transform.SetParent(null);
                        item.transform.position = nearestCell.transform.position;

                        // Cell'e kaydet
                        nearestCell.isOccupied = true;
                        nearestCell.cellID = item.itemID;
                        nearestCell.currentItem = item.gameObject;
                        item.currentCell = nearestCell;

                        // Collider'ı kapat
                        Collider col = item.GetComponent<Collider>();
                        if (col != null) col.enabled = false;

                        placed = true;
                    }
                }

                // Boş kalan parent objeyi sil
                if (activeItem != null && activeItem.GetComponentsInChildren<ItemScript>().Length == 0)
                    Destroy(activeItem);

                if (placed)
                {
                    CheckMatchAll();
                    placedObjectCount++;

                    if (placedObjectCount >= maxPlacedCount)
                    {
                        placedObjectCount = 0;
                        waveManager.SpawnThreeObjects();
                    }
                }
            }
        }

        activeItem = null;
    }
}