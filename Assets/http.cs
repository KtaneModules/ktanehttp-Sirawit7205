﻿using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class http : MonoBehaviour {

    public KMAudio Audio;
    public KMSelectable[] btn;
    public KMBombInfo Info;
    public TextMesh screen;
    public string[] texts = {"Cont","SwPrt","Proc","OK","Crtd","Mvperm","Found","UMod","UPrx","Bad R",
                             "Unauth","PayReq","Frbd","NFnd","Tm-out","Gone","ImaTp","SrvErr","BGw","SrvUn",
                             "Stk","Dtn","Ps","Prs","Cmd","Rls","Blue","Grn","Red","Yel","Blk","Wht",
                             "Cut","Uns","Set","Loc","Indc","Batt","Tmr","Lwr","Upr","Lvr","Time","Done"};
    public int[] respcode = { 100, 101, 102, 200, 201, 301, 302, 304, 305, 400, 401, 402, 403, 404, 408, 410, 418, 500, 502, 503,
                              601, 602, 603, 604, 605, 606, 701, 702, 703, 704, 705, 706, 801, 802, 803, 804, 805, 806, 901, 902, 903, 904, 905, 906 };
    public bool _isAwake = false, _isQuery = false;
    public int response, adder=0;
    public string current = null;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
    }

    private void Awake()
    {
        GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
        GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
        GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
        btn[0].OnInteract += delegate ()
        {
            HandlePress(0);
            return false;
        };
        btn[1].OnInteract += delegate ()
        {
            HandlePress(1);
            return false;
        };
        btn[2].OnInteract += delegate ()
        {
            HandlePress(2);
            return false;
        };
        btn[3].OnInteract += delegate ()
        {
            HandlePress(3);
            return false;
        };
        btn[4].OnInteract += delegate ()
        {
            HandlePress(4);
            return false;
        };
        btn[5].OnInteract += delegate ()
        {
            HandlePress(5);
            return false;
        };
        btn[6].OnInteract += delegate ()
        {
            HandlePress(6);
            return false;
        };
        btn[7].OnInteract += delegate ()
        {
            HandlePress(7);
            return false;
        };
        btn[8].OnInteract += delegate ()
        {
            HandlePress(8);
            return false;
        };
        btn[9].OnInteract += delegate ()
        {
            HandlePress(9);
            return false;
        };
    }

    protected void OnNeedyActivation()
    {
        if(!_isQuery)
        {
            int numbatt = 0, sum = 0;
            string serialno = null;
            foreach (string batteryInfo in Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null))
                numbatt += JsonConvert.DeserializeObject<Dictionary<string, int>>(batteryInfo)["numbatteries"];
            foreach (string serialNumberInfo in Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null))
                serialno = JsonConvert.DeserializeObject<Dictionary<string, string>>(serialNumberInfo)["serial"];
            for(int i=0;i<6;i++)
            {
                if (serialno[i] >= 48 && serialno[i] <= 57) sum += serialno[i] - 48;
            }
            adder = numbatt * sum;
            _isQuery = true;
            Debug.LogFormat("[NeedyHTTP #{0}] Table 2 adder calculated = {1}",_moduleId, adder);
        }
        int code = Random.Range(0, 44);
        screen.text = texts[code];
        response = respcode[code];
        if (code >= 20) response += adder;
        _isAwake = true;
        Debug.LogFormat("[NeedyHTTP #{0}] Selected code = {1}, Expected response = {2}", _moduleId, screen.text, response);
    }

    protected void OnNeedyDeactivation()
    {
        screen.text = "";
        exitfunc();
    }

    protected void OnTimerExpired()
    {
        GetComponent<KMNeedyModule>().OnStrike();
    }

    void HandlePress(int b)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn[b].transform);
        btn[b].AddInteractionPunch();
        if (!_isAwake)
        {
            current = null;
            GetComponent<KMNeedyModule>().OnStrike();
            Debug.LogFormat("[NeedyHTTP #{0}] Strike! Press on deactivated!",_moduleId);
            return;
        }
        current += b.ToString();
        if(current.Length == 3)
        {
            Debug.LogFormat("[Needy HTTP #{0}] Entered = {1}, Expected = {2}", _moduleId, current, response);
            if (current == response.ToString())
            {
                GetComponent<KMNeedyModule>().OnPass();
                Debug.LogFormat("[NeedyHTTP #{0}] Answer correct! Module passed!", _moduleId);
                exitfunc();
            }
            else
            {
                GetComponent<KMNeedyModule>().OnStrike();
                Debug.LogFormat("[NeedyHTTP #{0}] Answer incorrect! Strike!", _moduleId);
            }
            current = null;
        }
    }

    void exitfunc()
    {
        screen.text = "";
        _isAwake = false;
        Debug.LogFormat("[NeedyHTTP #{0}] Module deactivated.",_moduleId);
    }
}
