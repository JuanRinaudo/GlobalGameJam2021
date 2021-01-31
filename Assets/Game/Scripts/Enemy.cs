using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class Enemy : MonoBehaviour
{

    public GameObject explosionParticles;

    public Animator animator;
    public AIPath aiPath;
    public new SkinnedMeshRenderer renderer;
    private MaterialPropertyBlock materialProperties;

    private Color normalColor = new Color(0.38f, 0.38f, 0.42f);
    private Color lightColor = new Color(0.94f, 0.81f, 0.66f);
    //private Color lightColor = new Color(0.44f, 0.84f, 0.85f);

    public AudioClip enemyDeath;

    public bool followPlayer = false;

    private float energyLevel;
    public float energyDrainModifier = 0.1f;
    public float explodeEnergyLevel = .5f;

    void Awake()
    {
        materialProperties = new MaterialPropertyBlock();

        energyLevel = 0;
    }

    void Update()
    {
        if(followPlayer)
        {
            aiPath.destination = Game.instance.player.transform.position;
        }
        else if(aiPath.reachedEndOfPath)
        {
            aiPath.destination = Vector3.positiveInfinity;
        }

        materialProperties.SetColor("_Color", Color.Lerp(normalColor, lightColor, energyLevel / explodeEnergyLevel));
        materialProperties.SetColor("_EmissionColor", Color.Lerp(Color.black, lightColor, energyLevel / explodeEnergyLevel));
        renderer.SetPropertyBlock(materialProperties);

        if(energyLevel >= explodeEnergyLevel)
        {
            Explode();
        }

        animator.SetBool(MonsterModelAnimator.RUNNING_BOOL_HASH, aiPath.velocity.magnitude > 0);
    }

    public void Explode()
    {
        Instantiate(explosionParticles).transform.position = transform.position;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(UnityTags.PLAYER))
        {
            followPlayer = true;
            Game.instance.player.enemyDrainCount++;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag(UnityTags.PLAYER))
        {
            float drainValue = Mathf.Min(Time.deltaTime * energyDrainModifier, Game.instance.player.lightLevel);
            Game.instance.player.lightLevel -= drainValue;
            energyLevel = Mathf.Min(energyLevel + drainValue, explodeEnergyLevel);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(UnityTags.PLAYER))
        {
            followPlayer = false;
            Game.instance.player.enemyDrainCount--;
        }
    }

    private void OnDestroy()
    {
        if(followPlayer)
        {
            Audio.Instance.PlaySound(enemyDeath, 0.3f);
            Game.instance.player.enemyDrainCount--;
        }
    }
}
