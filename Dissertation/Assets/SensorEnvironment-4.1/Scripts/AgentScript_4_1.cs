using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using System.IO;
using System.Text;

public class AgentScript_4_1 : Agent
{
    ////////////////////////////////// Notes //////////////////////////////////

    // GetStepCount(), a under-the-hood-build-in function, is used to monitor the number of steps
    // taken by a agent on every episode.

    // Initialising variables
    Rigidbody rbd;
    RayPerception rayPer;
    Vector3 startPos;
    Vector3 startRot;
    public float speed = 10;
    private float movementHeight;
    private int countingSessions = 0;
    private int sensorCollisionsGlobal = 0;
    private int sensorCollisionsLocal = 0;
    int difficult;
    float markingAngle = 0;
    float markingRange = 3;
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
    // If training in the editor; Assets/Exported_Data
    private string path = "Exported_Data/";
    private string content;
    // Set to false if running multiple environments
    public bool verbose;
    // Reset target
    GameObject parentObject;
    GameObject firstChild;
    AcademyScript_4_1 academyScript;
    targetPlacing_4_1 targetScript;
    // Multiple brains
    public Brain secondBrain;
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
        }

        angle += 2*Mathf.PI / numberOfAngles;
      }
      return hitOccured;
    }
    void markCollision()
    {
      // Can only be utilised in the Editor
      markingAngle = 0;

      for(var i = 0; i < 8;i++)
      {
        RaycastHit hit;

        x = transform.position.x + markingRange*Mathf.Cos(markingAngle);
        z = transform.position.z + markingRange*Mathf.Sin(markingAngle);

        Vector3 dir = new Vector3(x,movementHeight,z);

        Debug.DrawLine(transform.position,dir,Color.green,5.0f);

        markingAngle += 2*Mathf.PI / 8;
      }
    }
    //////////////////////////////////////////////////////////////////////////////////
    // Agent-script specific functions
    public override void InitializeAgent()
    {
      // Storing the default brain, which is necessary is a two-brain-structure is desired.
      firstBrain = brain;
      // Storing our agent for later use
      agent = this.GetComponent<Agent>();
      // Get scripts from other objects.
      academyScript = GameObject.FindWithTag("Academy").GetComponent<AcademyScript_4_1>();
      parentObject = this.transform.parent.gameObject;
      firstChild = parentObject.transform.GetChild(0).gameObject;
      targetScript = firstChild.transform.GetChild(0).GetComponent<targetPlacing_4_1>();
      // Getting the number of areas
      numberOfAreas = GameObject.FindGameObjectsWithTag("Area").Length;

      if (drawTrails)
      {
        bool drawingScript = GetComponent<drawDynamicTrail_4_1>().enabled = true;
      } else
      {
        bool drawingScript = GetComponent<drawDynamicTrail_4_1>().enabled = false;
      }
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
      string[] detectableObjcts = {"Wall","Goal","Obstacle","Pedestrian","Sensor","CrowdedArea"};// <- should include Pedestrian and Sensor!?

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
      // rbd.AddForce(transform.forward * speed);
      rbd.velocity = transform.forward * speed;
      // Time penalty
      AddReward(-0.0005f);

      // if (distance < radius * 2)
      // {
      //   AddReward(-0.5f);
      // }
    }
    public override void AgentReset()
    {
      // Because of the two-brains-set-up
      if (resetLocations)
      {
        // Incrementing the counter
        countingSessions += 1;
        // Reset sensor collision tracker
        sensorCollisionsGlobal = 0;
        // Reset rotation and position
        transform.position = startPos;
        transform.eulerAngles = startRot;

        // Reset velocity
        rbd.velocity = Vector3.zero;
        rbd.angularVelocity = Vector3.zero;

        // Change location
        if ((int)academyScript.resetParameters["NumberOfSensorClouds"] != 0)
        {
          // Getting all sensors and crowded areas within the environment
          sensors.Clear();
          crowdedAreas.Clear();

          foreach (Transform child in firstChild.transform)
          {
            if (child.tag == "Sensor")
            {
              sensors.Add(child.gameObject);
            }
            if (child.tag == "CrowdedArea")
            {
              crowdedAreas.Add(child.gameObject);
            }
          }
          // Determining how many sensors are supposed to be in each cloud.
          numberOfSensors = sensors.Count / (int)academyScript.resetParameters["NumberOfSensorClouds"];//NumberofSensorClouds
          // Updating the position of the clouds.
          for (int j = 0; j < academyScript.resetParameters["NumberOfSensorClouds"];j++)//NumberofSensorClouds
          {
            randomPosition = new Vector3(Random.Range(academyScript.resetParameters["Radius"] - academyScript.extendX,academyScript.extendX - academyScript.resetParameters["Radius"]),
                                   environment.transform.localScale.y,
                                   Random.Range(academyScript.resetParameters["Radius"] - academyScript.extendZ,academyScript.extendZ - academyScript.resetParameters["Radius"]));

            // Ensuring that the sensors appears in the correct area.
            randomPosition = randomPosition + firstChild.transform.position;
            // Adding the correct amount of sensors in each cloud.
            for(int i = hold; i < (hold + numberOfSensors);i++)
            {
              Vector3 sensorPosition = Random.insideUnitSphere * academyScript.resetParameters["Radius"] + randomPosition;
              sensorPosition.y = movementHeight; //1.5f
              sensors[i].transform.position = sensorPosition;
            }
            // Ensuring that the crowdedArea obejct is placed in the center of the spawning area for the sensors within the particular cloud.
            crowdedAreas[j].transform.position = randomPosition;

            hold += numberOfSensors;
          }
          hold = 0;
        }
        // Change location of target
        targetScript.resetTarget();
      }

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
          if (verbose)
          {
            exportData(path+nameOfFile[0],
                      ""+trails[(trails.Length - 1)].GetComponent<LineRenderer>().positionCount + "\n");
          }
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
    // Collision detection
    void OnCollisionEnter(Collision collision)//public
    {
      if (collision.gameObject.CompareTag("Wall"))//Collider
      {
        // markCollision();
        AddReward(-1f);
        if (verbose)
        {
          // Tracking collision with pedestrians.
          exportData(path+nameOfFile[1],"0\n");
          // Tracking collisions with sensors
          exportData(path+nameOfFile[2],sensorCollisionsGlobal+", 0\n");//countingSessions+", "+
          // Getting the steps
          exportData(path+nameOfFile[3],"0"+", "+GetStepCount()+", "+DifficultArea(firstChild.transform.position)+"\n");
        }
        Done();
      }

      if ( collision.gameObject.CompareTag("Obstacle"))//collider
      {
        AddReward(-1f);
        if (verbose)
        {
          // Tracking collision with pedestrians.
          exportData(path+nameOfFile[1],"0\n");
          // Tracking collisions with sensors
          exportData(path+nameOfFile[2],sensorCollisionsGlobal+", 0\n");//countingSessions+", "+
          // Getting the steps
          exportData(path+nameOfFile[3],"0"+", "+GetStepCount()+", "+DifficultArea(firstChild.transform.position)+"\n");
        }
        Done();
      }

      if ( collision.gameObject.CompareTag("Pedestrian"))//collider
      {
        AddReward(-1f);
        if (verbose)
        {
          // Tracking collision with pedestrians.
          exportData(path+nameOfFile[1],"1\n");
          // Tracking collisions with sensors
          exportData(path+nameOfFile[2],sensorCollisionsGlobal+", 0\n");//countingSessions+", "+
          // Getting the steps
          exportData(path+nameOfFile[3],"0"+", "+GetStepCount()+", "+DifficultArea(firstChild.transform.position)+"\n");
        }
        Done();
      }

      if (collision.gameObject.CompareTag("Goal"))//collider
      {
        AddReward(1f);//5f
        if (verbose)
        {
          // Tracking collision with pedestrians.
          exportData(path+nameOfFile[1],"0\n");
          // Tracking collisions with sensors
          exportData(path+nameOfFile[2],sensorCollisionsGlobal+", 1\n");//countingSessions+", "+
          // Getting the steps
          exportData(path+nameOfFile[3],"1"+", "+GetStepCount()+", "+DifficultArea(firstChild.transform.position)+"\n");
        }
        Done();

      }
    }
    void OnTriggerEnter(Collider collider)//private
    {
      if (collider.gameObject.CompareTag("Sensor"))//without gameObject
      {
        AddReward(-1.0f);//-0.1f|-0.001f
        sensorCollisionsGlobal += 1;
        // sensorCollisionsLocal += 1;
        Done();
        // if (verbose)
        // {
        //   exportData(path+nameOfFile[2],countingSessions+", 1\n");
        // }
      } else if (collider.gameObject.CompareTag("CrowdedArea"))//without gameObject
      {
        Collider[] potentialCollisions = Physics.OverlapSphere(collider.gameObject.transform.position,(int)academyScript.resetParameters["Radius"]);
        if (academyScript.resetParameters["NoiseProb"] > Random.Range(0.0f,1.0f))
        {
          AddReward(-0.03f * (potentialCollisions.Length + Random.Range(0,potentialCollisions.Length)));//-0.001f
        } else
        {
          AddReward(-0.03f * potentialCollisions.Length);//-0.001f
        }
        // AddReward(-0)
        // AddReward(-0.5f);
        // sensorCollisionsLocal = 0;
      }
    }
    // void OnTriggerExit(Collider collider)//private
    // {
    //   if (collider.gameObject.CompareTag("CrowdedArea"))//without gameObject
    //   {
    //     if (academyScript.resetParameters["NoiseProb"] > Random.Range(0.0f,1.0f))
    //     {
    //       AddReward(-0.1f * (sensorCollisionsLocal + Random.Range(0,sensorCollisionsLocal)));//-0.001f
    //     } else
    //     {
    //       AddReward(-0.1f * sensorCollisionsLocal);//-0.001f
    //     }
    //   }
    // }
    // Switching brains
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
    ///////////////////////////////////////////////////
}
