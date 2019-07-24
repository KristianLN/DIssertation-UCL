using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
// From https://forum.unity.com/threads/solved-random-wander-ai-using-navmesh.327950/
public class WalkingAround_4_2 : MonoBehaviour
{
  public float wanderRadius;
  public float wanderTimer;
  private NavMeshAgent agent;
  private float timer;
  private float extendX;
  private float extendZ;
  private bool withinBounds;
  private Vector3 newPos;
  private float movementHeight;

  // Use this for initialization
  void Start () {
      agent = GetComponent<NavMeshAgent>();
      // Ensures a movement right from the beginning
      timer = wanderTimer;

      // Getting the bounds of the environment
      extendX = ((transform.parent.localScale.x) / 2) - 2;// Minus two to add some margin on the area where the NavMesh agent can walk
      extendZ = ((transform.parent.localScale.z) / 2) - 2;

      withinBounds = false;
      movementHeight = GameObject.FindWithTag("Academy").GetComponent<AcademyScript_4_2>().heightOfMovement();
  }

  // Update is called once per frame
  void Update ()
  {
      timer += Time.deltaTime;
      float x = transform.parent.parent.position.x;
      float z = transform.parent.parent.position.z;

      // wanderTimer is said publicly but could be said randomly within some bounded range.
      if (timer >= wanderTimer) {

        withinBounds = false;

        while(withinBounds==false)
        {
          newPos = RandomNavSphere(transform.position, wanderRadius, -1);
          // Checking if the x and z coordinates are within the environment.
          if ((newPos.x < x + extendX && newPos.x > x - extendX) && (newPos.z < z + extendZ && newPos.z > z - extendZ))
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
