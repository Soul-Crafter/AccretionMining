using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Maybe structure //
public struct Maybe<T>
{
    private readonly T _value;
    private readonly bool _hasValue;

    private Maybe(T value)
    {
        _value = value;
        _hasValue = true;
    }
    public static Maybe<T> Nothing => new Maybe<T>();
    public static Maybe<T> Just(T value) => new Maybe<T>(value);
    public T Value => _hasValue ? _value : throw new System.InvalidOperationException("No value present");
    public bool HasValue => _hasValue;
    public T GetValueOrDefault() => _value;
    public T GetValueOrDefault(T defaultValue) => _hasValue ? _value : defaultValue;
    public override string ToString() => _hasValue ? _value.ToString() : "Nothing";
}

public class GptGenerator : MonoBehaviour
{
    // Constants (Gen parameters and debug)
    [SerializeField] int MAXSTACKCALL = 100000;
    [SerializeField] float _step;
    [SerializeField] float _cutoff;
    [SerializeField] float regionSize;
    [SerializeField] float maxDist;
    [SerializeField] float eccentricity;

    // Debug
    [SerializeField] bool debugMode;
    [SerializeField] bool debugTriMode;
    [SerializeField] GameObject cube;
    [SerializeField] GameObject cube2;

    // Triangle Gen & Finalization
    [SerializeField] List<int> orderedTris = new List<int>();

    HashSet<Vector2> vertHolder = new HashSet<Vector2>();
    HashSet<Vector2> checkedPos = new HashSet<Vector2>();
    Dictionary<Vector3, int> region = new Dictionary<Vector3, int>();

    // Noise
    FractalBrownianMotionNoise noise = new FractalBrownianMotionNoise(octaves: 8, lacunarity: 2f, persistence: 0.5f, scale: 50f);


    // Final mesh data holders //
    public int[] finalTris;
    public Vector3[] finalVerts;
    





    // Start is called before the first frame update
    void Start()
    {
        // Determine the starting region and fill it. //
        Maybe<HashSet<Vector2>> foundRegion = findRegion();
        while(!foundRegion.HasValue)
        {
            foundRegion = findRegion();
        }

        Debug.Log(foundRegion.Value);
        region = ConvertVector2SetToVector3Dictionary(foundRegion.Value);
        Debug.Log(region.Count);

        // Generate triangles //
        getTriangles(region, orderedTris);

        // Update and finalize mesh. //
        updateMesh(finalTris, finalVerts);

        // Debug
        if (debugMode == true) debugCubes();
        if (debugTriMode == true) debugTris();
    }



    // Update is called once per frame
    void Update()
    {

    }



    // FIND REGION //
    /// <para>
    /// 1. Picks a seed
    /// 2. Walk right until region above _cutoff is found. (EXPLORE)
    /// 3. Determine all valid points above _cutoff using floodfill.
    /// 4. Repeat process if below specified region size.
    /// </para>
    private Maybe<HashSet<Vector2>> findRegion()
    {
        // Choose a seed and find a starting position
        Debug.Log("Starting.");
        float seed = Random.Range(500, 5000);
        float inc = Explore(seed);

        // Using the starting position, flood the area. //
        var root = new Vector2(seed + inc, seed);
        HashSet<Vector2> floodOut = null;
        floodOut = FloodFill(root, checkedPos, vertHolder, maxDist, MAXSTACKCALL);

        if (floodOut == null)
            return Maybe<HashSet<Vector2>>.Nothing;

        return Maybe<HashSet<Vector2>>.Just(floodOut);
    }



    // EXPLORE //
    private float Explore(float seed)
    {
        Debug.Log(seed);
        // Seed is the starting position in the perlin noise
        // Step is the resolution of the search
        // Cutoff is the minimum value of the perlin noise at a position to be considered.

        // Find starting position @ (seed + increment, seed)
        float increment = 0;
        while (noise.GetNoise(seed + increment, seed) < _cutoff)
        {
            increment += _step;
            Debug.Log("Searching for starting pos " + noise.GetNoise(seed + increment, seed));
        }
        Debug.Log("Found starting pos! " + increment + " " + noise.GetNoise(seed + increment, seed));




        //Debug.Log(string.Join(" \n", vertHolder.Select(x => string.Format("{0}, {1}", x.Key.x, x.Key.z))));

        return increment;
    }



    // FLOODFILL //
    HashSet<Vector2> FloodFill(Vector2 startPosition, HashSet<Vector2> visited, HashSet<Vector2> region, float maxDistance = 10, int maxStack = 10000)
    {
        Debug.Log("Flooding");
        visited.Clear();
        region.Clear();

        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(startPosition);

        while (stack.Count > 0 && region.Count < maxStack)
        {
            Vector2 position = stack.Pop();

            if (visited.Contains(position))
                continue;

            visited.Add(position);

            // Check if the current cell is above the threshold value and within the maximum distance
            float distance = Vector2.Distance(startPosition, position);
            if (GetCellValue(position) > _cutoff && distance <= maxDistance)
            {
                region.Add(position);

                // Push neighboring cells onto the stack if they are within the maximum distance
                PushIfWithinDistance(stack, startPosition, position + Vector2.right * _step, maxDistance);
                PushIfWithinDistance(stack, startPosition, position + Vector2.left * _step, maxDistance);
                PushIfWithinDistance(stack, startPosition, position + Vector2.up * _step, maxDistance);
                PushIfWithinDistance(stack, startPosition, position + Vector2.down * _step, maxDistance);
            }
        }

        if (region.Count >= regionSize)
        {
            Debug.Log("Returning region.");
            return region;
        }
        else
            return null;
    }



    void PushIfWithinDistance(Stack<Vector2> stack, Vector2 startPosition, Vector2 position, float maxDistance)
    {
        float distance = Vector2.Distance(startPosition, position);
        if (distance <= maxDistance)
        {
            stack.Push(position);
        }
    }



    float GetCellValue(Vector2 position)
    {
        // This function would return the value of the cell at position
        // This could be implemented in a number of ways depending on the specific problem being solved
        return noise.GetNoise(position.x, position.y);
    }





    // DEBUG
    // DEBUG CUBES
    private void debugCubes()
    {
        foreach (var x in region)
        {
            //Debug.Log(string.Format("({0}, {1}, {2})", x.Key.x, x.Key.y, x.Key.z));
            Instantiate(cube, x.Key, Quaternion.Euler(90, 0, 0));
        }
    }


    // DEBUG TRIANGLES (INOP)
    private void debugTris()
    {
        Debug.Log("Debugging tris");
        int counter = 0;
        GameObject indicator = cube;

        foreach(var i in finalTris)
        {
            Instantiate(indicator, finalVerts[i], Quaternion.identity);
            if(counter >= 2)
            {
                counter = 0;
                indicator.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
            }
            else
            {
                counter++;
            }
        }
    }





    
    // Conversions //
    public static Dictionary<Vector3, int> ConvertVector2SetToVector3Dictionary(HashSet<Vector2> vector2Set)
    {
        return vector2Set
            .Select((vector2, index) => new { Vector3 = new Vector3(vector2.x, 0, vector2.y), Index = index })
            .ToDictionary(item => item.Vector3, item => item.Index);
    }



    public static Dictionary<Vector3, int> ConvertVector2SetToVector3DictionaryWithNoise(HashSet<Vector2> vector2Set, FractalBrownianMotionNoise lclNoise)
    {
        return vector2Set       
            .Select((vector2, index) => new { Vector3 = new Vector3(vector2.x, lclNoise.GetNoise(vector2.x, vector2.y) * 2, vector2.y), Index = index })
            .ToDictionary(item => item.Vector3, item => item.Index);
    }





    // GET TRIANGLES //
    private void getTriangles(Dictionary<Vector3, int> vertDict, List<int> tTris)
    {
        // Define bounds of imaginary quad. (Find max and min values)
        var firstVert = vertDict.First().Key;
        var extrema = new Extrema(firstVert.x, firstVert.x, firstVert.z, firstVert.z);
        int counter = 2;
        foreach (var i in vertDict.Keys) 
        {
            if (i.x > extrema.maxX)
                extrema.maxX = i.x;
            if (i.x < extrema.minX)
                extrema.minX = i.x;
            if (i.z > extrema.maxZ)
                extrema.maxZ = i.z;
            if (i.z < extrema.minZ)
                extrema.minZ = i.z;

            counter++;

            // Debug lines for the bounds finder //
            Debug.Log(string.Format("min({0}, {1}), max({2}, {3})", extrema.minX, extrema.minZ, extrema.maxX, extrema.maxZ));
            //Instantiate(cube, new Vector3(extrema.minX, counter / 1000 + 2, extrema.minZ), Quaternion.identity);
            //Instantiate(cube, new Vector3(extrema.maxX, counter / 1000 + 2, extrema.maxZ), Quaternion.identity);
        }

        //Instantiate(cube2, new Vector3(extrema.minX, counter / 1000 + 3, extrema.minZ), Quaternion.identity);
        //Instantiate(cube2, new Vector3(extrema.maxX, counter / 1000 + 3, extrema.maxZ), Quaternion.identity);

        Debug.Log(string.Format("min({0}, {1}), max({2}, {3})", extrema.minX, extrema.minZ, extrema.maxX, extrema.maxZ));
        
        // Creates the list of triangle indices //
        for(float x = extrema.minX; x < extrema.maxX; x += _step)
        {
            for(float z = extrema.minZ; z < extrema.maxZ; z += _step)
            {
                checkQuad(new Vector3(x, 0, z), region);
            }
        }

        // Convert tri index list to array. //
        // Convert vertex dictionary to array. //

        // Place triangles in array. //
        finalTris = orderedTris.ToArray();

        // Transfers vert dict to final vert array and offsets them to be centered around the local origin. //
        Dictionary<int, Vector3> revRegion = region.ToDictionary(kp => kp.Value, kp => kp.Key);

        // Average extrema to determine offset that is required.
        float offsetX = (extrema.maxX + extrema.minX) / 2;
        float offsetZ = (extrema.maxZ + extrema.minZ) / 2;

        finalVerts = new Vector3[region.Count];
        foreach (var i in revRegion) 
            finalVerts[i.Key] = new Vector3(
                                    i.Value.x - offsetX,
                                    noise.GetNoise(i.Value.x, i.Value.z) * eccentricity, 
                                    i.Value.z - offsetZ);
    }


    // CHECK QUAD //
    private void checkQuad(Vector3 current, Dictionary<Vector3, int> vertDict)
    {
        // Stores in the indices of the desired verts //
        int tl = -1;
        int tr = -1;
        int bl = -1;
        int br = -1;

        if (vertDict.ContainsKey(current))
        {
            tl = vertDict[current]; // saves the index of the current pos
            Debug.Log("TL");

            if (vertDict.ContainsKey(moveVector3(current, _step, 0, _step)))
            {
                br = vertDict[moveVector3(current, _step, 0, _step)];
                Debug.Log("BR");

                if (vertDict.ContainsKey(stepIn(current, _step)))
                    orderedTris = orderedTris.Concat(new int[] {
                                                vertDict[stepIn(current, _step)],
                                                br,
                                                tl })
                                            .ToList();

                if (vertDict.ContainsKey(stepRight(current, _step)))
                    orderedTris = orderedTris.Concat(new int[] {
                                                br,
                                                vertDict[stepRight(current, _step)],
                                                tl })
                                            .ToList();
            }
            else if (vertDict.ContainsKey(stepIn(current, _step)) &&
                     vertDict.ContainsKey(stepRight(current, _step)))
                orderedTris = orderedTris.Concat(new int[] {
                                            vertDict[stepIn(current, _step)],
                                            vertDict[stepRight(current, _step)],
                                            tl })
                                        .ToList();
        }
        else if (vertDict.ContainsKey(stepIn(current, _step)) &&
                 vertDict.ContainsKey(stepRight(current, _step)) &&
                 vertDict.ContainsKey(moveVector3(current, _step, 0, _step)))
            orderedTris = orderedTris.Concat(new int[] {
                                        vertDict[stepIn(current, _step)],
                                        vertDict[moveVector3(current, _step, 0, _step)],
                                        vertDict[stepRight(current, _step)] })
                                    .ToList();
    }

    

    // Easy step functions
    // Vector3 v is the starting position
    // float is the _step value.
    Vector3 moveVector3(Vector3 v, float x, float y, float z) => new Vector3(v.x + x, v.y + y, v.z + z);
    Vector3 stepUp(Vector3 v, float y) => new Vector3(v.x, v.y + y, v.z);
    Vector3 stepDown(Vector3 v, float y) => new Vector3(v.x, v.y - y, v.z);

    Vector3 stepRight(Vector3 v, float x) => new Vector3(v.x + x, v.y, v.z);
    Vector3 stepLeft(Vector3 v, float x) => new Vector3(v.x - x, v.y, v.z);

    Vector3 stepIn(Vector3 v, float z) => new Vector3(v.x, v.y, v.z + z);
    Vector3 stepOut(Vector3 v, float z) => new Vector3(v.x, v.y, v.z - z);


    // Step functions for R2 Vectors. //
    Vector2 moveVector2(Vector2 v, float x, float y) => new Vector2(v.x + x, v.y + y);

    Vector2 stepUp(Vector2 v, float y) => new Vector2(v.x, v.y + y);
    Vector2 stepDown(Vector2 v, float y) => new Vector2(v.x, v.y - y);

    Vector2 stepRight(Vector2 v, float x) => new Vector2(v.x + x, v.y);
    Vector2 stepLeft(Vector2 v, float x) => new Vector2(v.x - x, v.y);

    // Update mesh //
    private void updateMesh(int[] triangles, Vector3[] vertices)
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = triangles;

        newMesh.RecalculateNormals();

        gameObject.GetComponent<MeshFilter>().mesh = newMesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = newMesh;

    }

    private void listArrayConcat<T> (List<T> list, T[] array)
    {
        foreach (T i in array) list.Add(i);
    }

}

// EXTREMA FOR QUAD OVERLAY //
class Extrema
{
    public float maxX { get; set; }
    public float minX { get; set; }
    public float maxZ { get; set; }
    public float minZ { get; set; }

    public Extrema()
    {
        maxX = float.MinValue;
        minX = float.MaxValue;
        maxZ = float.MinValue;
        minZ = float.MaxValue;
    }

    public Extrema(float maxX, float minX, float maxZ, float minZ)
    {
        this.maxX = maxX;
        this.minX = minX;
        this.maxZ = maxZ;
        this.minZ = minZ;
    }
}
