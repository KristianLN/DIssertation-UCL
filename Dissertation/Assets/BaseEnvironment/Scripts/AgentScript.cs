using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class AgentScript : Agent
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
    //private float counter = 0;

    // Start is called before the first frame update
    private void Start()
    {
      rbd = GetComponent<Rigidbody>();
      rayPer = GetComponent<RayPerception3D>();
      startPos = transform.position;
      startRot = transform.eulerAngles;
    }

    public override void CollectObservations()
    {
      float rayDistance = 50f;
      float[] rayAngles = {10f,20f,30f,40f,50f,60f,70f,80f,90f,100f,110f,120f,130f,140f,150f,160f,170f,180f};
      string[] detectableObjcts = {"Wall","Goal","Obstacle"};//
      AddVectorObs(rayPer.Perceive(rayDistance,rayAngles,detectableObjcts,0f,0f));
    }

    public override void AgentAction(float[] vectorAction,string textAction)
    {
      Vector3 rotateDir = Vector3.zero;
      if(brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
      {
        rotateDir = transform.up * Mathf.Clamp(vectorAction[0],-1f,1f);
      }
      // else
      // {
      //   var action = Mathf.FloorToInt(vectorAction[0]);
      //
      //   switch(action)
      //   {
      //     case 0: rotateDir = transform.up * 1f;
      //             break;
      //     case 1: rotateDir = transform.up * -1f;
      //             break;
      //     case 2: rotateDir = Vector3.zero;
      //             break;
      //   }
      // }
      // Rotate
      transform.Rotate(rotateDir,Time.deltaTime * 150f);

      // Move
      rbd.velocity = transform.forward * speed;

      // Time penalty
      AddReward(-0.0005f);
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
