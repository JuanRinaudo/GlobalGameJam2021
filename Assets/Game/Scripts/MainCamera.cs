using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{

    public Player player;
    public float followLerp = 0.8f;
    public float snapThreshold = 0.1f;

    private Vector3 offset;
    
    void Awake()
    {
        offset = transform.position - player.transform.position;
        transform.SetParent(null);
    }

    void FixedUpdate()
    {
        Vector3 targetPosition = player.transform.position + offset;
        Vector3 delta = targetPosition - player.transform.position;
        if(delta.magnitude < snapThreshold)
        {
            transform.position = player.transform.position + offset;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followLerp);
        }
    }
}
