﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using System.IO;
using System.Text;
//using System;

public class AgentScript_3 : Agent
{
    // Initialising variables
    Rigidbody rbd;
    RayPerception rayPer;
    Vector3 startPos;
    Vector3 startRot;
    public float speed = 10;
    private float movementHeight;
    private int countingSessions = 0;
    private int sensorCollisions = 0;
    // Sensor related
    private GameObject[] allSensors;
    private Vector3 center;
    private GameObject environment;
    private float distance;
    Vector3 randomPosition;
    private int numberOfSensors;
    private int hold = 0;
    private int numberOfAreas;
    private int sensorsInThisArea;
    List<GameObject> Sensors = new List<GameObject>();
    // Drawing trails
    private GameObject[] trails;
    public float NumberOfTrails;
    private GameObject trail;
    private GameObject agentObject;
    private LineRenderer lr;
    private LineRenderer lrC;
    private float alpha;
    private Material material;
    private int maximumPositions = 0;
    public bool drawTrails = false;
    // Exporting draw
    public string[] nameOfFile;
    private string path = "Assets/Exported_Data/";
    private string content;
    // Reset target
    GameObject parentObject;
    GameObject firstChild;
    AcademyScript_3 academyScript;
    targetPlacing_3 targetScript;
    /////////////////////////////////// Functions ////////////////////////////////////

    public Vector3 GetCenter()
    {
      // Initialize to make sure that everything is reset at the beginning
      center = new Vector3(0,0,0);

      // Locate all sensors within the screen
      allSensors = GameObject.FindGameObjectsWithTag("Sensor");

      // Calculate the center of the sensors, through the average position of the sensors.
      for(var i = 0; i < allSensors.Length;i++)
      {
        center += (allSensors[i].transform.position / allSensors.Length);
      }

      return center;
    }
    public void createTrail()
    {
      agentObject = GameObject.FindWithTag("Agent");
      // Create the new objec
      trail = new GameObject();
      // Configurate the new game object
      trail.name = "Trail";
      trail.tag = "Trail";
      trail.transform.parent = agentObject.transform;
      trail.AddComponent<LineRenderer>().SetColors(Color.blue,Color.blue);
      trail.GetComponent<LineRenderer>().SetWidth(0.2f,0.2f);
      // A neutral material for the color to take effect.
      trail.GetComponent<LineRenderer>().material = material;
    }
    public void exportData(string pathToFile, string contentToWrite)
    {
      File.AppendAllText(pathToFile,contentToWrite);
    }
    //////////////////////////////////////////////////////////////////////////////////
    public void Start()
    {
      // Get scripts from other objects.
      AcademyScript_3 academyScript = GameObject.FindWithTag("Academy").GetComponent<AcademyScript_3>();

      // Getting the number of areas
      numberOfAreas = GameObject.FindGameObjectsWithTag("Area").Length;

      if (drawTrails)
      {
        bool drawingScript = GetComponent<drawDynamicTrail_3>().enabled = true;
      } else
      {
        bool drawingScript = GetComponent<drawDynamicTrail_3>().enabled = false;
      }

      //countingSessions = 0;
      // Initialising variables
      rbd = GetComponent<Rigidbody>();
      rayPer = GetComponent<RayPerception3D>();
      startPos = transform.position;
      startRot = transform.eulerAngles;

      if (NumberOfTrails == 0)
      {
        NumberOfTrails = 50;
      }

      environment = GameObject.FindWithTag("Ground");
      movementHeight = academyScript.heightOfMovingObjects;
      material = new Material(Shader.Find("Sprites/Default"));
    }

    public override void CollectObservations()
    {
      float rayDistance = 50f;
      float[] rayAngles = {10f,20f,30f,40f,50f,60f,70f,80f,90f,100f,110f,120f,130f,140f,150f,160f,170f,180f};
      string[] detectableObjcts = {"Wall","Goal","Obstacle","Pedestrian","Sensor"};// <- should include Pedestrian and Sensor!?

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

      // if (distance < radius * 2)
      // {
      //   AddReward(-0.5f);
      // }
    }

    public void OnCollisionEnter(Collision collision)
    {
      if (collision.collider.CompareTag("Wall"))
      {
        AddReward(-1f);
        // Tracking collision with pedestrians.
        exportData(path+nameOfFile[1],"0\n");
        // Tracking collisions with sensors
        exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisions+", 0\n");
        Done();
      }

      if ( collision.collider.CompareTag("Obstacle"))
      {
        AddReward(-1f);
        // Tracking collision with pedestrians.
        exportData(path+nameOfFile[1],"0\n");
        // Tracking collisions with sensors
        exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisions+", 0\n");
        Done();
      }

      if ( collision.collider.CompareTag("Pedestrian"))
      {
        AddReward(-1f);
        // Tracking collision with pedestrians.
        exportData(path+nameOfFile[1],"1\n");
        // Tracking collisions with sensors
        exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisions+", 0\n");
        Done();
      }

      if (collision.collider.CompareTag("Goal"))
      {
        AddReward(1f);//5f
        // Tracking collision with pedestrians.
        exportData(path+nameOfFile[1],"0\n");
        // Tracking collisions with sensors
        exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisions+", 1\n");
        Done();

      }
    }
    private void OnTriggerEnter(Collider collider)
    {
      if (collider.CompareTag("Sensor"))
      {
        AddReward(-0.1f);
        sensorCollisions += 1;
        exportData(path+nameOfFile[2],countingSessions+", 1\n");
      }
    }

    public override void AgentReset()
    {
      // Getting the "correct" target
      academyScript = GameObject.FindWithTag("Academy").GetComponent<AcademyScript_3>();
      parentObject = this.transform.parent.gameObject;
      firstChild = parentObject.transform.GetChild(0).gameObject;
      targetScript = firstChild.transform.GetChild(0).GetComponent<targetPlacing_3>();

      // Incrementing the counter
      countingSessions += 1;
      // Reset sensor collision tracker
      sensorCollisions = 0;
      // Reset rotation and position
      transform.position = startPos;
      transform.eulerAngles = startRot;

      // Reset velocity
      rbd.velocity = Vector3.zero;
      rbd.angularVelocity = Vector3.zero;

      // Change location
      if ((int)academyScript.resetParameters["NumberOfSensorClouds"] != 0)
      {
        // Getting sensors within the area
        // allSensors = GameObject.FindGameObjectsWithTag("Sensor");
        // sensorsInThisArea = allSensors.Length / numberOfAreas;

        Sensors.Clear();

        foreach (Transform child in firstChild.transform)
        {
          if (child.tag == "Sensor")
          {
            Sensors.Add(child.gameObject);
          }
        }

        numberOfSensors = Sensors.Count / (int)academyScript.resetParameters["NumberOfSensorClouds"];//NumberofSensorClouds

        for (int j = 0; j < academyScript.resetParameters["NumberOfSensorClouds"];j++)//NumberofSensorClouds
        {
          randomPosition = new Vector3(Random.Range(academyScript.resetParameters["Radius"] - academyScript.extendX,academyScript.extendX - academyScript.resetParameters["Radius"]),
                                 environment.transform.localScale.y,
                                 Random.Range(academyScript.resetParameters["Radius"] - academyScript.extendZ,academyScript.extendZ - academyScript.resetParameters["Radius"]));

          // Ensuring that the sensors appears in the correct area.
          randomPosition = randomPosition + firstChild.transform.position;

          for(int i = hold; i < (hold + numberOfSensors);i++)
          {
            Vector3 sensorPosition = Random.insideUnitSphere * academyScript.resetParameters["Radius"] + randomPosition;
            sensorPosition.y = movementHeight; //1.5f
            Sensors[i].transform.position = sensorPosition;
          }
          hold += numberOfSensors;
        }
        hold = 0;
      }
      // Change location of target
      targetScript.resetTarget();

      // // Calculate the new center
      // center = GetCenter();
      //
      // Updating the trails
      if (drawTrails)
      {
        trails = GameObject.FindGameObjectsWithTag("Trail");
        // Exporting information on the latest trail, before updating.
        if (trails.Length > 0)
        {
          // ""+"some integer" is a way to force the integer to being read as a string.
          exportData(path+nameOfFile[0],
                    ""+trails[(trails.Length - 1)].GetComponent<LineRenderer>().positionCount + "\n");
        }
        // Deciding on whether to simply add a a trail or remove one before adding (if the number of trails is equal to the maximum specified).
        if (trails.Length < NumberOfTrails)
        {
          createTrail();
        } else
        {
          // Updating
          Destroy(trails[0]);
          createTrail();
        }

        // Changing Alpha on the trails
        // Extended implementation
        if (trails.Length % 10 == 0 && trails.Length != 0)// The denominator could be set publically.
        {
          int step = trails.Length / 10;
          int stepFloor = 0;
          int stepRoof = step;

          for (int i = 0;i<trails.Length;i++)
          {
            // Deciding on the level of alpha
            if (i >= stepFloor && i < stepRoof)
            {
              alpha = ((float)stepRoof / trails.Length);
            } else
            {
              stepRoof += step;
              stepFloor += step;
              alpha = ((float)stepRoof / trails.Length);
            }
            // Setting the alpha level
            lrC = trails[i].GetComponent<LineRenderer>();
            if (lrC.positionCount > maximumPositions)
            {
              maximumPositions = lrC.positionCount;
            }
            Gradient gradient = new Gradient();
            gradient.SetKeys(
              new GradientColorKey[] { new GradientColorKey(Color.blue,0.0f), new GradientColorKey(Color.blue,1.0f)},
              new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f),new GradientAlphaKey(alpha, 1.0f)} //(i / trails.Length)
            );
            lrC.colorGradient = gradient;
          }
        }
      }
    }
}
