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

    public override string ToString()
    {
        string retstr = "########### IFC Variables ###########\n";
        foreach (IfcVar var in vars) {
            retstr += "\tkey: [" + var.key + "], value: [" + var.value + "]\n";
        }
        return retstr;
    }
}

