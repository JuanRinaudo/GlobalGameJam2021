using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFinder : MonoBehaviour
{
    
    public Transform targetTransform;
    public LineRenderer lineRenderer;

    public bool active = false;

    private float atuneTime = 0;

    private float finderSpeed = 3f;
    private float sizeModifier = 4f;
    private float maxDistance = 100;

    void Update()
    {
        atuneTime = Mathf.Clamp01(atuneTime + Time.deltaTime * finderSpeed * (active ? 1 : -1));

        Vector3[] positions = new Vector3[2];
        positions[0] = transform.position;
        Vector3 targetDelta = targetTransform.position - transform.position;
        
        float distanceModified = Mathf.Clamp(maxDistance - targetDelta.magnitude, 0, maxDistance);
        
        positions[1] = transform.position + targetDelta.normalized * Mathf.Clamp(distanceModified / maxDistance, 0.1f, 1) * sizeModifier * atuneTime;

        lineRenderer.SetPositions(positions);
    }

    private void OnDisable()
    {
        atuneTime = 0;
    }

}
