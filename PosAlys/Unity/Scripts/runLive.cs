using UnityEngine;
using System.IO;
using System.Collections.Generic;

abstract public class runLive
{
    public GameObject[] m_JointSpheres;
    public GameObject[] m_Bones;
    public Material m_WoodMatRef;

    public bool m_isVRMode = false;
    public bool m_isMoveFloor = false;

    public GameObject LFoot;
    public GameObject RFoot;

    abstract public void Start();
    abstract public void Update(string Line);
    public void recenter()
    {
        if (m_isVRMode)
            GvrViewer.Instance.Recenter();
    }

    protected void drawEllipsoid(Vector3 Start, Vector3 End, GameObject Bone)
    {
        // Go to unit sphere
        Bone.transform.position = Vector3.zero;
        Bone.transform.rotation = Quaternion.identity;
        Bone.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        Vector3 BoneVec = End - Start;

        // Set z-axis of sphere to align with bone
        float zScale = BoneVec.magnitude * 0.95f;
        float xyScale = zScale * 0.3f;
        Bone.transform.localScale = new Vector3(xyScale, xyScale, zScale);

        // Rotate z-axis to align with bone vector
        if (BoneVec.magnitude > 0.00001)
        {
            Bone.transform.rotation = Quaternion.LookRotation(BoneVec.normalized);
            // Position at middle
            Bone.transform.position = (Start + End) / 2.0f;
        }
    }
}
