using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class chunkGen : MonoBehaviour
{
    [SerializeField] const int MAXSTACKCALL = 3000;
    [SerializeField] float _step;
    [SerializeField] float _cutoff;

    [SerializeField] GameObject cube;

    Dictionary<Vector3, int> vertHolder = new Dictionary<Vector3, int>();
    HashSet<Vector3> checkedPos = new HashSet<Vector3>();

    FractalBrownianMotionNoise noise = new FractalBrownianMotionNoise(octaves: 8, lacunarity: 2f, persistence: 0.5f, scale: 50f);


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting.");
        float seed = Random.Range(5, 25);
        float inc = Explore(seed);

        var root = new Vector3(seed + inc, 0, seed);
        checkedPos.Add(root);
        vertHolder.Add(root, vertHolder.Count);
        Scan(root);
        
        Debug.Log("Scan done! " + vertHolder.Count);
        debugCubes();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private float Explore(float seed)
    {
        Debug.Log(seed);
        // Seed is the starting position in the perlin noise
        // Step is the resolution of the search
        // Cutoff is the minimum value of the perlin noise at a position to be considered.

        // Find starting position @ (seed + increment, seed)
        float increment = 0;
        while (increment < 1000 && noise.GetNoise(seed + increment, seed) < _cutoff)
        {
            increment += _step;
            Debug.Log("Searching for starting pos " + noise.GetNoise(seed + increment, seed));
        }
        Debug.Log("Found starting pos! " + increment + " " + noise.GetNoise(seed + increment, seed));




        //Debug.Log(string.Join(" \n", vertHolder.Select(x => string.Format("{0}, {1}", x.Key.x, x.Key.z))));

        return increment;
    }

    private void Scan(Vector3 root)
    {
        Debug.Log("Starting scan.");
        // Scans points in a square around the root.
        var lclVerts = new Stack<Vector3>();


        for (float x = root.x - _step; x < root.x + _step; x += _step)
        {
            for (float z = root.z - _step; z < root.z + _step; z += _step)
            {
                var tempV = new Vector3(x, 0, z);
                Debug.Log(string.Format("Loop start, {0}, {1}, {2}", x, z, tempV));
                if (!checkedPos.Contains(tempV))
                {
                    checkedPos.Add(tempV);

                    if (vertHolder.Count < MAXSTACKCALL && noise.GetNoise(x, z) > _cutoff)
                    {
                        vertHolder.Add(tempV, vertHolder.Count);
                        lclVerts.Push(tempV);
                    }
                }
            }
        }

        while (vertHolder.Count < MAXSTACKCALL && lclVerts.Count > 0)
        {
            var vert = lclVerts.Pop();
            Scan(vert);
        }

        Debug.Log("Scan recursion complete!");

    }

    private void debugCubes()
    {
        foreach (var x in vertHolder)
        {
            Instantiate(cube, x.Key, Quaternion.Euler(90, 0, 0));
        }
    }
}
