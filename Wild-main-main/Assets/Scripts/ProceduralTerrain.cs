using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))] // MeshFilter와 MeshRenderer 컴포넌트를 필수로 사용해야 함
public class ProceduralTerrain : MonoBehaviour
{
    [Range(10, 200)]
    public int width = 100;  // 지형의 가로 크기
    [Range(10, 200)]
    public int height = 100;  // 지형의 세로 크기

    [Range(1f, 50f)]
    public float scale = 10f;  // 노이즈 맵의 크기 (스케일)
    [Range(1f, 20f)]
    public float heightMultiplier = 5f;  // 높이 값에 곱할 배율

    [Range(1, 8)]
    public int octaves = 4;  // 노이즈의 옥타브 수
    [Range(0f, 1f)]
    public float persistence = 0.5f;  // 지속성 (진폭이 얼마나 빠르게 감소할지 결정)
    [Range(1f, 5f)]
    public float lacunarity = 2f;  // 주파수 증가 비율

    [Header("Height Ranges (0~1)")]
    [Range(0f, 1f)] public float level1 = 0.1f;  // 첫 번째 높이 범위
    [Range(0f, 1f)] public float level2 = 0.3f;  // 두 번째 높이 범위
    [Range(0f, 1f)] public float level3 = 0.5f;  // 세 번째 높이 범위
    [Range(0f, 1f)] public float level4 = 0.7f;  // 네 번째 높이 범위
    [Range(0f, 1f)] public float level5 = 0.9f;  // 다섯 번째 높이 범위

    [Header("Materials for each level (1~5)")]
    public Material[] materials = new Material[5];  // 1~5번 레벨에 해당하는 머티리얼들 (0번 인덱스는 제거)

    private MeshFilter meshFilter;  // MeshFilter 컴포넌트
    private MeshRenderer meshRenderer;  // MeshRenderer 컴포넌트

    // 메쉬 생성을 시작하는 함수
    public void GenerateMesh()
    {
        meshFilter = GetComponent<MeshFilter>();  // MeshFilter 컴포넌트를 가져옴
        meshRenderer = GetComponent<MeshRenderer>();  // MeshRenderer 컴포넌트를 가져옴

        // 지형을 구성할 정점 배열 (너비 * 높이 만큼 생성)
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];  // UV 좌표 배열

        // 높이 맵을 저장할 2D 배열
        float[,] heightMap = new float[width + 1, height + 1];
        float minHeight = float.MaxValue;  // 최소 높이 초기화
        float maxHeight = float.MinValue;  // 최대 높이 초기화

        // 정점과 높이 맵을 계산
        for (int z = 0, i = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                float y = CalculateHeight(x, z);  // 각 (x, z) 좌표에 해당하는 높이 계산
                vertices[i] = new Vector3(x, y, z);  // 정점 위치 설정
                uvs[i] = new Vector2((float)x / width, (float)z / height);  // UV 좌표 계산

                heightMap[x, z] = y;  // 해당 위치의 높이 저장
                if (y < minHeight) minHeight = y;  // 최소 높이 갱신
                if (y > maxHeight) maxHeight = y;  // 최대 높이 갱신
            }
        }

        // 각 높이 레벨에 해당하는 서브메시를 위한 리스트 초기화
        List<int>[] submeshTriangles = new List<int>[materials.Length]; // 머티리얼 개수만큼 서브메시 생성
        for (int i = 0; i < materials.Length; i++) submeshTriangles[i] = new List<int>();

        // 각 서브메시에 삼각형 추가
        for (int z = 0, vert = 0; z < height; z++, vert++)
        {
            for (int x = 0; x < width; x++, vert++)
            {
                int a = vert;
                int b = vert + width + 1;
                int c = vert + 1;
                int d = vert + width + 2;

                // 각 삼각형을 해당하는 서브메시에 추가
                AddTriangleToSubmesh(a, b, c, vertices, submeshTriangles, minHeight, maxHeight);
                AddTriangleToSubmesh(c, b, d, vertices, submeshTriangles, minHeight, maxHeight);
            }
        }

        // 최종 메쉬 생성
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.subMeshCount = materials.Length;  // 머티리얼 개수만큼 서브메시 설정

        // 각 서브메시에 삼각형 설정
        for (int i = 0; i < materials.Length; i++)
        {
            mesh.SetTriangles(submeshTriangles[i], i);
        }

        mesh.RecalculateNormals();  // 노멀 계산
        meshFilter.sharedMesh = mesh;  // 메쉬 설정

        // 최종 머티리얼 배열 설정 (머티리얼이 부족할 경우, 첫 번째 머티리얼을 기본값으로 사용)
        Material[] finalMats = new Material[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            finalMats[i] = materials[i] != null ? materials[i] : materials[0];  // null인 경우 첫 번째 머티리얼로 설정
        }

        meshRenderer.sharedMaterials = finalMats;  // 최종 머티리얼 적용
    }

    // Perlin Noise를 사용하여 높이 계산
    float CalculateHeight(int x, int z)
    {
        float amplitude = 1f;  // 진폭
        float frequency = 1f;  // 주파수
        float noiseHeight = 0f;  // 노이즈 높이

        float xCoord = x / scale;  // x 좌표의 비율
        float zCoord = z / scale;  // z 좌표의 비율

        // 여러 옥타브의 Perlin Noise 계산
        for (int i = 0; i < octaves; i++)
        {
            float perlin = Mathf.PerlinNoise(xCoord * frequency, zCoord * frequency);
            noiseHeight += perlin * amplitude;

            amplitude *= persistence;  // 진폭 감소
            frequency *= lacunarity;  // 주파수 증가
        }

        return noiseHeight * heightMultiplier;  // 최종 높이 반환
    }

    // 삼각형을 해당 서브메시에 추가하는 함수
    void AddTriangleToSubmesh(int a, int b, int c, Vector3[] verts, List<int>[] submeshTris, float minH, float maxH)
    {
        float avgY = (verts[a].y + verts[b].y + verts[c].y) / 3f;  // 삼각형의 평균 높이 계산
        float t = Mathf.InverseLerp(minH, maxH, avgY);  // 평균 높이를 0과 1 사이로 정규화

        int index = 0;
        if (t < level1) index = 0;  // level1에 해당하는 높이
        else if (t < level2) index = 1;  // level2에 해당하는 높이
        else if (t < level3) index = 2;  // level3에 해당하는 높이
        else if (t < level4) index = 3;  // level4에 해당하는 높이
        else if (t < level5) index = 4;  // level5에 해당하는 높이
        else
        {
            // level5 이상일 경우, 마지막 레벨 (index 4)에 추가하도록 수정
            index = materials.Length - 1;
        }

        submeshTris[index].AddRange(new int[] { a, b, c });  // 해당 서브메시에 삼각형 추가
    }
}
