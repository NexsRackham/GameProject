using UnityEngine;

/// <summary>
/// Genera una malla plana subdividida en tiempo de edición o ejecución.
/// Ideal para sistemas de flotación precisos.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    [Header("Tamaño del plano")]
    public int width = 20;
    public int length = 20;

    [Header("Resolución (subdivisiones por unidad)")]
    [Range(1, 10)] public int resolution = 1;

    public void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        int xVerts = width * resolution + 1;
        int zVerts = length * resolution + 1;

        Vector3[] vertices = new Vector3[xVerts * zVerts];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(xVerts - 1) * (zVerts - 1) * 6];

        // Crear vértices y uvs
        for (int z = 0; z < zVerts; z++)
        {
            for (int x = 0; x < xVerts; x++)
            {
                int i = z * xVerts + x;
                vertices[i] = new Vector3(x / (float)resolution, 0, z / (float)resolution);
                uvs[i] = new Vector2(x / (float)(xVerts - 1), z / (float)(zVerts - 1));
            }
        }

        // Crear triángulos
        int ti = 0;
        for (int z = 0; z < zVerts - 1; z++)
        {
            for (int x = 0; x < xVerts - 1; x++)
            {
                int i = z * xVerts + x;
                triangles[ti++] = i;
                triangles[ti++] = i + xVerts;
                triangles[ti++] = i + 1;

                triangles[ti++] = i + 1;
                triangles[ti++] = i + xVerts;
                triangles[ti++] = i + xVerts + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void Start()
    {
        GenerateMesh();
    }
}