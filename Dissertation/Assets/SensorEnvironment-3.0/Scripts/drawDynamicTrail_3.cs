using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.IO;
using System.Text;

public class drawDynamicTrail_3 : MonoBehaviour
{
    private GameObject[] trails;
    private int traceStep = 0;
    private int traceNumber = 0;
    private int currentTrace;
    private LineRenderer lr;
    private float timeToDraw = 0;
    public float timeDelay = 0.5f;

    void Awake()
    {
      // trails = GameObject.FindGameObjectsWithTag("Trail");
      // AgentScript_3 agentScript = GameObject.FindWithTag("Agent").GetComponent<AgentScript_3>();
      // AcademyScript_3 academyScript = GameObject.FindWithTag("Academy").GetComponent<AcademyScript_3>();
    }

    void Update()
    {
      // trails = GameObject.FindGameObjectsWithTag("Trail");
      // Debug.Log(academyScript.heightOfMovingObjects);
      // if (agentScript.drawTrails){//drawTrails
      if (Time.time > timeToDraw)
      {
        trails = GameObject.FindGameObjectsWithTag("Trail");

        if (trails.Length != traceNumber)
        {
          traceNumber = trails.Length;
          traceStep = 0;
        } else
        {
          currentTrace = traceNumber - 1;
          lr = trails[currentTrace].GetComponent<LineRenderer>();

          if (traceStep >= lr.positionCount)
          {
            lr.positionCount = traceStep + 1;
          }
          lr.SetPosition(traceStep,transform.position);
          traceStep += 1;
        }
        timeToDraw = Time.time + timeDelay;
      }
      // }
    }
}
