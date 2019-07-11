using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
// From https://forum.unity.com/threads/solved-random-wander-ai-using-navmesh.327950/
public class WalkingAround_3 : MonoBehaviour
{
  public float wanderRadius;
  public float wanderTimer;

  private Transform target;
  private NavMeshAgent agent;
  private float timer;
  private float extendX;
  private float extendZ;
  private GameObject environment;
  private bool withinBounds;
  private Collider environmentCollider;
  private Vector3 newPos;
  private float movementHeight;

  // Use this for initialization
  void OnEnable () {
      agent = GetComponent<NavMeshAgent>();
      // Ensures a movement right from the beginning
      timer = wanderTimer;

      // Getting the bounds of the environment
      environment = GameObject.FindWithTag("Ground");
      environmentCollider = environment.GetComponent<Collider>();
      extendX = ((environment.transform.localScale.x) / 2) - 2;// Minus two to add some margin on the area where the NavMesh agent can walk
      extendZ = ((environment.transform.localScale.z) / 2) - 2;
      withinBounds = false;
      movementHeight = environment.GetComponent<ShowSensors_dynamically_3>().heightOfMovement();
  }

  // Update is called once per frame
  void Update () {
      timer += Time.deltaTime;
      // wanderTimer is said publicly but could be said randomly within some bounded range.
      if (timer >= wanderTimer) {
        withinBounds = false;

        while(withinBounds==false)
        {
          newPos = RandomNavSphere(transform.position, wanderRadius, -1);
          // Checking if the x and z coordinates are within the environment.
          if ((newPos.x < extendX && newPos.x > -extendX) && (newPos.x < extendZ && newPos.z > -extendZ))//(environmentCollider.bounds.Contains(newPos))//((environmentCollider.bounds.Contains(newPos.x)) && (environmentCollider.bounds.Contains(newPos.z)))//((newPos.x < extendX || newPos.x > -extendX) && (newPos.x < extendZ || newPos.z > -extendZ))
          {
            withinBounds = true;
            newPos = new Vector3 (newPos.x,movementHeight,newPos.z);
          }
        }
        agent.SetDestination(newPos);
        timer = 0;
      }
  }

  public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) {
      Vector3 randDirection = Random.insideUnitSphere * dist;

      randDirection += origin;

      NavMeshHit navHit;

      NavMesh.SamplePosition (randDirection, out navHit, dist, layermask);

      return navHit.position;
  }
}
