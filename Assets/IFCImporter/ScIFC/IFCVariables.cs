using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IFCVariables : MonoBehaviour
{

    [System.Serializable]
    public struct IfcVar
    {
        public string key;
        public string value;
    }

    public IfcVar[] vars;
}

