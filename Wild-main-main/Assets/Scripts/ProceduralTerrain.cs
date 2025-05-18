using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))] // MeshFilter�� MeshRenderer ������Ʈ�� �ʼ��� ����ؾ� ��
public class ProceduralTerrain : MonoBehaviour
{
    [Range(10, 200)]
    public int width = 100;  // ������ ���� ũ��
    [Range(10, 200)]
    public int height = 100;  // ������ ���� ũ��

    [Range(1f, 50f)]
    public float scale = 10f;  // ������ ���� ũ�� (������)
    [Range(1f, 20f)]
    public float heightMultiplier = 5f;  // ���� ���� ���� ����

    [Range(1, 8)]
    public int octaves = 4;  // �������� ��Ÿ�� ��
    [Range(0f, 1f)]
    public float persistence = 0.5f;  // ���Ӽ� (������ �󸶳� ������ �������� ����)
    [Range(1f, 5f)]
    public float lacunarity = 2f;  // ���ļ� ���� ����

    [Header("Height Ranges (0~1)")]
    [Range(0f, 1f)] public float level1 = 0.1f;  // ù ��° ���� ����
    [Range(0f, 1f)] public float level2 = 0.3f;  // �� ��° ���� ����
    [Range(0f, 1f)] public float level3 = 0.5f;  // �� ��° ���� ����
    [Range(0f, 1f)] public float level4 = 0.7f;  // �� ��° ���� ����
    [Range(0f, 1f)] public float level5 = 0.9f;  // �ټ� ��° ���� ����

    [Header("Materials for each level (1~5)")]
    public Material[] materials = new Material[5];  // 1~5�� ������ �ش��ϴ� ��Ƽ����� (0�� �ε����� ����)

    private MeshFilter meshFilter;  // MeshFilter ������Ʈ
    private MeshRenderer meshRenderer;  // MeshRenderer ������Ʈ

    // �޽� ������ �����ϴ� �Լ�
    public void GenerateMesh()
    {
        meshFilter = GetComponent<MeshFilter>();  // MeshFilter ������Ʈ�� ������
        meshRenderer = GetComponent<MeshRenderer>();  // MeshRenderer ������Ʈ�� ������

        // ������ ������ ���� �迭 (�ʺ� * ���� ��ŭ ����)
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];  // UV ��ǥ �迭

        // ���� ���� ������ 2D �迭
        float[,] heightMap = new float[width + 1, height + 1];
        float minHeight = float.MaxValue;  // �ּ� ���� �ʱ�ȭ
        float maxHeight = float.MinValue;  // �ִ� ���� �ʱ�ȭ

        // ������ ���� ���� ���
        for (int z = 0, i = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                float y = CalculateHeight(x, z);  // �� (x, z) ��ǥ�� �ش��ϴ� ���� ���
                vertices[i] = new Vector3(x, y, z);  // ���� ��ġ ����
                uvs[i] = new Vector2((float)x / width, (float)z / height);  // UV ��ǥ ���

                heightMap[x, z] = y;  // �ش� ��ġ�� ���� ����
                if (y < minHeight) minHeight = y;  // �ּ� ���� ����
                if (y > maxHeight) maxHeight = y;  // �ִ� ���� ����
            }
        }

        // �� ���� ������ �ش��ϴ� ����޽ø� ���� ����Ʈ �ʱ�ȭ
        List<int>[] submeshTriangles = new List<int>[materials.Length]; // ��Ƽ���� ������ŭ ����޽� ����
        for (int i = 0; i < materials.Length; i++) submeshTriangles[i] = new List<int>();

        // �� ����޽ÿ� �ﰢ�� �߰�
        for (int z = 0, vert = 0; z < height; z++, vert++)
        {
            for (int x = 0; x < width; x++, vert++)
            {
                int a = vert;
                int b = vert + width + 1;
                int c = vert + 1;
                int d = vert + width + 2;

                // �� �ﰢ���� �ش��ϴ� ����޽ÿ� �߰�
                AddTriangleToSubmesh(a, b, c, vertices, submeshTriangles, minHeight, maxHeight);
                AddTriangleToSubmesh(c, b, d, vertices, submeshTriangles, minHeight, maxHeight);
            }
        }

        // ���� �޽� ����
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.subMeshCount = materials.Length;  // ��Ƽ���� ������ŭ ����޽� ����

        // �� ����޽ÿ� �ﰢ�� ����
        for (int i = 0; i < materials.Length; i++)
        {
            mesh.SetTriangles(submeshTriangles[i], i);
        }

        mesh.RecalculateNormals();  // ��� ���
        meshFilter.sharedMesh = mesh;  // �޽� ����

        // ���� ��Ƽ���� �迭 ���� (��Ƽ������ ������ ���, ù ��° ��Ƽ������ �⺻������ ���)
        Material[] finalMats = new Material[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            finalMats[i] = materials[i] != null ? materials[i] : materials[0];  // null�� ��� ù ��° ��Ƽ����� ����
        }

        meshRenderer.sharedMaterials = finalMats;  // ���� ��Ƽ���� ����
    }

    // Perlin Noise�� ����Ͽ� ���� ���
    float CalculateHeight(int x, int z)
    {
        float amplitude = 1f;  // ����
        float frequency = 1f;  // ���ļ�
        float noiseHeight = 0f;  // ������ ����

        float xCoord = x / scale;  // x ��ǥ�� ����
        float zCoord = z / scale;  // z ��ǥ�� ����

        // ���� ��Ÿ���� Perlin Noise ���
        for (int i = 0; i < octaves; i++)
        {
            float perlin = Mathf.PerlinNoise(xCoord * frequency, zCoord * frequency);
            noiseHeight += perlin * amplitude;

            amplitude *= persistence;  // ���� ����
            frequency *= lacunarity;  // ���ļ� ����
        }

        return noiseHeight * heightMultiplier;  // ���� ���� ��ȯ
    }

    // �ﰢ���� �ش� ����޽ÿ� �߰��ϴ� �Լ�
    void AddTriangleToSubmesh(int a, int b, int c, Vector3[] verts, List<int>[] submeshTris, float minH, float maxH)
    {
        float avgY = (verts[a].y + verts[b].y + verts[c].y) / 3f;  // �ﰢ���� ��� ���� ���
        float t = Mathf.InverseLerp(minH, maxH, avgY);  // ��� ���̸� 0�� 1 ���̷� ����ȭ

        int index = 0;
        if (t < level1) index = 0;  // level1�� �ش��ϴ� ����
        else if (t < level2) index = 1;  // level2�� �ش��ϴ� ����
        else if (t < level3) index = 2;  // level3�� �ش��ϴ� ����
        else if (t < level4) index = 3;  // level4�� �ش��ϴ� ����
        else if (t < level5) index = 4;  // level5�� �ش��ϴ� ����
        else
        {
            // level5 �̻��� ���, ������ ���� (index 4)�� �߰��ϵ��� ����
            index = materials.Length - 1;
        }

        submeshTris[index].AddRange(new int[] { a, b, c });  // �ش� ����޽ÿ� �ﰢ�� �߰�
    }
}
