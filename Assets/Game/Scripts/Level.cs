using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Level : MonoBehaviour
{

    public static Level instance;

    public List<MagicStone> magicStones;
    public Spawn spawn;

    public AstarPath pathfinder;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        
    }
}
