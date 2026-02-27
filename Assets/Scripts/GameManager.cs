using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<Material> materials;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {

    }
}
