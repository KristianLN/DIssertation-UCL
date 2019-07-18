using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using System.IO;
using System.Text;
//using System;

public class AgentScript_5_invisibleAgent_Clean : Agent
{
    // Initialising variables
    Rigidbody rbd;
    RayPerception rayPer;
    Vector3 startPos;
    Vector3 startRot;
    [HideInInspector]
    public Vector3 rotateDir;
    GameObject parentObject;
    GameObject firstChild;

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
      // Get other objects.
      parentObject = this.transform.parent.gameObject;
      firstChild = parentObject.transform.GetChild(0).gameObject;


      // Initialising variables
      rbd = GetComponent<Rigidbody>();
      startPos = transform.position;
      startRot = transform.eulerAngles;
      maximumDistancePossible = Mathf.Sqrt(Mathf.Pow(firstChild.transform.localScale.x,2) + Mathf.Pow(firstChild.transform.localScale.z,2));
    }
    public override void CollectObservations()
    {

    }
    public override void AgentAction(float[] vectorAction,string textAction)
    {

    }

    private void OnTriggerEnter(Collider collider)
    {
      if (collider.CompareTag("Wall"))
      {
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
    public void Update()
    {

    }
}
