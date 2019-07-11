using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetPlacing_Old : MonoBehaviour
{
    public float numberOfAngles = 8;
    private float angle = 0;
    private float x;
    private float z;
    public float range = 3;
    private float extendX;
    private float extendZ;
    Vector3 randomPosition;
    // public GameObject environment;
    private GameObject environment;
    private bool didWeHit = false;
    private bool noHit = false;
    private GameObject goalObject;


    public bool checkForCollision()
    {
      angle = 0;
      noHit = false;
      for(var i = 0; i < numberOfAngles;i++)
      {
        RaycastHit hit;
        //Debug.Log(goalObject.transform.position.x);
        x = goalObject.transform.position.x + range*Mathf.Cos(angle);
        z = goalObject.transform.position.z + range*Mathf.Sin(angle);

        Vector3 dir = new Vector3(x,3,z);

        if (Physics.Raycast(goalObject.transform.position,dir, out hit))
        {
          Debug.DrawLine(goalObject.transform.position,dir,Color.red,10.0f);

          if (hit.distance <= range)
          {
            noHit = true;
          }
        } else
        {
          Debug.DrawLine(goalObject.transform.position,dir,Color.red,10.0f);
        }
        angle += 2*Mathf.PI / numberOfAngles;
      }
      return noHit;
    }

    public void checkForCollision2()
    {
      goalObject = GameObject.FindGameObjectsWithTag("Goal")[0];
      environment = GameObject.FindWithTag("Ground");
      Debug.Log("It works!");
      // while (didWeHit){
      //
      //   if (didWeHit)
      //   {
      //     Debug.Log("here again");
      //
      // Get new position
      extendX = (environment.transform.localScale.x) / 2;
      extendZ = (environment.transform.localScale.z) / 2;

      randomPosition = new Vector3(Random.Range(goalObject.transform.localScale.x - extendX,extendX - goalObject.transform.localScale.x),
                             3,
                             Random.Range(goalObject.transform.localScale.z - extendZ,extendZ - goalObject.transform.localScale.z));

      goalObject.transform.position = randomPosition;
      //     //didWeHit = checkForCollision();
      //   }
      // }
    }
    // everything in start as it is now, was the initial implementation.
    public void Start()
    {
      goalObject = GameObject.FindGameObjectsWithTag("Goal")[0];
      environment = GameObject.FindWithTag("Ground");
      //Debug.Log(environment.transform.localScale.x);
      // // If no number of angles is set by the user
      // if (numberOfAngles==0){
      //   numberOfAngles = 30.0f;
      // }
      //
      // // If no range is set by the user
      // if (range == 0)
      // {
      //   range = 10.0f;
      // }
      didWeHit = checkForCollision();
      //Debug.Log(didWeHit);


      checkForCollision2();
      // while (didWeHit){
      //   //Debug.Log("Here in!");
      //
      //   // didWeHit = checkForCollision();
      //   //Debug.Log(checkForCollision());
      //
      //   if (didWeHit)
      //   {
      //     Debug.Log("here again");
      //
      //     // Get new position
      //     // extendX = (environment.transform.localScale.x) / 2;
      //     // extendZ = (environment.transform.localScale.z) / 2;
      //     extendX = (environment.transform.localScale.x) / 2;
      //     extendZ = (environment.transform.localScale.z) / 2;
      //
      //     randomPosition = new Vector3(Random.Range(goalObject.transform.localScale.x - extendX,extendX - goalObject.transform.localScale.x),
      //                            3,
      //                            Random.Range(goalObject.transform.localScale.z - extendZ,extendZ - goalObject.transform.localScale.z));
      //
      //     goalObject.transform.position = randomPosition;
      //     Debug.Log(randomPosition);
      //     Debug.Log(goalObject.transform.position);
      //     didWeHit = checkForCollision();
      //
      //   }
      // }
    }

    private void OnCollisionEnter(Collision collision)
    {
      goalObject = GameObject.FindGameObjectsWithTag("Goal")[0];
      environment = GameObject.FindWithTag("Ground");

      if ( collision.collider.CompareTag("Agent"))
      {
        didWeHit = true;

        // If collision occur
        checkForCollision2();
        // while (didWeHit){
        //   Debug.Log("Here in!");
        //
        //   // didWeHit = checkForCollision();
        //   //Debug.Log(checkForCollision());
        //
        //   if (didWeHit)
        //   {
        //     Debug.Log("here again");
        //
        //     // Get new position
        //     // extendX = (environment.transform.localScale.x) / 2;
        //     // extendZ = (environment.transform.localScale.z) / 2;
        //     extendX = (environment.transform.localScale.x) / 2;
        //     extendZ = (environment.transform.localScale.z) / 2;
        //
        //     randomPosition = new Vector3(Random.Range(goalObject.transform.localScale.x - extendX,extendX - goalObject.transform.localScale.x),
        //                            3,
        //                            Random.Range(goalObject.transform.localScale.z - extendZ,extendZ - goalObject.transform.localScale.z));
        //
        //     goalObject.transform.position = randomPosition;
        //     Debug.Log(randomPosition);
        //     Debug.Log(goalObject.transform.position);
        //     didWeHit = checkForCollision();
        //
        //   }
        // }
        // AddReward(-1f);
        // Done();
      }
    }
}
