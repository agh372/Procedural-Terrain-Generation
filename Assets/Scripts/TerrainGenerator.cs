using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int width;
    public int height;
    public float scale;


    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;


    const int mapChunkSize = 241;

    public TerrainType[] regions;


    public Renderer textureRender;


    private float prevX = 0;
    private float prevZ = 0;

    public void Start()
    {
         //  float[,,] noisemap = NoiseGenerator.GenerateNoiseMap(width, height, scale, seed, octaves, persistance,  lacunarity, offset);
        // CreateVisualMesh(noisemap);

       
    }


    public void GenerateTerrain()
    {
        float[,,] noiseMap = NoiseGenerator.GenerateNoiseMap(width, height, scale, seed, octaves, persistance, lacunarity, offset);

        Color[] colourMap = new Color[width * width];

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int curr = 0;
                for (int h = 0; h < height; h++)
                {

                    if (noiseMap[x, h, y] != 0)
                        curr++;
                    else
                        break;
                }

                for (int i = 0; i < regions.Length; i++)
                {
                    if (curr <= regions[i].height)
                    {
                        colourMap[y * width + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        Texture2D texture = TextureGenerator.TextureFromColourMap(colourMap, width, width);
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, height, texture.height);

        CreateVisualMesh(noiseMap, texture);

    }


    public Mesh visualMesh;
    protected MeshRenderer meshRenderer;
    protected MeshCollider meshCollider;
    protected MeshFilter meshFilter;


    public virtual void CreateVisualMesh(float[,,] map, Texture2D texture)
    {
        visualMesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    if (map[x, y, z] == 0) continue;
                    float brick = map[x, y, z];


                    if (IsTransparent(x - 1, y, z, map))
                        CreateFace(brick, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris,x,z);
                    // Right wall
                    if (IsTransparent(x + 1, y, z, map))
                        CreateFace(brick, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris, x, z);
                    // Bottom wall
                    if (IsTransparent(x, y - 1, z, map))
                        CreateFace(brick, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris, x, z);
                    // Top wall
                    if (IsTransparent(x, y + 1, z, map))
                        CreateFace(brick, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris, x, z);
                    // Back
                    if (IsTransparent(x, y, z - 1, map))
                        CreateFace(brick, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris, x, z);
                    // Front
                    if (IsTransparent(x, y, z + 1, map))
                        CreateFace(brick, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris, x, z);

                }
            }
        }

        visualMesh.vertices = verts.ToArray();
        visualMesh.uv = uvs.ToArray();
        visualMesh.triangles = tris.ToArray();
        visualMesh.RecalculateBounds();
        visualMesh.RecalculateNormals();

        meshFilter.mesh = visualMesh;
       meshCollider.sharedMesh = visualMesh;

    }


    public virtual void CreateFace(float brick, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris,int x,int z)
    {
        int index = verts.Count;

        verts.Add(corner);
        verts.Add(corner + up);
        verts.Add(corner + up + right);
        verts.Add(corner + right);

        uvs.Add(new Vector2((prevX), (prevZ)));
        uvs.Add(new Vector2((prevX), (float)(prevZ + ((float)1/width))));
        uvs.Add(new Vector2(((float)(prevX + ((float)1 / width))), (float)(prevZ + ((float)1 / width))));
        uvs.Add(new Vector2(((float)(prevX + ((float)1 / width))), (prevZ)));



        prevX = (x * ((float)1 / width));
        prevZ = (z * ((float)1 / width));

        if (reversed)
        {
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 0);
        }
        else
        {
            tris.Add(index + 1);
            tris.Add(index + 0);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 0);
        }

    }

    public virtual bool IsTransparent(int x, int y, int z, float[,,] map)
    {
        float brick = GetValue(x, y, z,map);
        switch (brick)
        {
            default:
            case 0:
                return true;

            case 1: return false;
        }
    }
    public virtual float GetValue(int x, int y, int z, float[,,] map)
    {
        if ((x < 0) || (y < 0) || (z < 0) || (y >= height) || (x >= width) || (z >= height))
            return 0;
        return map[x, y, z];
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}
