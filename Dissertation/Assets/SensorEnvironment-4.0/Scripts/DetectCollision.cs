using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MLAgents;
using System.IO;
using System.Text;

public class DetectCollision : MonoBehaviour
{
  // Initialising variables
  private int countingSessions = 0; // Needs to be imported from the academy
  private int sensorCollisionsGlobal = 0;
  private int sensorCollisionsLocal = 0;
  int difficult;
  private float movementHeight;
  // Exporting draw
  public string[] nameOfFile;
  private string path = "Assets/Exported_Data/";
  private string content;
  // Reset target
  GameObject parentObject;
  GameObject firstChild;
  AcademyScript_4 academyScript;
  // Multiple brains
  Brain secondBrain;
  Brain firstBrain;
  Agent agent;
  bool resetLocations = true;
    // Collision detection to swich between brains
  float angle = 0;
  bool hitOccured = false;
  int numberOfAngles = 8;
  float range = 5;
  float x,z;
  int layerMask;
  //float z;

  //////////////////////////// Custom functions ////////////////////////////
  public void exportData(string pathToFile, string contentToWrite)
  {
    File.AppendAllText(pathToFile,contentToWrite);
  }
  public int DifficultArea(Vector3 targetPosition)
  {
    float absX = Mathf.Abs(targetPosition.x);
    float absZ = Mathf.Abs(targetPosition.z);
    if ((absZ > 30f && absZ < 40f) && (absX > 0f && absX < 15f))
    {
      difficult = 1;
    } else
    {
      difficult = 0;
    }
    return difficult;
  }
  public bool checkForCollision()
  {
    layerMask = 1 << 8;
    angle = 0;
    hitOccured = false;
    for(var i = 0; i < numberOfAngles;i++)
    {
      RaycastHit hit;

      x = transform.position.x + range*Mathf.Cos(angle);
      z = transform.position.z + range*Mathf.Sin(angle);

      Vector3 dir = new Vector3(x,movementHeight,z);
      // Debug.DrawLine(transform.position,dir,Color.red,1.0f);
      if (Physics.Raycast(transform.position,dir, out hit, range,layerMask))
      {
        hitOccured = true;
        //Debug.Log("HIT!");
      }

      angle += 2*Mathf.PI / numberOfAngles;
    }
    return hitOccured;
  }
  //////////////////////////////////////////////////////////////////////////
  //////////////////////////// Build-in functions ////////////////////////////
  public void Start()
  {
    // Initialize something
    parentObject = this.transform.parent.gameObject;
    firstChild = parentObject.transform.GetChild(0).gameObject;
    academyScript = GameObject.FindWithTag("Academy").GetComponent<AcademyScript_4>();
    agent = this.GetComponent<Agent>();
    firstBrain = this.GetComponent<AgentScript_4_clean>().firstBrain;
    secondBrain = this.GetComponent<AgentScript_4_clean>().secondBrain;
  }
  public void OnCollisionEnter(Collision collision)
  {
    if (collision.collider.CompareTag("Wall"))
    {
      agent.AddReward(-1f);
      // Tracking collision with pedestrians.
      exportData(path+nameOfFile[1],"0\n");
      // Tracking collisions with sensors
      exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisionsGlobal+", 0\n");
      // Getting the steps
      exportData(path+nameOfFile[3],"0"+", "+agent.GetStepCount()+", "+DifficultArea(firstChild.transform.position)+"\n");
      agent.Done();
    }

    if ( collision.collider.CompareTag("Obstacle"))
    {
      agent.AddReward(-1f);
      // Tracking collision with pedestrians.
      exportData(path+nameOfFile[1],"0\n");
      // Tracking collisions with sensors
      exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisionsGlobal+", 0\n");
      // Getting the steps
      exportData(path+nameOfFile[3],"0"+", "+agent.GetStepCount()+", "+DifficultArea(firstChild.transform.position)+"\n");
      agent.Done();
    }

    if ( collision.collider.CompareTag("Pedestrian"))
    {
      agent.AddReward(-1f);
      // Tracking collision with pedestrians.
      exportData(path+nameOfFile[1],"1\n");
      // Tracking collisions with sensors
      exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisionsGlobal+", 0\n");
      // Getting the steps
      exportData(path+nameOfFile[3],"0"+", "+agent.GetStepCount()+", "+DifficultArea(firstChild.transform.position)+"\n");
      agent.Done();
    }

    if (collision.collider.CompareTag("Goal"))
    {
      agent.AddReward(1f);//5f
      // Tracking collision with pedestrians.
      exportData(path+nameOfFile[1],"0\n");
      // Tracking collisions with sensors
      exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisionsGlobal+", 1\n");
      // Getting the steps
      exportData(path+nameOfFile[3],"1"+", "+agent.GetStepCount()+", "+DifficultArea(firstChild.transform.position)+"\n");
      agent.Done();

    }
  }
  private void OnTriggerEnter(Collider collider)
  {
    if (collider.CompareTag("Sensor"))
    {
      agent.AddReward(-0.001f);
      sensorCollisionsGlobal += 1;
      sensorCollisionsLocal += 1;
      exportData(path+nameOfFile[2],countingSessions+", 1\n");
    } else if (collider.CompareTag("CrowdedArea"))
    {
      sensorCollisionsLocal = 0;
    }
  }
  private void OnTriggerExit(Collider collider)
  {
    if (collider.CompareTag("CrowdedArea"))
    {
      if (academyScript.resetParameters["NoiseProb"] > Random.Range(0.0f,1.0f))
      {
        agent.AddReward(-0.001f * (sensorCollisionsLocal + Random.Range(0,sensorCollisionsLocal)));
      } else
      {
        agent.AddReward(-0.001f * sensorCollisionsLocal);
      }
    }
  }
  public void FixedUpdate()
  {
    if (secondBrain != null)
    {
      if (checkForCollision())
      {
        agent.GiveBrain(secondBrain);
        // No desire to reset locations because of the change in brain.
        resetLocations = false;
        agent.AgentReset();
        // Yet, we need the resetting of locations to potentially happen after.
        resetLocations = true;
      } else
      {
        // The default brain
        agent.GiveBrain(firstBrain);
        // No desire to reset locations because of the change in brain.
        resetLocations = false;
        agent.AgentReset();
        // Yet, we need the resetting of locations to potentially happen after.
        resetLocations = true;
      }
    }
  }
}
