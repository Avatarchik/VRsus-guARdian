﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ProximitySensor : TrapDefense
{
    public float nearRange = 3;
    public float farRange = 5;

    private float nearRangeSqr;
    private float farRangeSqr;

    private static Transform[] players;
    private static GameManager manager;

    public override string TrapName
    {
        get
        {
            return "Proximity Sensor";
        }
    }

    // Use this for initialization
    void Start()
    {
        if (farRange < nearRange)
        {
            float temp = farRange;
            farRange = nearRange;
            nearRange = temp;
        }

        nearRangeSqr = Mathf.Pow(nearRange, 2);
        transform.GetChild(0).localScale *= nearRangeSqr;

        farRangeSqr = Mathf.Pow(farRange, 2);
        transform.GetChild(1).localScale *= farRangeSqr;

#if !UNITY_IOS
        GetComponent<Renderer>().enabled = false;
        foreach (Collider c in GetComponents<Collider>())
        {
            if (!c.isTrigger)
            {
                c.enabled = false;
            }
        }
#endif
    }

    public override void OnStartServer()
    {
        if (players == null)
        {
            Player[] playerScripts = FindObjectsOfType<Player>();
            players = new Transform[1];
            for (int i = 0; i < players.Length; i++)
            {
                if (playerScripts[i].PlayerType == PlayerType.VR)
                    players[0] = playerScripts[i].transform;
            }
        }

        if (manager == null)
        {
            manager = FindObjectOfType<GameManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer || manager.CurrGamePhase != GamePhase.Playing)
            return;
        float closestDist = float.MaxValue;

        //this does not check for decoys
        foreach (Transform t in players)
        {
            if (t.position.y < transform.position.y - transform.localScale.y / 2)
                continue;

            float dist = (t.position - transform.position).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
            }
        }

        Color c = Color.black;
        float halfRange = (farRangeSqr - nearRangeSqr) / 2f;
        c.r = Mathf.Lerp(1, 0, (closestDist - nearRangeSqr) / halfRange);
        c.g = Mathf.Lerp(0, 1, (closestDist - nearRangeSqr + halfRange) / (halfRange * 2f));

        //Debug.Log(closestDist + ": " + c);
        GetComponent<Renderer>().material.color = c;
    }

    public override void ToggleSelected()
    {
        base.ToggleSelected();

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = selected;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, nearRangeSqr);


        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, farRangeSqr);

    }
}