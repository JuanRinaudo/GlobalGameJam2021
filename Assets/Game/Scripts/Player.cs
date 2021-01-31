using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    [Header("References")]
    public Animator animator;
    public new Rigidbody rigidbody;
    public List<MagicStone> magicStones;
    public new PlayerLight light;
    public Light deathLight;
    public float deathLightSpeed = 4;
    private float deathLightIntensity;
    private bool deathLightUpping;
    private int flipCount = 0;
    public int maxFlipCount = 8;
    public float movementSpeed = 5;

    [Header("Light")]
    public bool rechargingLight = false;
    public float lightLevel = 1;
    public float perStoneCharge = 1;
    public float lightDropspeed = 0.1f;
    public float lightRecharge = 0.1f;

    [Header("Detector")]
    public bool finding;
    public TransformFinder spawnFinder;
    public List<TransformFinder> magicStoneFinder;
    public float finderCost = 1;

    [Header("Sound")]
    public float stepTime = 0.2f;
    private float stepTimer = 0;
    public AudioSource stepAudio;
    public AudioSource energyDrainAudio;
    public AudioSource heartbeatAudio;
    public int enemyDrainCount = 0;

    private float rotationTime = 0;
    private Vector3 movementVelocity;
    private Quaternion targetRotation;

    void Awake()
    {
        Game.instance.player = this;
    }

    float GetMaxLightLevel()
    {
        return perStoneCharge * (magicStones.Count + 1);
    }

    void Update()
    {
        if(Game.instance.running)
        {
            finding = Input.GetKey(KeyCode.Space) && lightLevel > 0f;

            if(enemyDrainCount > 0 || finding)
            {
                if(!energyDrainAudio.isPlaying)
                {
                    energyDrainAudio.Play();
                }
            }
            else if(energyDrainAudio.isPlaying)
            {
                energyDrainAudio.Stop();
            }

            deathLight.gameObject.SetActive(lightLevel <= 0);
            if (lightLevel <= 0)
            {
                if (deathLightIntensity > 3 || deathLightIntensity < 0)
                {
                    deathLightUpping = !deathLightUpping;
                    flipCount++;
                    heartbeatAudio.Play();
                }
                if(flipCount == maxFlipCount)
                {
                    animator.SetTrigger(GirlModelAnimator.DEATH_TRIGGER);
                    Game.instance.GameLost();
                }
                else
                {
                    deathLightIntensity = deathLightIntensity + (deathLightUpping ? 1 : -1) * Time.deltaTime * deathLightSpeed;
                    deathLight.intensity = deathLightIntensity;
                }
            }
            else
            {
                deathLightIntensity = 0;
                flipCount = 1;
                deathLightUpping = true;
            }

            spawnFinder.active = finding;
            for (int i = 0; i < magicStoneFinder.Count; ++i)
            {
                magicStoneFinder[i].active = finding;
            }

            float maxLightLevel = GetMaxLightLevel();
            if (rechargingLight)
            {
                lightLevel = Mathf.Clamp(lightLevel + lightRecharge * Time.deltaTime, 0, maxLightLevel);
            }
            else
            {
                lightLevel -= lightDropspeed * Time.deltaTime + (finding ? 1 : 0) * finderCost * Time.deltaTime;
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
            
            bool moving = movementVelocity.magnitude > 0;
            animator.SetBool(GirlModelAnimator.RUNNING_BOOL_HASH, moving);

            if(moving)
            {
                stepTimer += Time.deltaTime;
                if(stepTimer > stepTime)
                {
                    stepAudio.Play();
                    stepTimer = 0;
                }
            }
            else
            {
                stepTimer = 0;
            }

            if (targetRotation != lastTargetRotation)
            {
                rotationTime = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        if (Game.instance.running)
        {
            rigidbody.transform.position = transform.position + movementVelocity * movementSpeed * Time.fixedDeltaTime;

            rotationTime = Mathf.Min(rotationTime + Time.fixedDeltaTime, 1);
            if (rotationTime < 1)
            {
                rigidbody.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(UnityTags.MAGICSTONE))
        {
            MagicStone magicStone = other.GetComponent<MagicStone>();
            magicStones.Add(magicStone);
            magicStone.Collect(gameObject);

            magicStoneFinder[magicStone.index].gameObject.SetActive(false);

            lightLevel = GetMaxLightLevel();
        }
        else if (other.gameObject.CompareTag(UnityTags.SPAWN))
        {
            rechargingLight = true;

            if(magicStones.Count == 4)
            {
                for(int i = 0; i < magicStones.Count; ++i)
                {
                    magicStones[i].GoToSlot();
                }

                LeanTween.delayedCall(3f, Game.instance.GameWon);
            }
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
