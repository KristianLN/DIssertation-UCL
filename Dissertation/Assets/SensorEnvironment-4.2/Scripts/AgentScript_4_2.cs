using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using System.IO;
using System.Text;

public class AgentScript_4_2 : Agent
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
    private float stepPenalty = -0.0005f;
    // Sensor related
    private GameObject[] allSensors;
    private Vector3 center;
    private GameObject environment;
    float extendX;
    float extendZ;
    private float distance;
    Vector3 randomPosition;
    private int numberOfSensors;
    private int hold = 0;
    private int numberOfAreas;
    private int sensorsInThisArea;
    List<GameObject> sensors = new List<GameObject>();
    // Crowded Areas
    List<GameObject> crowdedAreas = new List<GameObject>();
    public float[] densities;
    int areaType;
    int slowSteps_act = 0;
    int slowSteps_obs = 0;
    bool countSlowsteps = false;
    int CrowdedAreaCollisions = 0;
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
    private string path; //= "Assets/Exported_Data/";
    private string content;
    // Set to false if running multiple environments
    public bool verbose;
    public bool maxStepInvoked;
    // Reset target
    GameObject parentObject;
    GameObject firstChild;
    AcademyScript_4_2 academyScript;
    targetPlacing_4_2 targetScript;
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
    int count_fix_up = 0;
    int layerMask;
    // Reward shaping
    float priorDistance;
    float distanceToTarget;
    public bool rewardShaping;
    // Uncertainty
    public bool uncertainty;
    float maxLength;
    public float distMean;
    public float distStd;
    // Vector3 endPosition;
    // RaycastHit hit;
    // private float[] subList;
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
    public int DifficultArea()
    {
      if (firstChild.transform.GetChild(1).GetComponent<Collider>().bounds.Contains(firstChild.transform.GetChild(0).position))
      {

        difficult = 1;
      } else if (firstChild.transform.GetChild(2).GetComponent<Collider>().bounds.Contains(firstChild.transform.GetChild(0).position))
      {
        difficult = 1;
      } else
      {
        difficult = 0;
      }

      return difficult;
    }
    public bool checkForCollision(int angles, int range, string coverage = "full",int maskEnd = 8)
    {
      layerMask = 1 << maskEnd;
      angle = 0;
      hitOccured = false;
      for(var i = 0; i < angles;i++)
      {
        RaycastHit hit;

        x = transform.position.x + range*Mathf.Cos(angle);
        z = transform.position.z + range*Mathf.Sin(angle);

        Vector3 dir = new Vector3(x,movementHeight,z);

        if (Physics.Raycast(transform.position,dir, out hit, range,layerMask))
        {

          hitOccured = true;
          // Debug.DrawLine(transform.position,dir,Color.red);
        }
        if (coverage == "full")
        {
          angle += 2*Mathf.PI / numberOfAngles;
        } else if (coverage == "half")
        {
          angle += Mathf.PI / numberOfAngles;
        }

      }
      return hitOccured;
    }
    public List<float> CrowdedAreaInfo(float radius, float noise)
    {
      Collider[] collidersInSight = Physics.OverlapSphere(this.transform.position,radius);
      List<float> importantInfo = new List<float>();

      foreach (Collider collider in collidersInSight)
      {
        if (collider.tag == "CrowdedArea")
        {
          importantInfo.Add(collider.gameObject.GetComponent<AreaInfo>().density);

          float dist = Vector3.Distance(collider.gameObject.transform.position,this.transform.position);
          // Calculating the distance to the edge of the crowdedArea instead of the center.
          float distEdge = dist - environment.transform.localScale.x*collider.gameObject.transform.localScale.x;
          float normDistEdge = (distEdge + noise)/ radius;
          // if (distEdge < 0)
          // {
          //   distEdge = 0f;
          // }
          importantInfo.Add(normDistEdge);
        }
      }
      if ((1 < importantInfo.Count) && (importantInfo.Count < 4))
      {
        // for ( var i = 0; i <= (4 - importantInfo.Count);i++)
        // {
        //   importantInfo.Add(radius);
        // }
        importantInfo.Add(0f);
        importantInfo.Add(1f);
      } else if (importantInfo.Count == 0)
      {
        importantInfo.Add(0f);
        importantInfo.Add(1f);
        importantInfo.Add(0f);
        importantInfo.Add(1f);
      }
      return importantInfo;
    }
    public bool checkForCrowdedArea(float radius)
    {
      Collider[] collidersInSight = Physics.OverlapSphere(this.transform.position,radius);

      bool crowdedArea = false;

      foreach (Collider collider in collidersInSight)
      {
        if (collider.tag == "CrowdedArea")
        {
          crowdedArea = true;
        }
      }

      return crowdedArea;
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
    public float gaussianNoise(float mean, float std)
    {
      List<float> Xuni = new List<float>();
      List<float> Znorm = new List<float>();

      for (int i = 0; i < 2; i++)
      {
        Xuni.Add(Random.value);
      }

      Znorm.Add((mean + std*Mathf.Sqrt(-2 * Mathf.Log(Xuni[0])) * Mathf.Cos(2 * Mathf.PI * Xuni[1])));
      Znorm.Add((mean + std*Mathf.Sqrt(-2 * Mathf.Log(Xuni[0])) * Mathf.Sin(2 * Mathf.PI * Xuni[1])));

      if (Random.value > 0.5)
      {
        return Znorm[0];
      } else
      {
        return Znorm[1];
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
      academyScript = GameObject.FindWithTag("Academy").GetComponent<AcademyScript_4_2>();
      path = GameObject.FindWithTag("Academy").GetComponent<AcademyScript_4_2>().pathForStorage;
      parentObject = this.transform.parent.gameObject;
      firstChild = parentObject.transform.GetChild(0).gameObject;
      targetScript = firstChild.transform.GetChild(0).GetComponent<targetPlacing_4_2>();
      // Getting the distance to the target, for reward shaping.
      if (rewardShaping)
      {
        // distanceToTarget = Vector3.Distance(firstChild.transform.GetChild(0).transform.position,this.transform.position);
        priorDistance = Vector3.Distance(firstChild.transform.GetChild(0).transform.position,this.transform.position);
      }
      // Getting the number of areas
      numberOfAreas = GameObject.FindGameObjectsWithTag("Area").Length;

      if (drawTrails)
      {
        bool drawingScript = GetComponent<drawDynamicTrail_4_2>().enabled = true;
      } else
      {
        bool drawingScript = GetComponent<drawDynamicTrail_4_2>().enabled = false;
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
      extendX = environment.transform.localScale.x;
      extendZ = environment.transform.localScale.z;
      maxLength = Mathf.Sqrt(Mathf.Pow(extendX,2) + Mathf.Pow(extendZ,2));
      movementHeight = academyScript.heightOfMovingObjects;
      material = new Material(Shader.Find("Sprites/Default"));
    }
    public override void CollectObservations()
    {
      float rayDistance = 50f;
      float[] rayAngles = {10f,20f,30f,40f,50f,60f,70f,80f,90f,100f,110f,120f,130f,140f,150f,160f,170f,180f};
      string[] detectableObjcts = {"Wall","Goal","Obstacle","Pedestrian","CrowdedArea"};// "Sensor"

      if (uncertainty)
      {
        if (checkForCrowdedArea(15f))
        {
          float noise = gaussianNoise(distMean,distStd);
          exportData(path+parentObject.name+"_"+nameOfFile[4],noise+"\n");
          AddVectorObs(rayPer.PerceiveUncertainty(rayDistance,rayAngles,detectableObjcts,0f,0f,noise));
          // Adding some more information (Density and distance) on the crowdedAreas.
          AddVectorObs(CrowdedAreaInfo(25f,noise));
          // // Adding the distance to the target as an observation
          float distTarget = Vector3.Distance(firstChild.transform.GetChild(0).transform.position,this.transform.position);
          float normDistTarget = (distTarget + noise) / maxLength;
          AddVectorObs(normDistTarget);
        } else
        {
          // float noise = gaussianNoise(0f,1f);

          AddVectorObs(rayPer.PerceiveUncertainty(rayDistance,rayAngles,detectableObjcts,0f,0f,0f));
          // Adding some more information (Density and distance) on the crowdedAreas.
          AddVectorObs(CrowdedAreaInfo(25f,0f));
          // // Adding the distance to the target as an observation
          float distTarget = Vector3.Distance(firstChild.transform.GetChild(0).transform.position,this.transform.position);
          float normDistTarget = distTarget / maxLength;
          AddVectorObs(normDistTarget);
        }
      } else
      {
        AddVectorObs(rayPer.Perceive(rayDistance,rayAngles,detectableObjcts,0f,0f));
        // Adding some more information (Density and distance) on the crowdedAreas.
        // AddVectorObs(CrowdedAreaInfo(25f,0f));
        // // Adding the distance to the target as an observation
        // float distTarget = Vector3.Distance(firstChild.transform.GetChild(0).transform.position,this.transform.position);
        // float normDistTarget = distTarget / maxLength;
        // AddVectorObs(normDistTarget);
      }
    }
    public override void AgentAction(float[] vectorAction,string textAction)
    {
      if (rewardShaping)
      {
        // Calculate the current distance to the target
        distanceToTarget = Vector3.Distance(firstChild.transform.GetChild(0).transform.position,this.transform.position);
        // If the distance to the target increases, give the step penalty.
        // It is equivalent to give a step penalty all the time, and rewarding
        // the agent with equal sized reward if the agent takes step towards the target.
        if (distanceToTarget > priorDistance)
        {
          AddReward(stepPenalty);
        }
      } else
      {
        AddReward(stepPenalty);
      }

      // Debug.Log(distanceToTarget);
      // AddReward(stepPenalty);
      MoveAgent(vectorAction);

      // Count steps in the crowded areas.
      if (countSlowsteps)
      {
        slowSteps_obs += 1;
      }
    }
    public void MoveAgent(float[] action)
    {
      Vector3 rotateDir = Vector3.zero;
      if(brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
      {
        // rotateDir = transform.up * Mathf.Clamp(vectorAction[0],-1f,1f);
        // rotateDir = transform.up * Mathf.Clamp(vectorAction[1],-1f,1f);
        rotateDir = transform.up * Mathf.Clamp(action[0],-1f,1f);
      }

      // Rotate
      transform.Rotate(rotateDir,Time.deltaTime * 150f);

      // Move
      rbd.AddForce(transform.forward * speed, ForceMode.VelocityChange);

      if (countSlowsteps)
      {
        slowSteps_act += 1;
      }
    }
    public override void AgentReset()
    {
      // Because of the two-brains-set-up
      if (resetLocations)
      {
        // count = 0;
        // count_up = 0;
        count_fix_up = 0;
        // Resetting the crowded areas collision counter
        CrowdedAreaCollisions = 0;
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
          // sensors.Clear();
          crowdedAreas.Clear();

          foreach (Transform child in firstChild.transform)
          {
            if (child.tag == "CrowdedArea")
            {
              crowdedAreas.Add(child.gameObject);
            }
          }
          // Updating the position of the clouds.
          for (int j = 0; j < academyScript.resetParameters["NumberOfSensorClouds"];j++)//NumberofSensorClouds
          {
            randomPosition = new Vector3(Random.Range(academyScript.resetParameters["Radius"] - academyScript.extendX,academyScript.extendX - academyScript.resetParameters["Radius"]),
                                   environment.transform.localScale.y,
                                   Random.Range(academyScript.resetParameters["Radius"] - academyScript.extendZ,academyScript.extendZ - academyScript.resetParameters["Radius"]));

            // Ensuring that the sensors appears in the correct area.
            randomPosition = randomPosition + firstChild.transform.position;

            // Ensuring that the crowdedArea obejct is placed in the center of the spawning area for the sensors within the particular cloud.
            areaType = Random.Range(0,densities.Length);
            crowdedAreas[j].transform.position = randomPosition;
            crowdedAreas[j].GetComponent<AreaInfo>().density = densities[areaType];
            crowdedAreas[j].GetComponent<Renderer>().material = crowdedAreas[j].GetComponent<AreaInfo>().materials[areaType];
          }
        }
        // Change location of target
        targetScript.resetTarget();
        // Debug.Log(DifficultArea());
      }
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
    void OnCollisionEnter(Collision collision)
    {
      if (collision.gameObject.CompareTag("Wall"))
      {
        AddReward(-1f);
        if (verbose)
        {
          // Tracking collision with pedestrians.
          exportData(path+parentObject.name+"_"+nameOfFile[0],"0\n");
          // Tracking collisions with sensors
          // exportData(path+parentObject.name+"_"+nameOfFile[2],sensorCollisionsGlobal+", 0\n");
          // // Getting the steps
          // exportData(path+parentObject.name+"_"+nameOfFile[3],"0"+", "+GetStepCount()+", "+DifficultArea()+"\n");
          // Getting the steps
          exportData(path+parentObject.name+"_"+nameOfFile[1],"0"+", "+count_fix_up+", "+DifficultArea()+"\n");
          // Getting the crowded areas collisions
          exportData(path+parentObject.name+"_"+nameOfFile[3],"0"+", "+CrowdedAreaCollisions+"\n");
        }
        Done();
      }

      if ( collision.gameObject.CompareTag("Obstacle"))//collider
      {
        AddReward(-1f);
        if (verbose)
        {
          // Tracking collision with pedestrians.
          exportData(path+parentObject.name+"_"+nameOfFile[0],"0\n");
          // Tracking collisions with sensors
          // exportData(path+parentObject.name+"_"+nameOfFile[2],sensorCollisionsGlobal+", 0\n");
          // // Getting the steps
          // exportData(path+parentObject.name+"_"+nameOfFile[3],"0"+", "+GetStepCount()+", "+DifficultArea()+"\n");
          // Getting the steps
          exportData(path+parentObject.name+"_"+nameOfFile[1],"0"+", "+count_fix_up+", "+DifficultArea()+"\n");
          // Getting the crowded areas collisions
          exportData(path+parentObject.name+"_"+nameOfFile[3],"0"+", "+CrowdedAreaCollisions+"\n");
        }
        Done();
      }

      if ( collision.gameObject.CompareTag("Pedestrian"))
      {
        AddReward(-1f);
        if (verbose)
        {
          // Tracking collision with pedestrians.
          exportData(path+parentObject.name+"_"+nameOfFile[0],"1\n");
          // Tracking collisions with sensors
          // exportData(path+parentObject.name+"_"+nameOfFile[2],sensorCollisionsGlobal+", 0\n");
          // // Getting the steps
          // exportData(path+parentObject.name+"_"+nameOfFile[3],"0"+", "+GetStepCount()+", "+DifficultArea()+"\n");
          // Getting the steps
          exportData(path+parentObject.name+"_"+nameOfFile[1],"0"+", "+count_fix_up+", "+DifficultArea()+"\n");
          // Getting the crowded areas collisions
          exportData(path+parentObject.name+"_"+nameOfFile[3],"0"+", "+CrowdedAreaCollisions+"\n");
        }
        Done();
      }

      if (collision.gameObject.CompareTag("Goal"))
      {
        AddReward(1f);
        if (verbose)
        {
          // Tracking collision with pedestrians.
          exportData(path+parentObject.name+"_"+nameOfFile[0],"0\n");
          // Tracking collisions with sensors
          // exportData(path+parentObject.name+"_"+nameOfFile[2],sensorCollisionsGlobal+", 1\n");
          // // Getting the steps
          // exportData(path+parentObject.name+"_"+nameOfFile[3],"1"+", "+GetStepCount()+", "+DifficultArea()+"\n");
          // Getting the steps
          exportData(path+parentObject.name+"_"+nameOfFile[1],"1"+", "+count_fix_up+", "+DifficultArea()+"\n");
          // Getting the crowded areas collisions
          exportData(path+parentObject.name+"_"+nameOfFile[3],"1"+", "+CrowdedAreaCollisions+"\n");
        }
        Done();

      }
    }
    void OnTriggerEnter(Collider collider)
    {
      // if (collider.gameObject.CompareTag("Sensor"))//without gameObject
      // {
      //   AddReward(-1.0f);//-0.1f|-0.001f
      //   sensorCollisionsGlobal += 1;
      //   // sensorCollisionsLocal += 1;
      //   Done();
      //   // if (verbose)
      //   // {
      //   //   exportData(path+nameOfFile[2],countingSessions+", 1\n");
      //   // }
      // } else
      if (collider.gameObject.CompareTag("CrowdedArea"))//without gameObject
      {
        // Collider[] potentialCollisions = Physics.OverlapSphere(collider.gameObject.transform.position,(int)academyScript.resetParameters["Radius"]);
        // if (academyScript.resetParameters["NoiseProb"] > Random.Range(0.0f,1.0f))
        // {
        //   AddReward(-0.03f * (potentialCollisions.Length + Random.Range(0,potentialCollisions.Length)));//-0.001f
        // } else
        // {
        //   AddReward(-0.03f * potentialCollisions.Length);//-0.001f
        // }
        // AddReward(-0)
        // AddReward(-0.5f);
        // sensorCollisionsLocal = 0;
        countSlowsteps = true;
        stepPenalty = -collider.gameObject.GetComponent<AreaInfo>().density;
        // agent.GetComponent<AgentScript_4_2>().speed = 5f;
      }
    }
    void OnTriggerExit(Collider collider)
    {
      if (collider.gameObject.CompareTag("CrowdedArea"))//without gameObject
      {
        // if (academyScript.resetParameters["NoiseProb"] > Random.Range(0.0f,1.0f))
        // {
        //   AddReward(-0.1f * (sensorCollisionsLocal + Random.Range(0,sensorCollisionsLocal)));//-0.001f
        // } else
        // {
        //   AddReward(-0.1f * sensorCollisionsLocal);//-0.001f
        // }
        countSlowsteps = false;
        CrowdedAreaCollisions += 1;
        if (verbose)
        {
          exportData(path+parentObject.name+"_"+nameOfFile[2],slowSteps_act+","+stepPenalty+"\n");
        }
        slowSteps_act = 0;
        slowSteps_obs = 0;
        stepPenalty = -0.0005f;
        // agent.GetComponent<AgentScript_4_2>().speed = 10f;
      }
    }
    // Switching brains
    public void FixedUpdate()
    {
      count_fix_up += 1;
      if (secondBrain != null)
      {
        if (checkForCollision((int)18f, (int)10f, "half", (int)8f))
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
      // Debug.Log(maxStep);
      if (maxStepInvoked)
      {
        if (count_fix_up>=(int)4000f)
        {
          // Getting the steps
          exportData(path+parentObject.name+"_"+nameOfFile[1],"0"+", "+GetStepCount()+", "+DifficultArea()+"\n");
        }
      }
    }
    ///////////////////////////////////////////////////
}
