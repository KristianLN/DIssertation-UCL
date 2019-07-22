using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class AcademyScript_4 : Academy
{
  // General declarations
  private GameObject[] environments;
  [HideInInspector]
  public float extendX;
  [HideInInspector]
  public float extendZ;
  private int previousMovingSensors;
  private int previousSensors;
  private int previousClouds;
  private float previousRadius;
  Vector3 randomPosition;
  private string[] objectsOfInterest;
  // Declarations to addMovingSensors()
  public GameObject movingSensor;
  // Declarations to addSensorClouds()
  public GameObject sensor;
  public float heightOfMovingObjects = 3;
  public GameObject crowdedArea;
  float crowdedAreasToAdd = 0;
  // Declarations to enableWalls()
  private int choice;
  private int lB;
  private int uB;
  private int incrementer;
  List<int> processedObstacles = new List<int>();

  // Custom methods //
  void addMovingSensors(float xScale, float zScale, Vector3 translateVector, Transform parentTransform)
  {
    for (var x = 0; x < resetParameters["NumberOfMovingSensors"];x++)
    {
      randomPosition = new Vector3(Random.Range(1 - xScale,xScale - 1),
                             heightOfMovement(),
                             Random.Range(1 - zScale,zScale - 1));

      randomPosition = randomPosition + translateVector;

      var newMovingSensor = Instantiate(movingSensor,randomPosition,Quaternion.identity);
      newMovingSensor.transform.parent = parentTransform;
    }
  }
  void addSensorClouds(float xScale, float zScale, Vector3 translateVector, Transform parentTransform, float areasToAdd)
  {
    // Creating the clouds
    for (var i = 0; i < resetParameters["NumberOfSensorClouds"];i++)
    {
      // Creating the sounds within the cloud
      randomPosition = new Vector3(Random.Range(resetParameters["Radius"] - xScale,xScale - resetParameters["Radius"]),
                             heightOfMovement(),
                             Random.Range(resetParameters["Radius"] - zScale,zScale - resetParameters["Radius"]));

      randomPosition = randomPosition + translateVector;

      for (var x = 0; x < resetParameters["NumberOfSensors"];x++)
      {
        Vector3 sensorPosition = Random.insideUnitSphere * resetParameters["Radius"] + randomPosition;
        sensorPosition.y = heightOfMovingObjects;
        var newSensor = Instantiate(sensor,sensorPosition,Quaternion.identity);
        newSensor.transform.parent = parentTransform;
      }

    }

    if (areasToAdd != 0)
    {
      // float updatedNumberOfClouds = Mathf.Abs(resetParameters["NumberOfSensorClouds"] - previousClouds);

      for (var i = 0; i < areasToAdd;i++)
      {
        var newCrowdedArea = Instantiate(crowdedArea,randomPosition,Quaternion.identity);
        newCrowdedArea.transform.parent = parentTransform;
        newCrowdedArea.GetComponent<SphereCollider>().radius = resetParameters["Radius"];
        //newCrowdedArea.transform.localScale = new Vector3 (resetParameters["Radius"],resetParameters["Radius"],resetParameters["Radius"]);
      }
    }
  }
  void enableWalls(int lowerBound, int upperBound)
  {
    GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

    for (var i = 0; i < (int)resetParameters["ObstaclesToAdd"];i++)
    {
      bool drawANewNumber = true;
      while(drawANewNumber)
      {
        choice = Random.Range(lowerBound,upperBound);
        if (!processedObstacles.Contains(choice))
        {
          processedObstacles.Add(choice);
          drawANewNumber = false;
        }
      }
      // Enabling the obstacle
      obstacles[choice].GetComponent<MeshRenderer>().enabled = true;
      obstacles[choice].GetComponent<BoxCollider>().enabled = true;
    }
  }
  public float heightOfMovement()
  {
    return heightOfMovingObjects;
  }
  ///////////////////////////////////////////////////////////////////////////////////////////
  // Standard methods
  public override void InitializeAcademy()
  {
    environments = GameObject.FindGameObjectsWithTag("Ground");
    extendX = (environments[0].transform.localScale.x) / 2;// The first element in the environments array are used, because all environments are identical, and it therefore doesn't matter.
    extendZ = (environments[0].transform.localScale.z) / 2;

    previousMovingSensors = (int)resetParameters["NumberOfMovingSensors"];
    previousSensors = (int)resetParameters["NumberOfSensors"];
    previousClouds = (int)resetParameters["NumberOfSensorClouds"];
    previousRadius = (float)resetParameters["Radius"];

    // Disabling all obstacle at initialisation
    GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
    for (var i = 0; i < obstacles.Length;i++)
    {
      obstacles[i].GetComponent<MeshRenderer>().enabled = false;
      obstacles[i].GetComponent<BoxCollider>().enabled = false;
    }

    // Defining the incrementer to be used for setting sensors.
    incrementer = obstacles.Length / environments.Length;

    Debug.Log("----- Training session initialised -----");
    Debug.Log("Number of training areas: " + environments.Length);
    // Adjust manually
    Debug.Log("Type of agent: Two-Brain-Heuristic-Transition Agent");
  }

  public override void AcademyReset()
  {
    lB = 0;
    uB = incrementer;

    foreach (GameObject environment in environments)
    {
      int keepTrack = 0;
      foreach (Transform child in environment.transform)
      {
        if (child.tag == "CrowdedArea")
        {
          keepTrack += 1;
        }
      }
      // If the number of clouds going forward is less that the previous number, it is necessary to remove a "CrowdedArea" object
      if (resetParameters["NumberOfSensorClouds"]<keepTrack)
      {
        float crowdedAreasToDestroy = keepTrack - resetParameters["NumberOfSensorClouds"];
        // int keepTrack = 0;
        for (var i = 0; i < crowdedAreasToDestroy;i++)
        {
          Destroy(environment.transform.Find("CrowdedArea").gameObject);

        }
      } else if (resetParameters["NumberOfSensorClouds"]>keepTrack)
      {
        crowdedAreasToAdd = resetParameters["NumberOfSensorClouds"] - keepTrack;
      }
      // Time to add objects.
      addMovingSensors(extendX,extendZ,environment.transform.position,environment.transform);
      addSensorClouds(extendX,extendZ,environment.transform.position,environment.transform,crowdedAreasToAdd);
      enableWalls(lB,uB);
      lB += incrementer;
      uB += incrementer;
      crowdedAreasToAdd = 0;
    }
    // Status report
    // Debug.Log("----------- Update Status -----------");
    // Debug.Log("Number of:");
    // Debug.Log("- moving sensors: " + GameObject.FindGameObjectsWithTag("Pedestrian").Length);
    // Debug.Log("- sensors in the clouds: " + GameObject.FindGameObjectsWithTag("Sensor").Length);
    // Debug.Log("- obstacles: " + processedObstacles.Count);
  }
}
