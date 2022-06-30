using AUTD3Sharp;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;


public class Simple6Devices : MonoBehaviour
{
    Controller _autd = new Controller();

    Link _link = null;
    public Transform[] transforms = null;

    public GameObject? Target = null;

    void Start()
    {
        foreach (var transform in transforms)
        {
            //_autd.AddDevice(gameObject.transform.position, gameObject.transform.rotation);
            _autd.AddDevice(transform.position, transform.rotation);
        }

        string ifname = @"\\Device\\NPF_{425C764E-7B4C-44B7-A24D-60A34C403894}";
        _link = new SOEM(ifname, _autd.NumDevices);
        _autd.Open(_link);

        _autd.CheckAck = true;

        _autd.Clear();

        _autd.Synchronize();

        _autd.Send(new Sine(150)); // 150 Hz
    }

    void Update()
    {
        if (Target != null)
            _autd.Send(new Focus(Target.transform.position, 1.0));
    }

    private void OnApplicationQuit()
    {
        _autd.Stop();
        _autd.Clear();
        _autd.Dispose();
    }
}