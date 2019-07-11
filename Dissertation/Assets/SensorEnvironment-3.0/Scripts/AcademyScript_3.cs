using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class AcademyScript_3 : Academy
{
  // General declarations
  private GameObject environment;
  [HideInInspector]
  public float extendX;
  [HideInInspector]
  public float extendZ;
  private int previousMovingSensors;
  private int previousSensors;
  private int previousClouds;
  private float previousRadius;
  Vector3 randomPosition;
  public string[] objectsOfInterest;
  // Declarations to addMovingSensors()
  public GameObject movingSensor;
  // Declarations to addSensorClouds()
  public GameObject sensor;
  //public float radius = 5;
  public float heightOfMovingObjects = 3;

  // Custom methods //

  void addMovingSensors(float xScale, float zScale)
  {
    for (int x = 0; x < (int)resetParameters["NumberOfMovingSensors"];x++)
    {
      randomPosition = new Vector3(Random.Range(1 - xScale,xScale - 1),
                             heightOfMovement(),
                             Random.Range(1 - zScale,zScale - 1));

      Instantiate(movingSensor,randomPosition,Quaternion.identity);
    }
  }

  void addSensorClouds(float xScale, float zScale)
  {
    // Creating the clouds
    for (int i = 0; i < (int)resetParameters["NumberOfSensorClouds"];i++)
    {
      // Creating the sounds within the cloud
      randomPosition = new Vector3(Random.Range(resetParameters["Radius"] - xScale,xScale - resetParameters["Radius"]),
                             heightOfMovement(),
                             Random.Range(resetParameters["Radius"] - zScale,zScale - resetParameters["Radius"]));

      for (int x = 0; x < (int)resetParameters["NumberOfSensors"];x++)
      {
        Vector3 sensorPosition = Random.insideUnitSphere * resetParameters["Radius"] + randomPosition;
        sensorPosition.y = heightOfMovingObjects;//1.5f
        Instantiate(sensor,sensorPosition,Quaternion.identity);
      }
    }
  }

  public float heightOfMovement()
  {
    return heightOfMovingObjects;
  }

  public override void InitializeAcademy()
  {
    // var parameters = GetComponent<AcademyScript_3>().resetParameters;
    // Debug.Log(parameters[0]);
    environment = GameObject.FindWithTag("Ground");
    extendX = (environment.transform.localScale.x) / 2;
    extendZ = (environment.transform.localScale.z) / 2;

    addMovingSensors(extendX,extendZ);
    addSensorClouds(extendX,extendZ);
    previousMovingSensors = (int)resetParameters["NumberOfMovingSensors"];
    previousSensors = (int)resetParameters["NumberOfSensors"];
    previousClouds = (int)resetParameters["NumberOfSensorClouds"];
    previousRadius = resetParameters["Radius"];
  }

  public override void AcademyReset()
  {
    addMovingSensors(extendX,extendZ);
    addSensorClouds(extendX,extendZ);
  }

  public void Update()
  {
    if ((previousMovingSensors != (int)resetParameters["NumberOfMovingSensors"]) ||
     (previousSensors != (int)resetParameters["NumberOfSensors"]) ||
      (previousClouds != (int)resetParameters["NumberOfSensorClouds"]) ||
       (previousRadius != (int)resetParameters["Radius"]))
    {
      string[] objectsOfInterest = {"Sensor","Pedestrian"};

      foreach (string objectOfInterst in objectsOfInterest)
      {
        GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag(objectOfInterst);
        foreach(GameObject removeObject in objectsToDestroy)
        {
          Destroy(removeObject);
        }
      }

      Done();

      previousMovingSensors = (int)resetParameters["NumberOfMovingSensors"];
      previousSensors = (int)resetParameters["NumberOfSensors"];
      previousClouds = (int)resetParameters["NumberOfSensorClouds"];
      previousRadius = resetParameters["Radius"];
    }
  }

}
