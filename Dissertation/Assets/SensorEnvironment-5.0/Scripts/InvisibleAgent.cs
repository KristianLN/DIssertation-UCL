using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleAgent : MonoBehaviour
{
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

        // Instead of calling Done() on a regular agent
        // Reset rotation and position
        transform.position = startPos;
        transform.eulerAngles = startRot;

        // Reset velocity
        rbd.velocity = Vector3.zero;
        rbd.angularVelocity = Vector3.zero;
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

        // Instead of calling Done() on a regular agent
        // Reset rotation and position
        transform.position = startPos;
        transform.eulerAngles = startRot;

        // Reset velocity
        rbd.velocity = Vector3.zero;
        rbd.angularVelocity = Vector3.zero;
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

        // Instead of calling Done() on a regular agent
        // Reset rotation and position
        transform.position = startPos;
        transform.eulerAngles = startRot;

        // Reset velocity
        rbd.velocity = Vector3.zero;
        rbd.angularVelocity = Vector3.zero;
      }
    }
}
