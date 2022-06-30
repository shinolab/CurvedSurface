using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    //Switch
    bool flagMove = true; //true -> Near, false -> Far
    public bool FlagMove
    {
        get { return this.flagMove; }
        private set { this.flagMove = value; }
    }
    bool onCollision = false;
    public bool OnCollision
    {
        get { return this.onCollision; }
        private set { this.onCollision = value; }
    }

    Vector3 startPosition= new Vector3(0,0.05f,0.06f);
    Vector3 goalPosition = new Vector3(0, 0.248f, 0.06f);

    float t, leapt = 0f;

    //Go (0,0.27,0.1) -> (0,0,0.1) linear uniform motion

    private float goElapedTime;
    private const float goMoveTime = 0.7f;

    //Back (0,0,0.1) -> (0,0.27,0.1) Smooth Dump

    private float backElapsedTime;
    private const float backMoveTime = 0.7f;

    //https://appleorbit.hatenablog.com/entry/2015/10/18/210614



    void Start()
    {

    }

    void Update()
    {
        if (flagMove == true)
        {
            if (Vector3.SqrMagnitude(transform.position - goalPosition) < 0.0001f)
            {
                onCollision = true;

                //Debug.Log("Goal: "+Time.time);
                flagMove = false;              
                backElapsedTime = 0f;
            }

            t = Mathf.Min(goElapedTime / goMoveTime, 1f);
            leapt = (t * t) * (3f - (2f * t));
            transform.position = Vector3.Lerp(startPosition, goalPosition, t);
            goElapedTime += Time.deltaTime;
        }
        else
        {
            if(onCollision == true) onCollision = false;

            if (Vector3.SqrMagnitude(transform.position - startPosition) < 0.0001f)
            {
                //Debug.Log("Start: " + Time.time);
                flagMove = true;
                goElapedTime = 0f;

            }

            t = Mathf.Min(backElapsedTime / backMoveTime, 1f);
            leapt = (t * t) * (3f - (2f * t));
            transform.position = Vector3.Lerp(goalPosition, startPosition, leapt);
            backElapsedTime += Time.deltaTime;

        }
        //if (onCollision == true) Debug.Log("Object: Collision");
    }
}