using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameObjectWorld : MonoBehaviour
{
    public static GameObjectWorld Instance;
    public Transform cameraFollow;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            DestroyImmediate(this.gameObject);
    }
}
