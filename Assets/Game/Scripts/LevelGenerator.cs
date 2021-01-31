using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class LevelGenerator : MonoBehaviour
{

    [Header("General generation")]
    public Level level;
    public Player player;
    public int mapSize = 150;
    public int chunkSize = 10;
    public GameObject clearArea;

    [Header("Pathfinder")]
    public AstarPath pathfinder;

    [Header("Floor generation")]
    public GameObject chunkPrefab;
    public Vector2 perlinOffset;
    public float cellSize = 1;
    public float centerFlatZone = 1;
    public int maxHeight = 2;

    [Header("Monster generation")]
    public GameObject monsterPrefab;
    public int monsterCount = 30;
    public LayerMask monsterGenerationMask;

    [Header("Spawn generation")]
    public GameObject spawnPrefab;

    [Header("Magic stone generation")]
    public GameObject[] magicStonePrefabs;
    public List<float> stoneDistances;

    [Header("Tree generation")]
    public LayerMask treeGenerationMask;
    public GameObject[] treePrefabs;
    public Vector2 treePerlinOffset;
    public float probeOffset = .2f;
    public float treeThreshold = .8f;
    public float treeWobbleSize = 0.25f;

    void Start()
    {
        GenerateLevel();
    }

    private GameObject GenerateClearArea(float x, float z, float radius)
    {
        GameObject clearAreaInstance = Instantiate(clearArea, transform);
        clearAreaInstance.transform.position = new Vector3(x, maxHeight, z);
        clearAreaInstance.GetComponent<SphereCollider>().radius = radius;

        Physics.SyncTransforms();

        return clearAreaInstance;
    }

    private float PerlinSafeZoned(float x, float z)
    {
        float centerDistance = (new Vector3(x, 0, z) - new Vector3(mapSize * .5f, 0, mapSize * .5f)).magnitude;

        return centerDistance > centerFlatZone ? Mathf.PerlinNoise(perlinOffset.x + x, perlinOffset.y + z) : 0;
    }

    [ContextMenu("Generate level")]
    private void GenerateLevel()
    {
        bool hitted;
        RaycastHit hit;

        level.pathfinder = pathfinder;

        // #NOTE (Juan): Floor
        Chunk[] chunks = GetComponentsInChildren<Chunk>();
        for (int i = 0; i < chunks.Length; ++i)
        {
            DestroyImmediate(chunks[i].gameObject);
        }

        for (int cX = 0; cX <= Mathf.Ceil(mapSize / chunkSize); cX++)
        {
            for (int cZ = 0; cZ <= Mathf.Ceil(mapSize / chunkSize); cZ++)
            {
                GameObject chunkInstance = Instantiate(chunkPrefab, transform);
                chunkInstance.transform.position = Vector3.zero;

                List<Vector3> vertices = new List<Vector3>();
                List<int> indices = new List<int>();
                int vertexCount = 0;

                int chunkWidth = chunkSize;
                int chuckHeight = chunkSize;
                for (float x = 0; x < chunkWidth; x += cellSize)
                {
                    for (float z = 0; z < chuckHeight; z += cellSize)
                    {
                        float chunkX = cX * chunkSize;
                        float chunkZ = cZ * chunkSize;
                        
                        float topLeft = PerlinSafeZoned(perlinOffset.x + chunkX + x, perlinOffset.y + chunkZ + z) * maxHeight;
                        float topRight = PerlinSafeZoned(perlinOffset.x + chunkX + x + cellSize, perlinOffset.y + chunkZ + z) * maxHeight;
                        float bottomLeft = PerlinSafeZoned(perlinOffset.x + chunkX + x, perlinOffset.y + chunkZ + z + cellSize) * maxHeight;
                        float bottomRight = PerlinSafeZoned(perlinOffset.x + chunkX + x + cellSize, perlinOffset.y + chunkZ + z + cellSize) * maxHeight;

                        vertices.Add(new Vector3(chunkX + x, topLeft, chunkZ + z));
                        vertices.Add(new Vector3(chunkX + x + cellSize, topRight, chunkZ + z));
                        vertices.Add(new Vector3(chunkX + x, bottomLeft, chunkZ + z + cellSize));
                        vertices.Add(new Vector3(chunkX + x + cellSize, bottomRight, chunkZ + z + cellSize));

                        indices.Add(vertexCount + 2);
                        indices.Add(vertexCount + 1);
                        indices.Add(vertexCount + 0);
                        indices.Add(vertexCount + 2);
                        indices.Add(vertexCount + 3);
                        indices.Add(vertexCount + 1);

                        vertexCount += 4;
                    }
                }

                MeshFilter  meshFilter = chunkInstance.GetComponent<MeshFilter>();
                Mesh mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
                meshFilter.mesh = mesh;

                MeshCollider meshCollider = chunkInstance.GetComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
            }
        }

        // #NOTE (Juan): Destroy clear areas
        ClearArea[] clearAreas = GetComponentsInChildren<ClearArea>();
        for (int i = 0; i < clearAreas.Length; ++i)
        {
            DestroyImmediate(clearAreas[i].gameObject);
        }

        // #NOTE (Juan): Spawn area
        Spawn[] spawns = GetComponentsInChildren<Spawn>();
        for (int i = 0; i < spawns.Length; ++i)
        {
            DestroyImmediate(spawns[i].gameObject);
        }

        hitted = Physics.Raycast(new Vector3(mapSize * 0.5f, 10, mapSize * 0.5f), Vector3.down, out hit, float.PositiveInfinity, UnityLayers.FLOOR_MASK);

        if(hitted)
        {
            GameObject spawnInstance = Instantiate(spawnPrefab, transform);
            level.spawn = spawnInstance.GetComponent<Spawn>();
            GenerateClearArea(mapSize * 0.5f, mapSize * 0.5f, centerFlatZone * 0.75f);

            player.spawnFinder.targetTransform = spawnInstance.transform;

            spawnInstance.transform.position = hit.point;
        }

        // #NOTE (Juan): Magic stones
        MagicStone[] magicStones = GetComponentsInChildren<MagicStone>();
        for (int i = 0; i < magicStones.Length; ++i)
        {
            DestroyImmediate(magicStones[i].gameObject);
        }

        for(int i = 0; i < stoneDistances.Count; ++i)
        {
            float angle = Mathf.PI * 2 * Random.Range(0f, 1f);
            Vector3 magicStonePosition = new Vector3(mapSize * 0.5f, 0, mapSize * 0.5f) + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * mapSize * 0.5f * stoneDistances[i];
            GenerateClearArea(magicStonePosition.x, magicStonePosition.z, 3);

            hitted = Physics.Raycast(new Vector3(magicStonePosition.x, 50, magicStonePosition.z), Vector3.down, out hit, float.PositiveInfinity, UnityLayers.FLOOR_MASK);

            if(hitted)
            {
                GameObject magicStoneInstance = Instantiate(magicStonePrefabs[i], transform);
                magicStoneInstance.transform.position = hit.point;

                MagicStone magicStone = magicStoneInstance.GetComponent<MagicStone>();
                magicStone.index = i;
                level.magicStones.Add(magicStone);

                player.magicStoneFinder[i].targetTransform = magicStoneInstance.transform;
            }
        }

        // #NOTE (Juan): Trees
        Tree[] trees = GetComponentsInChildren<Tree>();
        for (int i = 0; i < trees.Length; ++i)
        {
            DestroyImmediate(trees[i].gameObject);
        }

        for (float x = 0; x < mapSize; x += probeOffset)
        {
            for (float z = 0; z < mapSize; z += probeOffset)
            {
                hitted = Physics.Raycast(new Vector3(x, 50, z), Vector3.down, out hit, float.PositiveInfinity, treeGenerationMask);
                if (hitted && hit.transform.gameObject.layer != UnityLayers.NOGENERATION_INDEX && Mathf.PerlinNoise(treePerlinOffset.x + x, treePerlinOffset.y + z) > treeThreshold)
                {
                    Vector2 treeWobble = Random.insideUnitCircle * treeWobbleSize;
                    GameObject treeInstance = Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)], transform);
                    treeInstance.transform.position = hit.point + new Vector3(treeWobble.x, 0, treeWobble.y);
                }
            }
        }

        // #NOTE (Juan): Enemies
        Enemy[] monsters = GetComponentsInChildren<Enemy>();
        for (int i = 0; i < monsters.Length; ++i)
        {
            DestroyImmediate(monsters[i].gameObject);
        }

        int placeMonsterTryCount = 0;
        for (int i = 0; i < monsterCount && placeMonsterTryCount < monsterCount * 2; ++i)
        {
            Vector3 position = new Vector3(Random.Range(0, mapSize), 50, Random.Range(0, mapSize));
            hitted = Physics.SphereCast(position, 0.3f, Vector3.down, out hit, float.PositiveInfinity, monsterGenerationMask);
            if (hitted && hit.transform.gameObject.layer == UnityLayers.FLOOR_INDEX)
            {
                GameObject monsterInstance = Instantiate(monsterPrefab, transform);
                monsterInstance.transform.position = hit.point;
            }
            else
            {
                i--;
                placeMonsterTryCount++;
            }
        }

        // #NOTE (Juan): Player
        hitted = Physics.Raycast(new Vector3(mapSize * 0.5f, 10, mapSize * 0.5f), Vector3.down, out hit, float.PositiveInfinity, UnityLayers.FLOOR_MASK);

        if(hitted)
        {
            player.transform.position = hit.point + new Vector3(0, 1, 0);
        }

        // #NOTE (Juan): Pathfinding
        Physics.SyncTransforms();
        pathfinder.Scan();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
