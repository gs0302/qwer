using UnityEngine;

public class EntityState : MonoBehaviour
{
  private enum State
  {
    Die,
    Alive,
    Attack,
    Jump,
    Run,
    Walk,

  }
}
