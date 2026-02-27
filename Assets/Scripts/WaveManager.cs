using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Spawnlanacak Objeler")]
    public List<GameObject> objectList;   // Prefabları buraya ekle

    [Header("Spawn Noktaları (3 tane)")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public Transform spawnPoint3;

    void Start()
    {
        SpawnThreeObjects();
    }

    public void SpawnThreeObjects()
    {
        if (objectList.Count == 0)
        {
            Debug.LogWarning("Object list boş!");
            return;
        }

        // Spawn noktalarını diziye alıyoruz
        Transform[] spawnPoints = { spawnPoint1, spawnPoint2, spawnPoint3 };

        // 3 noktaya aynı anda spawn
        List<Material> materials = GameManager.Instance.materials;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject randomObject = objectList[Random.Range(0, objectList.Count)];
            GameObject spawned = Instantiate(randomObject, spawnPoints[i].position, spawnPoints[i].rotation);

            // Altındaki her item'a rastgele materyal ata
            ItemScript[] items = spawned.GetComponentsInChildren<ItemScript>();
            foreach (ItemScript item in items)
            {
                int randomIndex = Random.Range(0, materials.Count);
                item.itemID = randomIndex;
                item.GetComponent<Renderer>().material = materials[randomIndex];
            }
        }
    }
}