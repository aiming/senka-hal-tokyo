using System;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public event Action OnGet;

    void OnTriggerEnter(Collider other)
    {
        OnGet();
    }
}
