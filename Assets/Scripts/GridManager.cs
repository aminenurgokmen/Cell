using UnityEngine;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public CellScript cellPrefab;
    public int gridSizeX = 5;
    public int gridSizeZ = 5;
    public float cellRadius = 1f;
    public bool generateGrid = false;

    private CellScript[,] grid;

    public float XSpacing => Mathf.Sqrt(3f) * cellRadius;
    public float ZSpacing => 1.5f * cellRadius;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (generateGrid)
        {
            generateGrid = false;
            ClearGrid();
            MakeGrid();
        }
    }

    void ClearGrid()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    void MakeGrid()
    {
        grid = new CellScript[gridSizeX, gridSizeZ];
        float xSpacing = XSpacing;
        float zSpacing = ZSpacing;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                float xPos = x * xSpacing;
                if (z % 2 == 1)
                    xPos += xSpacing * 0.5f;
                float zPos = z * zSpacing;

                CellScript cell = Instantiate(cellPrefab, new Vector3(xPos, 0, zPos), Quaternion.identity);
                cell.transform.parent = transform;
                cell.gridX = x;
                cell.gridZ = z;
                grid[x, z] = cell;
            }
        }
    }

    public void CollectGridFromChildren()
    {
        if (grid != null && grid.GetLength(0) == gridSizeX && grid.GetLength(1) == gridSizeZ)
            return;

        grid = new CellScript[gridSizeX, gridSizeZ];
        foreach (Transform child in transform)
        {
            CellScript cell = child.GetComponent<CellScript>();
            if (cell != null && cell.gridX >= 0 && cell.gridX < gridSizeX && cell.gridZ >= 0 && cell.gridZ < gridSizeZ)
            {
                grid[cell.gridX, cell.gridZ] = cell;
            }
        }
    }

    public CellScript GetNearestCell(Vector3 worldPos)
    {
        CollectGridFromChildren();

        CellScript nearest = null;
        float minDist = float.MaxValue;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                if (grid[x, z] == null) continue;
                float dist = Vector3.Distance(worldPos, grid[x, z].transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = grid[x, z];
                }
            }
        }
        return nearest;
    }

    public CellScript GetCell(int x, int z)
    {
        CollectGridFromChildren();

        if (x < 0 || x >= gridSizeX || z < 0 || z >= gridSizeZ)
            return null;
        return grid[x, z];
    }

    /// <summary>
    /// Parçanın tüm hücrelerini direction dizisi ile bulur.
    /// directions: her eleman bir komşu yönü (0-5). Boş dizi = sadece merkez.
    /// Örnek: hexCount=2 → directions={2} (sağ-alt), hexCount=3 → directions={2,3} (sağ-alt, sol-alt)
    /// </summary>
    public CellScript[] GetPieceCells(CellScript centerCell, int[] directions)
    {
        CellScript[] cells = new CellScript[1 + directions.Length];
        cells[0] = centerCell;

        int cx = centerCell.gridX;
        int cz = centerCell.gridZ;

        for (int i = 0; i < directions.Length; i++)
        {
            CellScript neighbor = GetNeighbor(cx, cz, directions[i]);
            cells[i + 1] = neighbor;
        }
        return cells;
    }

    public bool CanPlacePiece(CellScript centerCell, int[] directions)
    {
        CellScript[] cells = GetPieceCells(centerCell, directions);
        foreach (var cell in cells)
        {
            if (cell == null || cell.isOccupied)
                return false;
        }
        return true;
    }

    public void PlacePiece(CellScript centerCell, int[] directions)
    {
        CellScript[] cells = GetPieceCells(centerCell, directions);
        foreach (var cell in cells)
        {
            if (cell != null)
                cell.isOccupied = true;
        }
    }

    /// <summary>
    /// Hex komşu yönleri. Even ve odd satırlar için ayrı offset'ler.
    /// Sıra: sağ-üst, sağ, sağ-alt, sol-alt, sol, sol-üst
    /// </summary>
    private static readonly Vector2Int[,] hexNeighborOffsets = new Vector2Int[2, 6]
    {
        { // even row (z % 2 == 0)
            new Vector2Int(0, 1),   // sağ-üst
            new Vector2Int(1, 0),   // sağ
            new Vector2Int(0, -1),  // sağ-alt
            new Vector2Int(-1, -1), // sol-alt
            new Vector2Int(-1, 0),  // sol
            new Vector2Int(-1, 1)   // sol-üst
        },
        { // odd row (z % 2 == 1)
            new Vector2Int(1, 1),   // sağ-üst
            new Vector2Int(1, 0),   // sağ
            new Vector2Int(1, -1),  // sağ-alt
            new Vector2Int(0, -1),  // sol-alt
            new Vector2Int(-1, 0),  // sol
            new Vector2Int(0, 1)    // sol-üst
        }
    };

    /// <summary>
    /// Belirli bir hücreden belirli bir yöndeki komşuyu döndürür.
    /// direction: 0=sağ-üst, 1=sağ, 2=sağ-alt, 3=sol-alt, 4=sol, 5=sol-üst
    /// </summary>
    public CellScript GetNeighbor(int x, int z, int direction)
    {
        int parity = z % 2;
        Vector2Int offset = hexNeighborOffsets[parity, direction];
        return GetCell(x + offset.x, z + offset.y);
    }

    /// <summary>
    /// HexCount'a göre komşu yön dizisi döndürür.
    /// 1 → boş dizi (sadece merkez), 2 → {2} (sağ-alt), 3 → {2, 3} (sağ-alt + sol-alt)
    /// </summary>
    public static int[] GetDirectionsForHexCount(int hexCount)
    {
        switch (hexCount)
        {
            case 1: return new int[0];
            case 2: return new int[] { 2 };        // sağ-alt
            case 3: return new int[] { 2, 3 };     // sağ-alt + sol-alt
            default: return new int[0];
        }
    }
}
