using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawDynamicTrail : MonoBehaviour
{
    private GameObject[] trails;
    private int traceStep = 0;
    private int traceNumber = 0;
    private int currentTrace;
    private LineRenderer lr;
    private float timeToDraw = 0;
    public bool drawTrails = false;
    
    void Update()
    {
      if (drawTrails){
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
          timeToDraw = Time.time + 0.5f;
        }
      }
    }
}
