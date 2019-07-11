using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSensors_dynamically : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject sensor;
    public float numberOfSensors = 10;
    public float radius = 5;
    //public Transform[] sensorLocations;
    public GameObject environment;
    private float extendX;
    private float extendZ;
    Vector3 randomPosition;

    public void Start()
    {
      extendX = (environment.transform.localScale.x) / 2;
      extendZ = (environment.transform.localScale.z) / 2;

      randomPosition = new Vector3(Random.Range(radius - extendX,extendX - radius),
                             environment.transform.localScale.y,
                             Random.Range(radius - extendZ,extendZ - radius));

      for (int x = 0; x < numberOfSensors;x++)
      {
        Vector3 sensorPosition = Random.insideUnitSphere * radius + randomPosition;
        sensorPosition.y = 3;
        Instantiate(sensor,sensorPosition,Quaternion.identity);//Random.insideUnitSphere * radius + randomPosition instead of sensorPosition
      }
    }

}
