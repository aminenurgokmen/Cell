using UnityEngine;

public class CellScript : MonoBehaviour
{
    public int gridX;
    public int gridZ;
    public bool isOccupied = false;
    public bool _isOccupied;



    void Update()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.down * 1f;
        Debug.DrawRay(origin, Vector3.up * 3f, Color.red);
        if (GameManager.Instance.droped)
        {
            if (Physics.Raycast(origin, Vector3.up, out hit, 3f, 1 << 7))
            {
                Debug.Log("Hit: " + hit.collider.gameObject.name);

                _isOccupied = true;
            }

        }


    }
}
