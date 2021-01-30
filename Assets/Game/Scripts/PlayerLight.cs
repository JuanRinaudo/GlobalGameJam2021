using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLight : MonoBehaviour
{

    private float lightRange;
    //private float particlesEmisionRate;
    private float trailTime;

    public Light lightEmitter;
    public ParticleSystem particles;
    public TrailRenderer trail;

    void Awake()
    {
        lightEmitter = GetComponent<Light>();

        lightRange = lightEmitter.range;
        //particlesEmisionRate = particles.emission.rateOverTime.constant;
        trailTime = trail.time;
    }

    void Update()
    {
        
    }

    public void SetLightPercent(float percent)
    {
        lightEmitter.range = lightRange * percent;
        trail.time = trailTime * percent;
    }
}
