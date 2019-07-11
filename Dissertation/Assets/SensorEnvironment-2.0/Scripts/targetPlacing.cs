using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class targetPlacing : MonoBehaviour
{
    public float numberOfAngles = 8;
    private float angle = 0;
    private float x;
    private float z;
    public float range = 3;
    private float extendX;
    private float extendZ;
    Vector3 randomPosition;
    private GameObject environment;
    private GameObject goalObject;
    private bool didWeHit = false;
    private bool noHit = false;
    private string[] tagsOfInterst = new string[3];

    public bool checkForCollision()
    {
      angle = 0;
      noHit = false;
      for(var i = 0; i < numberOfAngles;i++)
      {
        RaycastHit hit;

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

    public void inCaseOfCollision()
    {
      goalObject = GameObject.FindGameObjectsWithTag("Goal")[0];
      environment = GameObject.FindWithTag("Ground");

      extendX = (environment.transform.localScale.x) / 2;
      extendZ = (environment.transform.localScale.z) / 2;

      randomPosition = new Vector3(Random.Range(goalObject.transform.localScale.x - extendX,extendX - goalObject.transform.localScale.x),
                             3,
                             Random.Range(goalObject.transform.localScale.z - extendZ,extendZ - goalObject.transform.localScale.z));

      goalObject.transform.position = randomPosition;
    }

    public void Start()
    {
      // Defining the tags of interest
      tagsOfInterst[0] = "Obstacle";
      tagsOfInterst[1] = "Wall";
      tagsOfInterst[2] = "Agent";

      goalObject = GameObject.FindGameObjectsWithTag("Goal")[0];
      environment = GameObject.FindWithTag("Ground");

      if (checkForCollision())
      {
        inCaseOfCollision();
      }
    }

    public void OnCollisionEnter(Collision collision)
    {
      if (tagsOfInterst.Contains(collision.collider.tag))
      {
        didWeHit = true;

        inCaseOfCollision();
      }
    }

    public void OnTriggerStay(Collider collision)
    {
      if (tagsOfInterst.Contains(collision.tag))
      {
        didWeHit = true;

        inCaseOfCollision();
      }
    }

    public void Update()
    {
      if (checkForCollision())
      {
        inCaseOfCollision();
      }
    }
}
