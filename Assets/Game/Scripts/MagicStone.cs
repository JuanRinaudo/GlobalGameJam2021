using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicStone : MonoBehaviour
{

    public BoxCollider collider;
    public int index;
    public bool followingPlayer;

    public AudioSource pickup;

    public float floatDistance = 1;
    public float floatSpeed = 3;

    public float lerpSpeed = 2;
    private float lerpValue = 1;
    private Vector3 startPoint;
    private Vector3 targetPoint;

    private Vector3 startingPosition;

    void Start()
    {
        startingPosition = transform.position;
    }

    void Update()
    {
        if(collider.enabled)
        {
            transform.position = startingPosition + Vector3.up * (floatDistance * 0.5f + Mathf.Sin(Time.time * floatSpeed) * floatDistance * 0.4f);
        }
        else if(followingPlayer)
        {
            lerpValue = Mathf.Min(lerpValue + Time.deltaTime * lerpSpeed, 1);
            if (lerpValue >= 1)
            {
                startPoint = transform.position;
                targetPoint = transform.parent.position + Random.onUnitSphere * 2 + Vector3.up;
                lerpValue = 0;
            }

            transform.position = Vector3.Lerp(startPoint, targetPoint, lerpValue);
        }
    }

    public void GoToSlot()
    {
        followingPlayer = false;
        transform.parent = null;

        LeanTween.move(gameObject, Level.instance.spawn.stoneSlot[index].transform.position + Vector3.down * .75f, 2f).setEase(LeanTweenType.linear).setOnComplete(() =>
        {
            gameObject.SetActive(false);
            Level.instance.spawn.stoneSlot[index].gameObject.SetActive(true);
        });
    }

    public void Collect(GameObject parent)
    {
        pickup.Play();

        collider.enabled = false;
        transform.parent = parent.transform;

        followingPlayer = true;
    }
}
