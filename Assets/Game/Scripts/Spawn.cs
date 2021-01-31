using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{

    private Vector3 stoneStartPosition;
    public float stoneDeltaY = 1;
    public float stoneSpeed = 1;
    public float stoneOffset = 1.2345f;
    public GameObject stone;

    public GameObject[] stoneSlot;

    void Awake()
    {
        stoneStartPosition = stone.transform.localPosition;
    }

    void Update()
    {
        stone.transform.localPosition = stoneStartPosition + new Vector3(0, Mathf.Sin(Time.time * stoneSpeed + stoneOffset) * stoneDeltaY, 0);
    }
}
