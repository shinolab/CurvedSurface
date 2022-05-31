using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

public class BumpsEuroDemo3 : MonoBehaviour
{
    /******************************************************
    *               変数定義
    *******************************************************/
    private ManagerEuroDemo3 _manager;  // manager

    // Base Condition
    private float _delay    = 0.0f;  // bump
    private float _halfCurveLength;  // curve ength
    private float _halfExploreLength;
    private float _fingerRadius;
    Vector3 _planeYZ;

    /*------------------------------------------------------
     *             Bumpの状態
     * ---------------------------------------------------*/
    float _bumpRadius;    // コブの半径
    float _bumpDiameter;  // コブの直径 いる?
    float _bumpPosX = 0f; // コブのx座標
    float _bumpPosY = 0f; // コブのy座標
    int   _bumpCount = 0; // コブの個数
    float _eachBumpDistance; // 互いに離れたコブの距離

    /*------------------------------------------------------
     *             提示法の種類
     * ---------------------------------------------------*/
    bool _flagMode = true;   //true -> Adjusting, false -> Presenting
    bool _flagLM   = true;   //true -> Strength, false -> Position

    // Bump Object
    public GameObject LeftBump = null;
    //public GameObject RightBump = null;
    /******************************************************
    *              プロパティ
    *******************************************************/
    #region Property
    //For CalcBump
    public float Delay
    {
        get { return this._delay; }
        private set { this._delay = value; }
    }
    public float HalfCurveLength
    {
        get { return this._halfCurveLength; }
        private set { this._halfCurveLength = value; }
    }
    
    public float HalfExploreLength
    {
        get { return this._halfExploreLength; }
        private set { this._halfExploreLength = value; }
    }
    public float BumpPosY
    {
        get          { return this._bumpPosY; }
        private set { this._bumpPosY = value; }
    }
    public float BumpPosX
    {
        get { return this._bumpPosX; }
        private set { this._bumpPosX = value; }
    }
    public int BumpNumber
    {
        get { return this._bumpCount; }
        private set { this._bumpCount = value; }
    }
    #endregion Property
    float curveHight;


    Vector3 bumpPos;

    //For Send Information to AUTD Controller

    float distance = 0;

   


    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerEuroDemo3>();

        _planeYZ      = _manager.PlaneYZ;
        _fingerRadius = _manager.FingerRadius;
        curveHight    = _manager.CurveHight;
        _bumpRadius   = _manager.BumpRadius[0];
        distance      = _manager.Distance;
    }

    // Update is called once per frame
    void Update()
    {
        /*------------------------------------------------------
         *             Set Flag
         * ---------------------------------------------------*/
        _flagMode = _manager.FlagMode;
        _flagLM = _manager.FlagLM;


        _bumpRadius = _manager.BumpRadius[0];
        distance = _manager.Distance;


        //If Adjusting and( Bigger or Smaller or Further or Closer)
        if (_flagMode == true && _manager.HasChangedValue)
        {

            CalcBump();
            DetectCenterBump();

            if ((distance > 0) && (_bumpRadius > 0))
            {
                PlaceBumps();
                Debug.Log("BumpRadius" + _bumpRadius);
            }

        }

    }

    private void CalcBump() //Calc bumpPosY
    {
        //Calc halfCurveLength, halfExploreLength
        if (2 * _bumpRadius * curveHight > Mathf.Pow(curveHight, 2))
        {
            _halfCurveLength = Mathf.Sqrt(2 * _bumpRadius * curveHight - Mathf.Pow(curveHight, 2));
        }
        else
        {
            _halfCurveLength = 0;
            Debug.Log("halfCurveLength=0");
        }

        _delay = _bumpRadius / (_bumpRadius + _fingerRadius);
        _halfExploreLength = _halfCurveLength / _delay;


        //Calc bumpPosY
        if (Mathf.Pow(_bumpRadius, 2) < Mathf.Pow(_halfCurveLength, 2)) // Inside of SQRT < 0  ->  Errpor
        {
            _bumpPosY = _planeYZ.y;
        }
        else
        {
            _bumpPosY = _planeYZ.y - Mathf.Sqrt(Mathf.Pow(_bumpRadius, 2) - Mathf.Pow(_halfCurveLength, 2));
        }
        curveHight = _bumpRadius - _planeYZ.y + _bumpPosY;

        Debug.Log("Bump, delay: " + _delay.ToString("F4") + "halfCurveLength" + _halfCurveLength.ToString("F4") + "planeYZ.y" + _planeYZ.y.ToString("F4") + "bumpRadius" + _bumpRadius.ToString("F4") + ", bumpPosY" + _bumpPosY.ToString("F4") + ", curvedHight" + curveHight.ToString("F8"));

    }


    private void DetectCenterBump()
    {

        if (_flagLM) //Strength && color =red
        {
            LeftBump.GetComponent<Renderer>().material.color = new Color32(162, 190, 255, 255); //blue
            //RightBump.GetComponent<Renderer>().material.color = new Color32(162, 190, 255, 255); //blue
        }
        else //Position && color = blue
        {
            LeftBump.GetComponent<Renderer>().material.color = new Color32(255, 162, 162, 255); //red
            //RightBump.GetComponent<Renderer>().material.color = new Color32(255, 162, 162, 255); //red
        }

        LeftBump.transform.localScale = new Vector3(_bumpRadius * 2, 0.14f, _bumpRadius * 2);
        //RightBump.transform.localScale = new Vector3(_bumpRadius * 2, 0.14f, _bumpRadius * 2);

        bumpPos = LeftBump.transform.position;
        bumpPos.y = _bumpPosY;
        LeftBump.transform.position = bumpPos;

        //bumpPos = RightBump.transform.position;
        //bumpPos.y = _bumpPosY;
        //RightBump.transform.position = bumpPos;
    }

    private void PlaceBumps()
    {
        int i;
        //Delete Objects
        if (_bumpCount > 0)
        {
            for (i = 0; i < _bumpCount; i++)
            {
                Destroy(GameObject.Find("right" + i));
                Destroy(GameObject.Find("left" + i));
            }
        }

        //Place
        /*------------------------------------------------------
         *             Bumpの個数を決定する
         * ---------------------------------------------------*/
        _bumpCount = (int)(0.173f / (distance)) + 1;
        /*------------------------------------------------------
         *             Bumpの生成
         * ---------------------------------------------------*/
        for (i = 0; i < _bumpCount; i++)
        {
            _eachBumpDistance = i * distance;

            GameObject rightBumps = Instantiate(LeftBump, new Vector3(LeftBump.transform.position.x + _eachBumpDistance, _bumpPosY, 0.06f), Quaternion.Euler(90f, 0.0f, 0f)); //Right
            GameObject leftBumps = Instantiate(LeftBump, new Vector3(LeftBump.transform.position.x - _eachBumpDistance, _bumpPosY, 0.06f), Quaternion.Euler(90f, 0.0f, 0f)); //Left

            rightBumps.name = "right" + i;
            leftBumps.name = "left" + i;
        }

        //Debug.Log("bumpNumber; " + bumpNumber + ", eachBumpDitance; " + eachBumpDitance.ToString("F4"));
    }



}
