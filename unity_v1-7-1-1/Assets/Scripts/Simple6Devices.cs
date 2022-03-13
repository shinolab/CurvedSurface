using AUTD3Sharp;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;


public class Simple6Devices : MonoBehaviour
{
    AUTD _autd = new AUTD();

    Link _link = null;
    public GameObject Focus = null;
    public Transform[] transforms = null;

    void Start()
    {
        foreach (var transform in transforms)
        {
            //_autd.AddDevice(gameObject.transform.position, gameObject.transform.rotation);
            _autd.AddDevice(transform.position, transform.rotation);
        }

        string ifname = @"\\Device\\NPF_{425C764E-7B4C-44B7-A24D-60A34C403894}";
        _link = Link.SOEM(ifname, _autd.NumDevices);
        _autd.Open(_link);

        _autd.SilentMode = false;
        _autd.Send(Modulation.Sine(150)); // 150 Hz
    }

    void Update()
    {
        if (Focus != null)
        {
            _autd.Send(Gain.FocalPoint(Focus.transform.position));
        }
    }

    private void OnApplicationQuit()
    {
        _autd.Stop();
        _autd.Clear();
        _autd.Dispose();
    }
}
