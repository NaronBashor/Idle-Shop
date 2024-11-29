using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinCountPrefab : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI coinText;
    [SerializeField] private float duration;
    [SerializeField] private float movementSpeed;

    private void Start()
    {
        coinText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        transform.position += Vector3.up * movementSpeed * Time.deltaTime;
        Destroy(this.gameObject, duration);
    }
}
