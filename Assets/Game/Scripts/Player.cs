using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    [Header("References")]
    public new Rigidbody rigidbody;
    public List<MagicStone> magicStones;
    public new PlayerLight light;
    public float movementSpeed = 5;

    [Header("Light")]
    public bool rechargingLight = false;
    public float lightLevel = 1;
    public float perStoneCharge = 1;
    public float lightDropspeed = 0.1f;
    public float lightRecharge = 0.1f;

    private float rotationTime = 0;
    private Vector3 movementVelocity;
    private Quaternion targetRotation;

    void Awake()
    {

    }

    float GetMaxLightLevel()
    {
        return perStoneCharge * (magicStones.Count + 1);
    }

    void Update()
    {
        float maxLightLevel = GetMaxLightLevel();
        if (rechargingLight)
        {
            lightLevel = Mathf.Clamp(lightLevel + lightRecharge * Time.deltaTime, 0, maxLightLevel);
        }
        else
        {
            lightLevel -= lightDropspeed * Time.deltaTime;
        }

        light.SetLightPercent(lightLevel / maxLightLevel);

        Quaternion lastTargetRotation = targetRotation;
        movementVelocity = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            movementVelocity.z += 1;
            targetRotation = Quaternion.Euler(0, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            movementVelocity.z -= 1;
            targetRotation = Quaternion.Euler(0, 180, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            movementVelocity.x -= 1;
            targetRotation = Quaternion.Euler(0, 270, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            movementVelocity.x += 1;
            targetRotation = Quaternion.Euler(0, 90, 0);
        }

        if(targetRotation != lastTargetRotation)
        {
            rotationTime = 0;
        }
    }

    private void FixedUpdate()
    {
        rigidbody.transform.position = transform.position + movementVelocity * movementSpeed * Time.fixedDeltaTime;

        rotationTime = Mathf.Min(rotationTime + Time.fixedDeltaTime, 1);
        if(rotationTime < 1)
        {
            rigidbody.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(UnityTags.MAGICSTONE))
        {
            MagicStone magicStone = other.GetComponent<MagicStone>();
            magicStones.Add(magicStone);
            lightLevel = GetMaxLightLevel();
            magicStone.Collect(gameObject);
        }
        else if (other.gameObject.CompareTag(UnityTags.SPAWN))
        {
            rechargingLight = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(UnityTags.SPAWN))
        {
            rechargingLight = false;
        }
    }

}
