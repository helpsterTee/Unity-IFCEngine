#region License
/** Copyright(c) 2017 helpsterTee (https://github.com/helpsterTee)
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/
#endregion

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using IfcEngineWrapper;
using System;
using UnityEditor;
using System.Threading;
using CielaSpike;

#if _WIN64
using int_t = System.Int64;
#else
using int_t = System.Int32;
#endif

public class ImportIFC : MonoBehaviour {

    List<IfcItem> items;
    Material initMaterial;

    bool isImporting = false;
    bool allFinished = false;

    List<Mesh> meshes = new List<Mesh>();
    GameObject go;

    double Latitude = 0;
    double Longitude = 0;
    double Elevation = 0;
    public double[] GetLatLonEle()
    {
        return new double[] { Latitude, Longitude, Elevation };
    }

    /* delegates */
    public delegate void CallbackEventHandler(GameObject go);
    public event CallbackEventHandler ImportFinished;

    /* public editor assignable variables */
    public MaterialAssignment MaterialAssignment;

    private Dictionary<String, Material> classToMat = new Dictionary<string, Material>();

    // Use this for initialization
    public void Init()
    {
        initMaterial = Resources.Load("IFCDefault", typeof(Material)) as Material;

        /* prepare material assignment */
        if (MaterialAssignment != null)
        {
            for (int i = 0; i < MaterialAssignment.MaterialDB.Length; ++i)
            {
                IFCMaterialAssoc mas = MaterialAssignment.MaterialDB[i];
                classToMat.Add(mas.IFCClass, mas.Material);
            }
        }
    }

    public void ImportFile(string path, string name)
    {
        this.StartCoroutineAsync(Import(path, name));
    }

    IEnumerator Import(string path, string objname) {
        IfcUtil util = new IfcUtil();

        string name = objname;
        string file = path;

        Debug.Log(file);

        float projectScale = 1.0f;

        yield return Ninja.JumpToUnity;
        if (GameObject.Find(name) != null)
        {
            Debug.Log("GameObject already exists, aborting import!");
            yield return null;
        }
        yield return Ninja.JumpBack;

        Debug.Log("Parsing geometry from IFC file");
        yield return null;
        bool result = util.ParseIFCFile(file);

        if (!result)
        {
            Debug.Log("Error parsing IFC File");
            yield return null;
        } else
        {
            Debug.Log("Finished parsing geometry");
            if (util.Latitude != 0 && util.Longitude != 0)
            {
                Debug.Log("Found georeference with coordinates:" + util.Latitude.ToString() + "," + util.Longitude.ToString());
                Latitude = util.Latitude;
                Longitude = util.Longitude;
                Elevation = util.Elevation;
            } else
            {
                Debug.Log("Found no georeference");
            }

            if (util.SILengthUnit != null)
            {
                Debug.Log("Project file is measured in " + util.SILengthUnit);
                if (util.SILengthUnit.Equals(".MILLI..METRE."))
                {
                    projectScale = 1/1000.0f;
                } else if (util.SILengthUnit.Equals(".CENTI..METRE.")){
                    projectScale = 1/100.0f;
                } else if (util.SILengthUnit.Equals(".DECI..METRE."))
                {
                    projectScale = 1/10.0f;
                }
            }
            
            yield return null;
        }

        //okay here
        items = util.Geometry;

        // calculate dimensions
        Vector3 min = new Vector3();
        Vector3 max = new Vector3();
        bool InitMinMax = false;
        GetDimensions(util.ModelRoot, ref min, ref max, ref InitMinMax);

        Vector3 center = new Vector3();
        center.x = (max.x + min.x) / 2f;
        center.y = (max.y + min.y) / 2f;
        center.z = (max.z + min.z) / 2f;

        center *= projectScale;

        float size = max.x - min.x;

        if (size < max.y - min.y) size = max.y - min.y;
        if (size < max.z - min.z) size = max.z - min.z;

        yield return Ninja.JumpToUnity;
        go = new GameObject();
        go.name = name;
        yield return Ninja.JumpBack;

        int cnt = 0;
        foreach (IfcItem item in items)
        {
            if (cnt % 50 == 0)
            {
                Debug.Log("Processing mesh " + cnt + " of " + items.Count);
                yield return null;
            }

            yield return Ninja.JumpToUnity;
            Mesh m = new Mesh();
            m.name = item.ifcType;
            yield return Ninja.JumpBack;
            List<Vector3> vertices = new List<Vector3>();
            for(int i=0; i<item.verticesCount; i++)
            {
                Vector3 vec = new Vector3((item.vertices[6 * i + 0]) * projectScale, (item.vertices[6 * i + 2]) * projectScale, (item.vertices[6 * i + 1]) * projectScale);
                vertices.Add(vec);
            }
            Debug.Assert(item.vertices.Length == item.verticesCount * 6);

            yield return Ninja.JumpToUnity;
            m.SetVertices(vertices);
            m.SetIndices(item.indicesForFaces, MeshTopology.Triangles, 0, true);

            // calculate UVs
            float scaleFactor = 0.5f;
            Vector2[] uvs = new Vector2[vertices.Count];
            int len = m.GetIndices(0).Length;
            int_t[] idxs = m.GetIndices(0);
            yield return Ninja.JumpBack;
            for (int i=0; i<len; i=i+3)
            {
                Vector3 v1 = vertices[idxs[i + 0]];
                Vector3 v2 = vertices[idxs[i + 1]];
                Vector3 v3 = vertices[idxs[i + 2]];
                Vector3 normal = Vector3.Cross(v3 - v1, v2 - v1);
                Quaternion rotation;
                if (normal == Vector3.zero)
                    rotation = new Quaternion();
                else
                    rotation = Quaternion.Inverse(Quaternion.LookRotation(normal));
                uvs[idxs[i + 0]] = (Vector2)(rotation * v1) * scaleFactor;
                uvs[idxs[i + 1]] = (Vector2)(rotation * v2) * scaleFactor;
                uvs[idxs[i + 2]] = (Vector2)(rotation * v3) * scaleFactor;
            }
            yield return Ninja.JumpToUnity;
            m.SetUVs(0, new List<Vector2>(uvs));
            m.RecalculateNormals();
            yield return Ninja.JumpBack;
            meshes.Add(m);
            
            cnt++;
        }

        cnt = 0;
        yield return Ninja.JumpToUnity;
        foreach (Mesh m in meshes)
        {
            Material mat = initMaterial;

            /* check if materials assigned, otherwise use init mat */
            if (classToMat.ContainsKey(m.name))
            {
                classToMat.TryGetValue(m.name, out mat);
            }

            GameObject child = new GameObject(m.name + cnt);
            child.transform.parent = go.transform;
            MeshFilter meshFilter = (MeshFilter)child.AddComponent(typeof(MeshFilter));
            meshFilter.mesh = m;
            MeshRenderer renderer = child.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

            renderer.material = mat;

            child.AddComponent(typeof(SerializeMesh));

            cnt++;
            yield return null;
        }

        if (util.TrueNorth != 0)
        {
            go.transform.Rotate(new Vector3(0, 1, 0), (float)util.TrueNorth*-1);
        }

        PrefabUtility.CreatePrefab("Assets/IFCGeneratedGeometry/" + name + ".prefab", go);

        allFinished = true;
        
        // callback for external
        if (ImportFinished != null)
        {
            ImportFinished(go);
        }

        yield return null;
    }

    #region helper methods

    private void GetDimensions(IfcItem ifcItem, ref Vector3 min, ref Vector3 max, ref bool InitMinMax)
    {
        while (ifcItem != null)
        {
            if (ifcItem.verticesCount != 0)
            {
                if (InitMinMax == false)
                {
                    min.x = ifcItem.vertices[3 * 0 + 0];
                    min.y = ifcItem.vertices[3 * 0 + 2];
                    min.z = ifcItem.vertices[3 * 0 + 1];
                    max = min;

                    InitMinMax = true;
                }

                int_t i = 0;
                while (i < ifcItem.verticesCount)
                {

                    min.x = Math.Min(min.x, ifcItem.vertices[6 * i + 0]);
                    min.y = Math.Min(min.y, ifcItem.vertices[6 * i + 2]);
                    min.z = Math.Min(min.z, ifcItem.vertices[6 * i + 1]);

                    max.x = Math.Max(max.x, ifcItem.vertices[6 * i + 0]);
                    max.y = Math.Max(max.y, ifcItem.vertices[6 * i + 2]);
                    max.z = Math.Max(max.z, ifcItem.vertices[6 * i + 1]);

                    i++;
                }
            }

            GetDimensions(ifcItem.child, ref min, ref max, ref InitMinMax);

            ifcItem = ifcItem.next;
        }
    }

    #endregion
}
