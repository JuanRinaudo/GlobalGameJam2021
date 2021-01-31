using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExplosion : MonoBehaviour
{

    public ParticleSystem particles;

    void Awake()
    {
        particles.Play();
    }

    private void Update()
    {
        if(!particles.isPlaying)
        {
            Destroy(gameObject);
        }
    }

}
