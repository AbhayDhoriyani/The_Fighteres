using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverLifeTime : MonoBehaviour
{
    [SerializeField] float lifeTime = 1f;

    void Start()
    {
        Destroy(gameObject, lifeTime);   
    }
}
