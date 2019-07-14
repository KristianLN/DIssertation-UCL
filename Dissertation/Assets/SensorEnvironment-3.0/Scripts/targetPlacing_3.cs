using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MLAgents;
using System.IO;
using System.Text;

public class targetPlacing_3 : MonoBehaviour
{
    public float numberOfAngles = 8;
    private float angle = 0;
    private float x;
    private float z;
    public float range = 3;
    private float extendX;
    private float extendZ;
    Vector3 randomPosition;
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

        x = transform.position.x + range*Mathf.Cos(angle);
        z = transform.position.z + range*Mathf.Sin(angle);

        Vector3 dir = new Vector3(x,3,z);

        if (Physics.Raycast(transform.position,dir, out hit))
        {


          if (hit.distance <= range)
          {
            noHit = true;
            Debug.DrawLine(transform.position,dir,Color.red,10.0f);
          }
        }

        angle += 2*Mathf.PI / numberOfAngles;
      }
      return noHit;
    }

    public void inCaseOfCollision()
    {
      extendX = (transform.parent.localScale.x) / 2;
      extendZ = (transform.parent.localScale.z) / 2;

      randomPosition = new Vector3(Random.Range(transform.localScale.x - extendX,extendX - transform.localScale.x),
                             3,
                             Random.Range(transform.localScale.z - extendZ,extendZ - transform.localScale.z));

      transform.position = randomPosition + this.transform.parent.parent.position;
      transform.rotation = Quaternion.identity;
    }

    public void Start()
    {
      // Defining the tags of interest
      tagsOfInterst[0] = "Obstacle";
      tagsOfInterst[1] = "Wall";
      tagsOfInterst[2] = "Agent";

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
    public void resetTarget()
    {
      inCaseOfCollision();

      if(checkForCollision())
      {
        inCaseOfCollision();
      }
    }
}
