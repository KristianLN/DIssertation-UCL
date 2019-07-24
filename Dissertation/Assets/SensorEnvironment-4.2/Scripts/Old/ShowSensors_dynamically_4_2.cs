using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ShowSensors_dynamically_4_2 : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject sensor;
    public int numberOfSensors = 10;
    public float radius = 5;
    public int NumberofSensorClouds = 1;
    public float heightOfMovingObjects = 3;
    private float extendX;
    private float extendZ;
    // private bool found = false;
    Vector3 randomPosition;

    public float heightOfMovement()
    {
      return heightOfMovingObjects;
    }

    public void Start()
    {
      extendX = (transform.localScale.x) / 2;
      extendZ = (transform.localScale.z) / 2;

      // Creating the clouds
      for (int i = 0; i < NumberofSensorClouds;i++)
      {
        // Creating the sounds within the cloud
        randomPosition = new Vector3(Random.Range(radius - extendX,extendX - radius),
                               transform.localScale.y,
                               Random.Range(radius - extendZ,extendZ - radius));

        for (int x = 0; x < numberOfSensors;x++)
        {
          Vector3 sensorPosition = Random.insideUnitSphere * radius + randomPosition;
          sensorPosition.y = heightOfMovingObjects;//1.5f
          Instantiate(sensor,sensorPosition,Quaternion.identity);
        }
      }
    }

}
