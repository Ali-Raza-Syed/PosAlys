using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class runLiveKFW : runLive
{
    private bool m_isKFWFingers = false;


    override public void Start()
    {
        m_isMoveFloor = true;

        // Parse header
        string Line = "# <REAL WORLD METERS (x, y ,z)>: SpineBase, SpineMid, Neck, Head, ShoulderLeft, ElbowLeft, WristLeft, HandLeft, ShoulderRight, ElbowRight, Unknown, HandRight, HipLeft, KneeLeft, AnkleLeft, FootLeft, HipRight, KneeRight, AnkleRight, FootRight, SpineShoulder, HandTipLeft, ThumbLeft, HandTipRight, ThumbRight";
        // First tokenize by :
        string[] LRHead = Line.Split(':');

        // Next, parse right part of the header
        string[] FormatString = LRHead[1].Split(',');

        m_isKFWFingers = false;
        Debug.Log("Format joints length is " + FormatString.Length);
        int JSize = FormatString.Length;
        if (m_isKFWFingers == false)
            JSize = JSize - 4; // Ignore fingertips

        m_JointSpheres = new GameObject[JSize];
        for (int i = 0; i < m_JointSpheres.Length; ++i)
        {
            m_JointSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            m_WoodMatRef = Resources.Load("wood_Texture", typeof(Material)) as Material; // loads from Assests/Resources directory
            if (m_WoodMatRef != null)
                m_JointSpheres[i].GetComponent<Renderer>().material = m_WoodMatRef;
            else
            {
                Debug.Log("Wood texture not assigned, will draw red.");
                m_JointSpheres[i].GetComponent<Renderer>().material.color = Color.red;
            }

            // Size of spheres
            float SphereRadius = 0.05f;
            m_JointSpheres[i].transform.localScale = new Vector3(SphereRadius, SphereRadius, SphereRadius);
        }

        // Next up create ellipsoids for bones
        int nBones = 21;
        if (m_isKFWFingers == false)
            nBones = nBones - 4;
        m_Bones = new GameObject[nBones];
        for (int i = 0; i < nBones; ++i)
        {
            m_Bones[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (m_WoodMatRef != null)
                m_Bones[i].GetComponent<Renderer>().material = m_WoodMatRef;
            else
            {
                Debug.Log("Wood texture not assigned, will draw red.");
                m_Bones[i].GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }

    override public void Update(string Line)
    {
        //Debug.Log(Line);
        //Debug.Log("Line length: " + Line.Length);
        if (Line.Length == 0)
            return;

        // Parse line
        string[] Tokens = Line.Split(',');
        //Debug.Log("Detected " + Tokens.Length / 3 + " joints.");
        float LowestY = 0.0f;
        Vector3[] Joints = new Vector3[Tokens.Length / 3];
        for (int i = 0; i < m_JointSpheres.Length; ++i)
        {
            Joints[i].x = -float.Parse(Tokens[3 * i + 0]); // Prevent mirroring
            Joints[i].y = -float.Parse(Tokens[3 * i + 1]); // Flip y-axis for Unity
            Joints[i].z = -float.Parse(Tokens[3 * i + 2]); // Flip z-axis for Google VR

            if (Joints[i].y < LowestY)
                LowestY = Joints[i].y;

            //Debug.Log(Joints[i]);
            m_JointSpheres[i].transform.position = Joints[i];
        }

        // Hide some spheres
        if (m_isKFWFingers == false)
        {
            m_JointSpheres[7].GetComponent<MeshRenderer>().enabled = false;
            m_JointSpheres[11].GetComponent<MeshRenderer>().enabled = false;
        }

        // Make floor stick to bottom-most joint (at index 16 or 20)
        GameObject Plane = GameObject.Find("CheckerboardPlane");
        if (Plane != null)
        {
            float PlaneFootBuffer = 0.02f;
            Vector3 OrigPos = Plane.transform.position;

            if (m_isMoveFloor)
                Plane.transform.position = new Vector3(OrigPos[0], LowestY - PlaneFootBuffer, OrigPos[2]);

            // Move camera with checkerboard plane
            if (m_isVRMode)
            {
                GameObject Head = GameObject.Find("Main Camera");
                Head.transform.position = Joints[3];
                Head.transform.rotation = GvrViewer.Controller.Head.transform.rotation;
            }
            else
            {
                GameObject FollowerCamera = GameObject.Find("ExternalCamera");
                if (FollowerCamera != null)
                {
                    OrigPos = FollowerCamera.transform.position;
                    FollowerCamera.transform.position = new Vector3(OrigPos[0], Plane.transform.position.y + 1, OrigPos[2]);
                }
            }
        }

        //0-SpineBase, 1-SpineMid
        drawEllipsoid(Joints[0], Joints[1], m_Bones[0]);
        //1-SpineMid, 2-Neck
        drawEllipsoid(Joints[1], Joints[2], m_Bones[1]);
        //2-Neck, 3-Head
        drawEllipsoid(Joints[2], Joints[3], m_Bones[2]);

        //20-SpineShoulder, 4-ShoulderLeft
        drawEllipsoid(Joints[20], Joints[4], m_Bones[3]);
        //4-ShoulderLeft, 5-ElbowLeft
        drawEllipsoid(Joints[4], Joints[5], m_Bones[4]);
        //5-ElbowLeft, 6-WristLeft
        drawEllipsoid(Joints[5], Joints[6], m_Bones[5]);
        //6-WristLeft, 7-HandLeft
        if (m_isKFWFingers == true)
            drawEllipsoid(Joints[6], Joints[7], m_Bones[6]);
        else
            m_Bones[6].GetComponent<MeshRenderer>().enabled = false;

        //20-SpineShoulder, 8-ShoulderRight
        drawEllipsoid(Joints[20], Joints[8], m_Bones[7]);
        //8-ShoulderRight, 9-ElbowRight
        drawEllipsoid(Joints[8], Joints[9], m_Bones[8]);
        //9-ElbowRight, 10-Unknown
        drawEllipsoid(Joints[9], Joints[10], m_Bones[9]);
        //10-Unknown, 11-HandRight
        if (m_isKFWFingers == true)
            drawEllipsoid(Joints[10], Joints[11], m_Bones[10]);
        else
            m_Bones[10].GetComponent<MeshRenderer>().enabled = false;

        //12-HipLeft, 13-KneeLeft
        drawEllipsoid(Joints[12], Joints[13], m_Bones[11]);
        //13-KneeLeft, 14-AnkleLeft
        drawEllipsoid(Joints[13], Joints[14], m_Bones[12]);
        //14-AnkleLeft, 15-FootLeft
        //drawEllipsoid(Joints[14], Joints[15], m_Bones[13]);

        //16-HipRight, 17-KneeRight
        drawEllipsoid(Joints[16], Joints[17], m_Bones[14]);
        //17-KneeRight, 18-AnkleRight
        drawEllipsoid(Joints[17], Joints[18], m_Bones[15]);
        //18-AnkleRight, 19-FootRight
        //drawEllipsoid(Joints[18], Joints[19], m_Bones[16]);

        // Disable toe sphere
        m_JointSpheres[18].GetComponent<MeshRenderer>().enabled = false;
        m_JointSpheres[19].GetComponent<MeshRenderer>().enabled = false;
        m_JointSpheres[14].GetComponent<MeshRenderer>().enabled = false;
        m_JointSpheres[15].GetComponent<MeshRenderer>().enabled = false;
        m_Bones[13].GetComponent<MeshRenderer>().enabled = false;
        m_Bones[16].GetComponent<MeshRenderer>().enabled = false;

        // Draw mesh
        RFoot.transform.rotation = Quaternion.LookRotation((Joints[19] - Joints[18]).normalized);
        RFoot.transform.rotation = Quaternion.Euler(RFoot.transform.eulerAngles + new Vector3(140, 0, 0));
        RFoot.transform.position = Joints[18];

        // Rotate z-axis to align with bone vector
        LFoot.transform.rotation = Quaternion.LookRotation((Joints[15] - Joints[14]).normalized);
        LFoot.transform.rotation = Quaternion.Euler(LFoot.transform.eulerAngles + new Vector3(140, 0, 0));
        // Position at middle
        LFoot.transform.position = Joints[14];

        if (m_isKFWFingers == true)
        {
            //7-HandLeft, 21-HandTipLeft
            drawEllipsoid(Joints[7], Joints[21], m_Bones[17]);
            //7-HandLeft, 22-ThumbLeft
            drawEllipsoid(Joints[7], Joints[22], m_Bones[18]);

            //11-HandRight, 23-HandTipRight
            drawEllipsoid(Joints[11], Joints[23], m_Bones[19]);
            //11-HandRight, 24-ThumbRight
            drawEllipsoid(Joints[11], Joints[24], m_Bones[20]);
        }
    }
}
