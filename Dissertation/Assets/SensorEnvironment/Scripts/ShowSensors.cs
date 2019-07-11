using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSensors : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject sensor;
    public float numberOfSensors = 10;
    public float radius = 5;
    public Transform[] sensorLocations;

    void Start()
    {
      int rand = Random.Range(0,sensorLocations.Length);
      if (rand < 2){
        radius = 10;
      }
      for (int x = 0; x < numberOfSensors;++x)
      {
        Instantiate(sensor,Random.insideUnitSphere * radius + sensorLocations[rand].position,Quaternion.identity);
      }
    }

}
