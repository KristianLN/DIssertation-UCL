using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMovingSensors_4 : MonoBehaviour
{
    public int NumberOfMovingSensors = 10;
    public GameObject sensor;
    private float extendX;
    private float extendZ;
    Vector3 randomPosition;
    //private GameObject[] movingSensors;
    //private Rigidbody rl;
    //private Vector3 randomMovement;
    //public float movementSpeed;
    //public float continueProbability;
    //private float turn;
    // public float rotateProbability;
    // private float rotate;
    // public float moveProbability;
    //private float move;
    // Start is called before the first frame update
    void Start()
    {
      //environment = GameObject.FindWithTag("Ground");
      extendX = (transform.localScale.x) / 2;
      extendZ = (transform.localScale.z) / 2;

      for (int x = 0; x < NumberOfMovingSensors;x++)
      {
        randomPosition = new Vector3(Random.Range(1 - extendX,extendX - 1),
                               3,
                               Random.Range(1 - extendZ,extendZ - 1));

        Instantiate(sensor,randomPosition,Quaternion.identity);
      }
    }


    void FixedUpdate() // FixedUpdate is used on recommendations by Unity.
    {
      // movingSensors = GameObject.FindGameObjectsWithTag("Pedestrian");
      //
      // for (int i = 0; i < movingSensors.Length;i++)
      // {
      //   // First try
      //   // randomMovement = new Vector3(Random.Range(-50f,50f),3,Random.Range(-50f,50f));
      //   //rl = movingSensors[i].GetComponent<Rigidbody>();
      //   // rl.velocity = randomMovement;
      //   // Draw a random number to see if the agent should rotate
      //   // rotate = Random.Range(0.0f,1.0f);
      //   // if (rotate < rotationProbability)
      //   // {
      //   //   movingSensors[i].transform.rotation = Quaternion.Euler(0,Random.Range(0f,180f),0);
      //   // }
      //   //rl.AddForce(transform.forward * movementSpeed);
      //   //Debug.Log(Random.Range(0f,1f));
      //
      //   // Second try
      //   // turn = Random.Range(0.0f,1.0f);
      //   // if (continueProbability <= turn){
      //   //   movingSensors[i].transform.position += new Vector3(0,0,movementSpeed);
      //   //   // movingSensors[i].transform.position += new Vector3(Random.Range(-movementSpeed,movementSpeed),0,0);
      //   //   // If we move in this direction, increase the probability of doing so.
      //   //   if (continueProbability != 0.0f || continueProbability != 1.0f)
      //   //   {
      //   //     continueProbability += 0.01f;
      //   //   }
      //   // }
      //   // else
      //   // {
      //   //   movingSensors[i].transform.position += new Vector3(0,0,Random.Range(-movementSpeed,movementSpeed));
      //   //   if (continueProbability != 0.0f || continueProbability != 1.0f)
      //   //   {
      //   //     continueProbability -= 0.01f;
      //   //   }
      //   // }
      //
      //   // Third try
      //   // if (Random.Range(0f,1f)< 0.01){
      //   //   rl = movingSensors[i].GetComponent<Rigidbody>();
      //   //   randomPosition = new Vector3(Random.Range(1 - extendX,extendX - 1),
      //   //                          3,
      //   //                          Random.Range(1 - extendZ,extendZ - 1));
      //   //   rl.MovePosition(randomPosition);//+ transform.forward * Time.deltaTime
      //   // }
      //
      // }
    }
}
