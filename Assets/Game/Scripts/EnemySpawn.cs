using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{

    public Enemy enemy;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(UnityTags.SPAWN))
        {
            enemy.Explode();
        }
    }

}
