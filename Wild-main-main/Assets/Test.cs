using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour
{
  GameObject cube;
  MeshFilter meshFilter;
  Mesh mesh;
  public GameObject prefab;

  Color[] colors = new Color[8] {    Color.red, Color.green, Color.blue, Color.yellow,
    Color.cyan, Color.magenta, Color.white, Color.black
  };
  void Start()
  {
    cube = GameObject.Find("Cube");
    meshFilter = cube.GetComponent<MeshFilter>();
    mesh = meshFilter.mesh;
    Vector3[] vertices = mesh.vertices.Distinct().ToArray();


    for (int i = 0; i < vertices.Length; i++)
    {
      Vector3 vertex = vertices[i];
      Debug.Log(vertex);
      GameObject point = Instantiate(prefab, vertex, Quaternion.identity);
      point.GetComponent<MeshRenderer>().material.color = colors[i];
    }
  }
  void Update()
  {

  }
}
