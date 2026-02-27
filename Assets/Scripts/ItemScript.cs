using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public int itemID;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Renderer>().material = GameManager.Instance.materials[itemID];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
