using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class AcademyScript_4_2 : Academy
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
  int areaType;
  GameObject[] trainingAreas;
  // Declarations to addMovingSensors()
  public GameObject movingSensor;
  // Declarations to addSensorClouds()
  public GameObject sensor;
  public float heightOfMovingObjects = 3;
  public GameObject crowdedArea;
  float crowdedAreasToAdd = 0;
  float[] densities;
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
    }

    if (areasToAdd != 0)
    {
      for (var i = 0; i < areasToAdd;i++)
      {
        areaType = Random.Range(0,densities.Length);
        var newCrowdedArea = Instantiate(crowdedArea,randomPosition,Quaternion.identity);
        newCrowdedArea.transform.parent = parentTransform;
        // Explanation on what's going on below:
        // In order to correcly set the size of the crowded area, and the belonging collider, it is necessary to alter the scale of the object.
        // The scale of a child (the object) follows the scale of its parent (the environment), which is why the scale of the object is set as fractions
        // of the scale of the environment. The scale is stored as the distance from 0 to the end of the enviroment, in both positive and negative direction,
        // implying that the scale is half of the actual size of the environment. That's why the stored scales are multiplied by 2 below.
        newCrowdedArea.transform.localScale = new Vector3(resetParameters["Radius"] / (xScale*2f),
                                                          resetParameters["Radius"],
                                                          resetParameters["Radius"] / (zScale*2f));
        //newCrowdedArea.GetComponent<SphereCollider>().radius = resetParameters["Radius"];
        // For visualisation

        newCrowdedArea.GetComponent<AreaInfo>().density = densities[areaType];
        newCrowdedArea.GetComponent<Renderer>().material = newCrowdedArea.GetComponent<AreaInfo>().materials[areaType];
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
    // Enabling training environments
    trainingAreas = GameObject.FindGameObjectsWithTag("Area");
    for (var i = 0; i < (trainingAreas.Length - (int)resetParameters["NumberOfAreas"]);i++)
    {
      trainingAreas[i].SetActive(false);
    }

    densities = GameObject.FindWithTag("Agent").GetComponent<AgentScript_4_2>().densities;
    environments = GameObject.FindGameObjectsWithTag("Ground");

    previousMovingSensors = (int)resetParameters["NumberOfMovingSensors"];
    // previousSensors = (int)resetParameters["NumberOfSensors"];
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
    Debug.Log("Type of agent: Single Agent");
  }

  public override void AcademyReset()
  {
    lB = 0;
    uB = incrementer;

    environments = GameObject.FindGameObjectsWithTag("Ground");
    extendX = (environments[0].transform.localScale.x) / 2;// The first element in the environments array are used, because all environments are identical, and it therefore doesn't matter.
    extendZ = (environments[0].transform.localScale.z) / 2;
    // Ensuring that each environments has the correct specifications.
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
