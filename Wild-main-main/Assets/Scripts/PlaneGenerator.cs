using UnityEngine;

public class PlaneGenerator : MonoBehaviour
{
  public int width = 10; // Width of the plane
  public int height = 10; // Height of the plane

  [Range(0.01f, 1f)]
  public float scale = 1f; // Scale of the plane
  public GameObject planePrefab; // Prefab for the plane
  public GameObject level;
  void Start()
  {
    for (int x = 0; x < width; x++)
    {
      for (int z = 0; z < height; z++)
      {
        GameObject plane = Instantiate(planePrefab, new Vector3(x - (width / 2), 0, z - (height / 2)) * (scale * 10), Quaternion.identity);

        plane.transform.localScale = new Vector3(scale, 1, scale);

        plane.transform.SetParent(level.transform); // Set the parent to the level object
      }
    }
  }
}
