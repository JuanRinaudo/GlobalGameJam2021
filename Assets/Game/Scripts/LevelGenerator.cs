using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    [Header("General generation")]
    public Player player;
    public Spawn spawn;
    public float mapSize = 150;
    public GameObject clearArea;

    [Header("Floor generation")]
    public Vector2 perlinOffset;
    public float cellSize = 1;
    public float centerFlatZone = 1;
    public int maxHeight = 2;

    [Header("Magic stone generation")]
    public GameObject magicStonePrefab;
    public List<float> stoneDistances;

    [Header("Tree generation")]
    public LayerMask treeGenerationMask;
    public GameObject treePrefab;
    public Vector2 treePerlinOffset;
    public float probeOffset = .2f;
    public float treeThreshold = .8f;
    public float treeWobbleSize = 0.25f;
    
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    void Awake()
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
        RaycastHit hit;

        // #NOTE (Juan): Floor
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        int vertexCount = 0;

        for (float x = 0; x < mapSize; x += cellSize)
        {
            for (float z = 0; z < mapSize; z += cellSize)
            {
                float topLeft = PerlinSafeZoned(perlinOffset.x + x, perlinOffset.y + z) * maxHeight;
                float topRight = PerlinSafeZoned(perlinOffset.x + x + cellSize, perlinOffset.y + z) * maxHeight;
                float bottomLeft = PerlinSafeZoned(perlinOffset.x + x, perlinOffset.y + z + cellSize) * maxHeight;
                float bottomRight = PerlinSafeZoned(perlinOffset.x + x + cellSize, perlinOffset.y + z + cellSize) * maxHeight;

                vertices.Add(new Vector3(x, topLeft, z));
                vertices.Add(new Vector3(x + cellSize, topRight, z));
                vertices.Add(new Vector3(x, bottomLeft, z + cellSize));
                vertices.Add(new Vector3(x + cellSize, bottomRight, z + cellSize));

                indices.Add(vertexCount + 2);
                indices.Add(vertexCount + 1);
                indices.Add(vertexCount + 0);
                indices.Add(vertexCount + 2);
                indices.Add(vertexCount + 3);
                indices.Add(vertexCount + 1);

                vertexCount += 4;
            }
        }

        meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        // #NOTE (Juan): Spawn area
        ClearArea[] clearAreas = GetComponentsInChildren<ClearArea>();
        for (int i = 0; i < clearAreas.Length; ++i)
        {
            DestroyImmediate(clearAreas[i].gameObject);
        }

        GenerateClearArea(mapSize * 0.5f, mapSize * 0.5f, centerFlatZone * 0.75f);

        // #NOTE (Juan): Magic stones
        MagicStone[] magicStones = GetComponentsInChildren<MagicStone>();
        for (int i = 0; i < magicStones.Length; ++i)
        {
            DestroyImmediate(magicStones[i].gameObject);
        }

        for(int i = 0; i < stoneDistances.Count; ++i)
        {
            Vector2 randomCirclePosition = Random.insideUnitCircle;
            Vector3 magicStonePosition = new Vector3(mapSize * 0.5f, 0, mapSize * 0.5f) + new Vector3(randomCirclePosition.x, 0, randomCirclePosition.y) * mapSize * 0.5f;
            GenerateClearArea(magicStonePosition.x, magicStonePosition.z, 3);

            Physics.Raycast(new Vector3(magicStonePosition.x, 50, magicStonePosition.z), Vector3.down, out hit, float.PositiveInfinity, UnityLayers.FLOOR_MASK);

            GameObject magicStoneInstance = Instantiate(magicStonePrefab, transform);
            magicStoneInstance.transform.position = hit.point;
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
                Physics.Raycast(new Vector3(x, 50, z), Vector3.down, out hit, float.PositiveInfinity, treeGenerationMask);
                if (hit.transform.gameObject.layer != UnityLayers.NOGENERATION_INDEX && Mathf.PerlinNoise(treePerlinOffset.x + x, treePerlinOffset.y + z) > treeThreshold)
                {
                    Vector2 treeWobble = Random.insideUnitCircle * treeWobbleSize;
                    GameObject treeInstance = Instantiate(treePrefab, transform);
                    treeInstance.transform.position = hit.point + new Vector3(treeWobble.x, 0, treeWobble.y);
                }
                else if (hit.transform.gameObject.layer == UnityLayers.NOGENERATION_INDEX)
                {
                    Debug.Log("Safe zone");
                }
            }
        }

        // #NOTE (Juan): Player
        Physics.Raycast(new Vector3(mapSize * 0.5f, 10, mapSize * 0.5f), Vector3.down, out hit, float.PositiveInfinity, UnityLayers.FLOOR_MASK);

        spawn.transform.position = hit.point;

        player.transform.position = hit.point + new Vector3(0, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
