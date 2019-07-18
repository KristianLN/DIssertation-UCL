using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using System.IO;
using System.Text;
//using System;

public class AgentScript_5_InvisibleAgent : Agent
{
    // Initialising variables
    Rigidbody rbd;
    RayPerception rayPer;
    Vector3 startPos;
    Vector3 startRot;
    [HideInInspector]
    public Vector3 rotateDir;
    public float speed = 10;
    private float movementHeight;
    private int countingSessions = 0;
    private int sensorCollisionsGlobal = 0;
    private int sensorCollisionsLocal = 0;
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
    List<GameObject> sensors = new List<GameObject>();
    List<GameObject> crowdedAreas = new List<GameObject>();
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
    AcademyScript_5_lowLevel academyScript;
    targetPlacing_5 targetScript;
    // Low-level agent reward.
    List<Vector3> intendedPath = new List<Vector3>();
    List<Vector3> realisedPath = new List<Vector3>();
    GameObject invisibleAgent;
    public int RewardFrequency;
    private float maximumDistancePossible;

    /////////////////////////////////// Functions ////////////////////////////////////
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
      // Storing the default brain, which is necessary is a two-brain-structure is desired.
      // firstBrain = brain;
      // Storing our agent for later use
      // agent = this.GetComponent<Agent>();
      // Get scripts from other objects.
      academyScript = GameObject.FindWithTag("Academy").GetComponent<AcademyScript_5_lowLevel>();
      parentObject = this.transform.parent.gameObject;
      firstChild = parentObject.transform.GetChild(0).gameObject;
      targetScript = firstChild.transform.GetChild(0).GetComponent<targetPlacing_5>();
      invisibleAgent = GameObject.FindWithTag("InvisibleAgent");
      // Getting the number of areas
      numberOfAreas = GameObject.FindGameObjectsWithTag("Area").Length;
      maximumDistancePossible = Mathf.Sqrt(Mathf.Pow(firstChild.transform.localScale.x,2) + Mathf.Pow(firstChild.transform.localScale.z,2));
      // // Drawing trails if wished.
      // if (drawTrails)
      // {
      //   bool drawingScript = GetComponent<drawDynamicTrail_5>().enabled = true;
      // } else
      // {
      //   bool drawingScript = GetComponent<drawDynamicTrail_5>().enabled = false;
      // }

      // Initialising variables
      rbd = GetComponent<Rigidbody>();
      // rayPer = GetComponent<RayPerception3D>();
      startPos = transform.position;
      startRot = transform.eulerAngles;

      // if (NumberOfTrails == 0)
      // {
      //   NumberOfTrails = 50;
      // }

      // environment = GameObject.FindWithTag("Ground");
      // movementHeight = academyScript.heightOfMovingObjects;
      // material = new Material(Shader.Find("Sprites/Default"));
    }
    public override void CollectObservations()
    {
      float rayDistance = 20f;//50f
      float[] rayAngles = {10f,20f,30f,40f,50f,60f,70f,80f,90f,100f,110f,120f,130f,140f,150f,160f,170f,180f};
      string[] detectableObjcts = {"Pedestrian","Sensor"};// "Wall","Goal","Obstacle",

      AddVectorObs(rayPer.Perceive(rayDistance,rayAngles,detectableObjcts,0f,0f));
      // Adding the observations from the high-level agent.
      AddVectorObs(GetComponent<AgentScript_5_highLevel>().rotateDir);
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

    private void OnTriggerEnter(Collider collider)
    {
      if (collider.CompareTag("Wall"))
      {
        // AddReward(-1f);
        // // Tracking collision with pedestrians.
        // exportData(path+nameOfFile[1],"0\n");
        // // Tracking collisions with sensors
        // exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisionsGlobal+", 0\n");
        // Punish the low-level agent proportional to the distance between the invisible agent and the actual agent
        float dist = Vector3.Distance(transform.position, GameObject.FindWithTag("Agent").transform.position);
        float reward = (-1) * (dist / maximumDistancePossible); // We multiply by -1 to ensure that a great distance is negatively rewarded and reverse for a short distance.
        GameObject.FindWithTag("Agent").GetComponent<AgentScript_5_lowLevel>().AddReward(reward);

        // Resetting both the high-level and low-level agent
        GameObject.FindWithTag("Agent").GetComponent<AgentScript_5_highLevel>().AgentReset();
        GameObject.FindWithTag("Agent").GetComponent<AgentScript_5_lowLevel>().AgentReset();
        Done();
      }

      if (collider.CompareTag("Obstacle"))
      {
        // AddReward(-1f);
        // // Tracking collision with pedestrians.
        // exportData(path+nameOfFile[1],"0\n");
        // // Tracking collisions with sensors
        // exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisionsGlobal+", 0\n");
        // Punish the low-level agent proportional to the distance between the invisible agent and the actual agent
        float dist = Vector3.Distance(transform.position, GameObject.FindWithTag("Agent").transform.position);
        float reward = (-1) * (dist / maximumDistancePossible); // We multiply by -1 to ensure that a great distance is negatively rewarded and reverse for a short distance.
        GameObject.FindWithTag("Agent").GetComponent<AgentScript_5_lowLevel>().AddReward(reward);

        // Resetting both the high-level and low-level agent
        GameObject.FindWithTag("Agent").GetComponent<AgentScript_5_highLevel>().AgentReset();
        GameObject.FindWithTag("Agent").GetComponent<AgentScript_5_lowLevel>().AgentReset();
        Done();
      }

      if (collider.CompareTag("Goal"))
      {
        // AddReward(-1f);
        // // Tracking collision with pedestrians.
        // exportData(path+nameOfFile[1],"0\n");
        // // Tracking collisions with sensors
        // exportData(path+nameOfFile[2],countingSessions+", "+sensorCollisionsGlobal+", 0\n");
        // Punish the low-level agent proportional to the distance between the invisible agent and the actual agent
        float dist = Vector3.Distance(transform.position, GameObject.FindWithTag("Agent").transform.position);
        float reward = (-1) * (dist / maximumDistancePossible); // We multiply by -1 to ensure that a great distance is negatively rewarded and reverse for a short distance.
        GameObject.FindWithTag("Agent").GetComponent<AgentScript_5_lowLevel>().AddReward(reward);

        Done();
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
    }

}
