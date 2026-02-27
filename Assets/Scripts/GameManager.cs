using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<Material> materials;

    public GameObject activeItem;   // Şu an sürüklenen obje
    private Camera mainCam;
    private float dragHeight = 0f;  // Objeyi hangi Y seviyesinde taşıyacağız

    void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
    }

    void Update()
    {
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

        if (Physics.Raycast(ray, out hit, 1 << 6))
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
        activeItem = null;
    }
}