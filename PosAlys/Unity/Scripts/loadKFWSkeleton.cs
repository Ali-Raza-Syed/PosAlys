using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class loadKFWSkeleton : MonoBehaviour
{

    private StreamReader m_3DPoseFileStream;

    // For dynamic rendering
    private GameObject[] m_JointSpheres;
    private GameObject[] m_Bones;
    public Material m_WoodMatRef;
    public GameObject LFoot;
    public GameObject RFoot;
    private Vector3 m_CameraOffset;

    private bool m_isKFWFingers = false;
    private int m_FrameCtr = 0;
    private bool m_isValid = false;
    private bool m_isVRMode = false;
    private bool m_isMoveFloor = false;

    private bool m_isVNECTMode = true;
    private List<int> m_ValidJointIdx;

    private string m_SequenceName;

    // Use this for initialization
    void Start()
    {
        //Screen.SetResolution(1920, 1080, true);
        Application.runInBackground = true;

        m_isVRMode = false;
        m_isMoveFloor = true;
        m_isVNECTMode = true;

        if (m_isVNECTMode)
            m_SequenceName = "Football_2";
        else
            m_SequenceName = "DCorridor_KFW";
        FileStream fs = new FileStream(UnityEngine.Application.streamingAssetsPath + "/" + m_SequenceName + ".txt", FileMode.Open, FileAccess.Read, FileShare.Read);
        m_3DPoseFileStream = new StreamReader(fs);
        if (m_3DPoseFileStream == null)
            Debug.Log("Unable to load pose file. Check path.");

        m_FrameCtr = 0;

        // Print header
        string Line = m_3DPoseFileStream.ReadLine();

        if (m_isVNECTMode)
        {
            // Parse header
            // First tokenize by :
            string[] LRHead = Line.Split(':');

            // Next, parse right part of the header
            string[] FormatString = LRHead[1].Split(',');

            m_isKFWFingers = false;
            Debug.Log("Format joints length is " + FormatString.Length);

            // Print joints: 29
            // 5-root_rx, 8-spine_3_rx, 11-spine_4_rx, 14-spine_2_rx, 17-spine_1_rx, 20-neck_1_rx, 21-head_ee_ry, 23-left_clavicle_rz, 26-left_shoulder_ry, 27-left_elbow_rx, 28-left_lowarm_twist, 30-left_hand_ry, 31-left_ee_rx, 33-right_clavicle_rz, 36-right_shoulder_ry, 37-right_elbow_rx, 38-right_lowarm_twist, 40-right_hand_ry, 41-right_ee_rx, 44-left_hip_ry, 45-left_knee_rx, 47-left_ankle_ry, 48-left_toes_rx, 49-left_foot_ee, 52-right_hip_ry, 53-right_knee_rx, 55-right_ankle_ry, 56-right_toes_rx, 57-right_foot_ee
            int JSize = 29;
            m_ValidJointIdx = new List<int>();
            m_ValidJointIdx.Add(5);
            m_ValidJointIdx.Add(8);
            m_ValidJointIdx.Add(11);
            m_ValidJointIdx.Add(14);
            m_ValidJointIdx.Add(17);
            m_ValidJointIdx.Add(20);
            m_ValidJointIdx.Add(21);
            m_ValidJointIdx.Add(23);
            m_ValidJointIdx.Add(26);
            m_ValidJointIdx.Add(27);
            m_ValidJointIdx.Add(28);
            m_ValidJointIdx.Add(30);
            m_ValidJointIdx.Add(31);
            m_ValidJointIdx.Add(33);
            m_ValidJointIdx.Add(36);
            m_ValidJointIdx.Add(37);
            m_ValidJointIdx.Add(38);
            m_ValidJointIdx.Add(40);
            m_ValidJointIdx.Add(41);
            m_ValidJointIdx.Add(44);
            m_ValidJointIdx.Add(45);
            m_ValidJointIdx.Add(47);
            m_ValidJointIdx.Add(48);
            m_ValidJointIdx.Add(49);
            m_ValidJointIdx.Add(52);
            m_ValidJointIdx.Add(53);
            m_ValidJointIdx.Add(55);
            m_ValidJointIdx.Add(56);
            m_ValidJointIdx.Add(57);

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
            int nBones = 28;
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
        else
        {
            // Parse header
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
    }

    // Update is called once per frame
    void Update()
    {
        string Line = m_3DPoseFileStream.ReadLine();
        Debug.Log(Line);
        m_isValid = false;

        if (Line.Length == 0)
            return;

        m_isValid = true;

        if (m_isVNECTMode)
        {
            // Parse line
            int ParseOffset = 1; // First is frame number
            string[] Tokens = Line.Split(',');
            //Debug.Log("Detected " + (Tokens.Length - ParseOffset) / 3 + " joints.");
            if ((Tokens.Length - ParseOffset) / 3 != 58)
                return;
            float LowestY = 0.0f;
            Vector3[] Joints = new Vector3[(Tokens.Length - ParseOffset) / 3];
            for (int i = 0; i < m_JointSpheres.Length; ++i)
            {
                int Idx = m_ValidJointIdx[i];
                Joints[i].x = float.Parse(Tokens[3 * Idx + 0 + ParseOffset]) * 0.001f;
                Joints[i].y = float.Parse(Tokens[3 * Idx + 1 + ParseOffset]) * 0.001f;
                Joints[i].z = float.Parse(Tokens[3 * Idx + 2 + ParseOffset]) * 0.001f;

                if (Joints[i].y < LowestY)
                    LowestY = Joints[i].y;

                //Debug.Log(Joints[i]);
                //m_JointSpheres[i].transform.position = Vector3.Lerp(m_JointSpheres[i].transform.position, Joints[i], 0.2f);
                //Joints[i] = m_JointSpheres[i].transform.position; // Hack
                m_JointSpheres[i].transform.position = Joints[i];
            }

            // Make floow stick to bottom-most joint (at index 16 or 20)
            GameObject Plane = GameObject.Find("CheckerboardPlane");
            if (Plane != null)
            {
                float PlaneFootBuffer = 0.02f;
                Vector3 OrigPos = Plane.transform.position;

                if(m_isMoveFloor)
                    Plane.transform.position = new Vector3(OrigPos[0], LowestY - PlaneFootBuffer, OrigPos[2]);

                GameObject FollowerCamera = GameObject.Find("FollowerCamera");
                if (m_isVRMode)
                {
                    // Align VR head pose with character
                    FollowerCamera.transform.position = Joints[6];
                    FollowerCamera.transform.rotation = GvrViewer.Controller.Head.transform.rotation;
                }
                else
                {
                    // Move camera with checkerboard plane
                    OrigPos = FollowerCamera.transform.position;
                    FollowerCamera.transform.position = new Vector3(OrigPos[0], Plane.transform.position.y + 1, OrigPos[2]);
                }
            }

            //0-root_rx, 1-spine_3_rx
            drawEllipsoid(Joints[0], Joints[1], m_Bones[0]);
            //1-spine_3_rx, 2-spine_4_rx
            drawEllipsoid(Joints[1], Joints[2], m_Bones[1]);
            //0-root_rx, 3-spine_2_rx
            drawEllipsoid(Joints[0], Joints[3], m_Bones[2]);
            //3-spine_2_rx, 4-spine_1_rx
            drawEllipsoid(Joints[3], Joints[4], m_Bones[3]);
            //2-spine_4_rx, 5-neck_1_rx
            drawEllipsoid(Joints[2], Joints[5], m_Bones[4]);
            //5-neck_1_rx, 6-head_ee_ry
            drawEllipsoid(Joints[5], Joints[6], m_Bones[5]);

            //2-spine_4_rx, 7-left_clavicle_rz
            drawEllipsoid(Joints[2], Joints[7], m_Bones[6]);
            //7-left_clavicle_rz, 8-left_shoulder_ry
            drawEllipsoid(Joints[7], Joints[8], m_Bones[7]);
            //8-left_shoulder_ry, 9-left_elbow_rx
            drawEllipsoid(Joints[8], Joints[9], m_Bones[8]);
            //9-left_elbow_rx, 10-left_lowarm_twist
            drawEllipsoid(Joints[9], Joints[10], m_Bones[9]);
            //10-left_lowarm_twist, 11-left_hand_ry
            drawEllipsoid(Joints[10], Joints[11], m_Bones[10]);
            //11-left_hand_ry, 12-left_ee_rx
            drawEllipsoid(Joints[11], Joints[12], m_Bones[11]);

            //2-spine_4_rx, 13-right_clavicle_rz
            drawEllipsoid(Joints[2], Joints[13], m_Bones[12]);
            //13-right_clavicle_rz, 14-right_shoulder_ry
            drawEllipsoid(Joints[13], Joints[14], m_Bones[13]);
            //14-right_shoulder_ry, 15-right_elbow_rx
            drawEllipsoid(Joints[14], Joints[15], m_Bones[14]);
            //15-right_elbow_rx, 16-right_lowarm_twist
            drawEllipsoid(Joints[15], Joints[16], m_Bones[15]);
            //16-right_lowarm_twist, 17-right_hand_ry
            drawEllipsoid(Joints[16], Joints[17], m_Bones[16]);
            //17-right_hand_ry, 18-right_ee_rx
            drawEllipsoid(Joints[17], Joints[18], m_Bones[17]);

            //4-spine_1_rx, 19-left_hip_ry
            drawEllipsoid(Joints[4], Joints[19], m_Bones[18]);
            //19-left_hip_ry, 20-left_knee_rx
            drawEllipsoid(Joints[19], Joints[20], m_Bones[19]);
            //20-left_knee_rx, 21-left_ankle_ry
            drawEllipsoid(Joints[20], Joints[21], m_Bones[20]);
            //21-left_ankle_ry, 22-left_toes_rx
            drawEllipsoid(Joints[21], Joints[22], m_Bones[21]);
            //22-left_toes_rx, 23-left_foot_ee
            //drawEllipsoid(Joints[22], Joints[23], m_Bones[22]);

            //4-spine_1_rx, 24-right_hip_ry
            drawEllipsoid(Joints[4], Joints[24], m_Bones[23]);
            //24-right_hip_ry, 25-right_knee_rx
            drawEllipsoid(Joints[24], Joints[25], m_Bones[24]);
            //25-right_knee_rx, 26-right_ankle_ry
            drawEllipsoid(Joints[25], Joints[26], m_Bones[25]);
            //26-right_ankle_ry, 27-right_toes_rx
            drawEllipsoid(Joints[26], Joints[27], m_Bones[26]);
            //27-right_toes_rx, 28-right_foot_ee
            //drawEllipsoid(Joints[27], Joints[28], m_Bones[27]);
        }
        else
        {
            // Parse line
            string[] Tokens = Line.Split(',');
            //Debug.Log("Detected " + Tokens.Length / 3 + " joints.");
            float LowestY = 0.0f;
            Vector3[] Joints = new Vector3[Tokens.Length / 3];
            for (int i = 0; i < m_JointSpheres.Length; ++i)
            {
                Joints[i].x = float.Parse(Tokens[3 * i + 0]);
                Joints[i].y = -float.Parse(Tokens[3 * i + 1]); // Fip y-axis
                Joints[i].z = float.Parse(Tokens[3 * i + 2]);

                if (Joints[i].y < LowestY)
                    LowestY = Joints[i].y;

                //Debug.Log(Joints[i]);
                m_JointSpheres[i].transform.position = Joints[i];
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
                GameObject FollowerCamera = GameObject.Find("FollowerCamera");
                if (m_isVRMode)
                {
                    FollowerCamera.transform.position = Joints[3];
                    FollowerCamera.transform.rotation = GvrViewer.Controller.Head.transform.rotation;
                }
                else
                {
                    OrigPos = FollowerCamera.transform.position;
                    FollowerCamera.transform.position = new Vector3(OrigPos[0], Plane.transform.position.y + 1, OrigPos[2]);
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
            drawEllipsoid(Joints[6], Joints[7], m_Bones[6]);

            //20-SpineShoulder, 8-ShoulderRight
            drawEllipsoid(Joints[20], Joints[8], m_Bones[7]);
            //8-ShoulderRight, 9-ElbowRight
            drawEllipsoid(Joints[8], Joints[9], m_Bones[8]);
            //9-ElbowRight, 10-Unknown
            drawEllipsoid(Joints[9], Joints[10], m_Bones[9]);
            //10-Unknown, 11-HandRight
            drawEllipsoid(Joints[10], Joints[11], m_Bones[10]);

            //12-HipLeft, 13-KneeLeft
            drawEllipsoid(Joints[12], Joints[13], m_Bones[11]);
            //13-KneeLeft, 14-AnkleLeft
            drawEllipsoid(Joints[13], Joints[14], m_Bones[12]);
            //14-AnkleLeft, 15-FootLeft
            //LFoot.position = Joints[14];
            //LFoot.transform.rotation = Quaternion.LookRotation((Joints[15] - Joints[14]).normalized);
            
            // Rotate z-axis to align with bone vector
            LFoot.transform.rotation = Quaternion.LookRotation((Joints[15] - Joints[14]).normalized);
            LFoot.transform.rotation = Quaternion.Euler(LFoot.transform.eulerAngles + new Vector3(90,0,0));
            

            // Position at middle
            LFoot.transform.position = Joints[14];
            //drawEllipsoid(Joints[14], Joints[15], LFoot);

            //16-HipRight, 17-KneeRight
            drawEllipsoid(Joints[16], Joints[17], m_Bones[14]);
            //17-KneeRight, 18-AnkleRight
            drawEllipsoid(Joints[17], Joints[18], m_Bones[15]);
            //18-AnkleRight, 19-FootRight
            //drawEllipsoid(Joints[18], Joints[19], m_Bones[16]);
            RFoot.transform.rotation = Quaternion.LookRotation((Joints[19] - Joints[18]).normalized);
            RFoot.transform.rotation = Quaternion.Euler(RFoot.transform.eulerAngles + new Vector3(90, 0, 0));
            RFoot.transform.position = Joints[18];
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

    public void drawEllipsoid(Vector3 Start, Vector3 End, GameObject Bone)
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
        Bone.transform.rotation = Quaternion.LookRotation(BoneVec.normalized);
        // Position at middle
        Bone.transform.position = (Start + End) / 2.0f;
    }

    void LateUpdate()
    {
        if (m_isValid == false)
            return;

        //string FileName = "D:/code/" + m_SequenceName + "/Capture" + m_FrameCtr.ToString().PadLeft(6, '0') + ".png";
        //Application.CaptureScreenshot(FileName);
        //m_FrameCtr++;
    }
}
