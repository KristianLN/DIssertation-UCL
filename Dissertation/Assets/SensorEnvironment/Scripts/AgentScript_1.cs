using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class AgentScript_1 : Agent
{
    // Initialising variables
    Rigidbody rbd;
    RayPerception rayPer;
    Vector3 startPos;
    Vector3 startRot;

    public float speed = 10;
    public Transform target; //public Target target
    public Transform[] TargetSpawns;
    Rigidbody Rbb;
    public GameObject Target;
    private ShowSensors_dynamically Script;
    private GameObject[] allSensors;
    private Vector3 center;
    private Vector3 currentPosition;
    private Vector3 currentAbsPosition;
    private float counter = 0;

    /////////////////////////////
    public GameObject environment;
    public float radius = 5;
    private float extendX;
    private float extendZ;
    private float distance;
    Vector3 randomPosition;


    // Start is called before the first frame update
    private void Start()
    {
      rbd = GetComponent<Rigidbody>();
      rayPer = GetComponent<RayPerception3D>();
      startPos = transform.position;
      startRot = transform.eulerAngles;

      //Debug.Log(GetCenter());
      //center = GetCenter();
      //Debug.Log("The number of sensors are: " + allSensors.Length);
      //Debug.Log(center);

    }
    Vector3 GetCenter()
    {
      // Initialize to make sure that everything is reset at the beginning
      center = new Vector3(0,0,0);
      counter = 0;
      //allSensors = new GameObject[0];
      //Debug.Log("1: The number of sensors are: " + allSensors.Length);
      // Locate all sensors within the screen
      allSensors = GameObject.FindGameObjectsWithTag("Sensor");
      //Debug.Log(allSensors);
      // Calculate the center of the sensors, through the average position of the sensors.
      //Debug.Log("2: The number of sensors are: " + allSensors.Length);
      for(var i = 0; i < allSensors.Length;i++)
      {
        //Debug.Log(allSensors[i].transform.position);
        //Debug.Log(i);
        center += (allSensors[i].transform.position / allSensors.Length);
        //counter += 1;
      }
      //Debug.Log(i);
      //center = center / counter;
      return center;
    }

    public override void CollectObservations()
    {
      float rayDistance = 50f;
      float[] rayAngles = {10f,20f,30f,40f,50f,60f,70f,80f,90f,100f,110f,120f,130f,140f,150f,160f,170f,180f};
      string[] detectableObjcts = {"Wall","Goal","Obstacle"};//

      // Distance to center of Sensors
      // currentPosition = transform.position;
      //currentAbsPosition = new Vector3(Mathf.Abs(currentPosition.x),currentPosition.y,Mathf.Abs(currentPosition.z));

      // Debug.Log("Information");
      // //Debug.Log(startPos);//transform.position
      // center = GetCenter();
      // Debug.Log(center);
      // Debug.Log(Vector3.Distance(center,currentPosition));
      //Debug.Log(Vector3.Distance(center,currentAbsPosition));
      AddVectorObs(rayPer.Perceive(rayDistance,rayAngles,detectableObjcts,0f,0f));
      // Adding the distance to the observations received.
      // distance = Vector3.Distance(center,transform.position);
      // AddVectorObs(distance);
    }

    public override void AgentAction(float[] vectorAction,string textAction)
    {
      Vector3 rotateDir = Vector3.zero;
      if(brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
      {
        rotateDir = transform.up * Mathf.Clamp(vectorAction[0],-1f,1f);
      }

      // Rotate
      transform.Rotate(rotateDir,Time.deltaTime * 150f);

      // Move
      rbd.velocity = transform.forward * speed;

      // Time penalty
      AddReward(-0.0005f);

      // if (distance < 7)
      // {
      //   AddReward(-0.5f);
      // }


    }

    private void OnCollisionEnter(Collision collision)
    {
      if (collision.collider.CompareTag("Wall"))
      {
        AddReward(-1f);
        Done();
      }

      if ( collision.collider.CompareTag("Obstacle"))
      {
        AddReward(-1f);
        Done();
      }

      if (collision.collider.CompareTag("Goal"))
      {
        AddReward(5f);

        int rand = Random.Range(0,TargetSpawns.Length);
        Rbb = Target.GetComponent<Rigidbody>();
        Rbb.MovePosition(TargetSpawns[rand].position);
        Done();

      }
    }
    private void OnTriggerEnter(Collider collider)
    {
      if (collider.CompareTag("Sensor"))
      {

        AddReward(-0.1f);
      }
    }
    public override void AgentReset()
    {

      // Reset rotation and position
      transform.position = startPos;
      transform.eulerAngles = startRot;

      // Reset velocity
      rbd.velocity = Vector3.zero;
      rbd.angularVelocity = Vector3.zero;

      // Get new position
      extendX = (environment.transform.localScale.x) / 2;
      extendZ = (environment.transform.localScale.z) / 2;

      randomPosition = new Vector3(Random.Range(radius - extendX,extendX - radius),
                             environment.transform.localScale.y,
                             Random.Range(radius - extendZ,extendZ - radius));


      // Change location
      allSensors = GameObject.FindGameObjectsWithTag("Sensor");

      for(int i = 0; i < allSensors.Length;i++)
      {
        Vector3 sensorPosition = Random.insideUnitSphere * radius + randomPosition;
        sensorPosition.y = 3;
        allSensors[i].transform.position = sensorPosition;//Random.insideUnitSphere * radius + randomPosition
      }

      // Calculate the new center
      center = GetCenter();
    }

}
