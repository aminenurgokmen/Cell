using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WaveManager : MonoBehaviour
{
    [Header("Spawnlanacak Objeler")]
    public List<GameObject> objectList;   // Prefabları buraya ekle

    [Header("Spawn Noktaları (3 tane)")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public Transform spawnPoint3;

    [HideInInspector]
    public List<GameObject> spawnedPieces = new List<GameObject>();

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
            Vector3 targetPos = spawnPoints[i].position;
            Vector3 startPos = targetPos + new Vector3(0, 0, -2f);
            GameObject spawned = Instantiate(randomObject, startPos, spawnPoints[i].rotation);
            spawned.transform.DOMove(targetPos, 0.4f).SetEase(Ease.OutBack);
            spawnedPieces.Add(spawned);

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