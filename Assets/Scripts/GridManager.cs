using UnityEngine;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    public CellScript cellPrefab;
    public int gridSizeX = 5;
    public int gridSizeZ = 5;
    public float cellRadius = 1f;
    public bool generateGrid = false;

    private CellScript[,] grid;

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
        float xSpacing = Mathf.Sqrt(3f) * cellRadius;
        float zSpacing = 1.5f * cellRadius;

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
                grid[x, z] = cell;
            }
        }
    }
}
