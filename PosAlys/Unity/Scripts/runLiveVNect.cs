using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public class runLiveVNect : runLive
{
    private List<int> m_ValidJointIdx;
	private int squats_count = 0;
	private bool stand_flag = true;
	private LinkedList<double> angles = new LinkedList<double>();
	private int num_of_angles_to_avg = 5;
	private int frame_num = 0;

    override public void Start()
    {
        m_isMoveFloor = true;

        // Print joints: 29
        // 5-root_rx, 8-spine_3_rx, 11-spine_4_rx, 14-spine_2_rx, 17-spine_1_rx, 20-neck_1_rx, 21-head_ee_ry, 23-left_clavicle_rz, 26-left_shoulder_ry, 27-left_elbow_rx, 28-left_lowarm_twist, 30-left_hand_ry, 31-left_ee_rx, 33-right_clavicle_rz, 36-right_shoulder_ry, 37-right_elbow_rx, 38-right_lowarm_twist, 40-right_hand_ry, 41-right_ee_rx, 44-left_hip_ry, 45-left_knee_rx, 47-left_ankle_ry, 48-left_toes_rx, 49-left_foot_ee, 52-right_hip_ry, 53-right_knee_rx, 55-right_ankle_ry, 56-right_toes_rx, 57-right_foot_ee
        int JSize = 21;
        m_ValidJointIdx = new List<int>();
        m_ValidJointIdx.Add(3 * 0);
		m_ValidJointIdx.Add(3 * 1);
		m_ValidJointIdx.Add(3 * 2);
		m_ValidJointIdx.Add(3 * 3);
		m_ValidJointIdx.Add(3 * 4);
		m_ValidJointIdx.Add(3 * 5);
		m_ValidJointIdx.Add(3 * 6);
		m_ValidJointIdx.Add(3 * 7);
		m_ValidJointIdx.Add(3 * 8);
		m_ValidJointIdx.Add(3 * 9);
		m_ValidJointIdx.Add(3 * 10);
		m_ValidJointIdx.Add(3 * 11);
		m_ValidJointIdx.Add(3 * 12);
		m_ValidJointIdx.Add(3 * 13);
		m_ValidJointIdx.Add(3 * 14);
		m_ValidJointIdx.Add(3 * 15);
		m_ValidJointIdx.Add(3 * 16);
		m_ValidJointIdx.Add(3 * 17);
		m_ValidJointIdx.Add(3 * 18);
		m_ValidJointIdx.Add(3 * 19);
		m_ValidJointIdx.Add(3 * 20);

		//Creating 21 joint spheres
        m_JointSpheres = new GameObject[JSize];
        for (int i = 0; i < m_JointSpheres.Length; ++i)
        {
            m_JointSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_WoodMatRef = Resources.Load("wood_Texture", typeof(Material)) as Material; // loads from Assests/Resources directory
            if (m_WoodMatRef != null)
            {
                m_JointSpheres[i].GetComponent<Renderer>().material = m_WoodMatRef;
                //m_JointSpheres[i].GetComponent<Renderer>().material.color = new Color(252.0f / 255.0f, 114.0f / 255.0f, 114.0f / 255.0f);
                m_JointSpheres[i].GetComponent<Renderer>().material.color = new Color(252.0f / 255.0f, 164.0f / 255.0f, 63.0f / 255.0f);
            }
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
        int nBones = 20;
        m_Bones = new GameObject[nBones];
        for (int i = 0; i < nBones; ++i)
        {
            m_Bones[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (m_WoodMatRef != null)
            {
                m_Bones[i].GetComponent<Renderer>().material = m_WoodMatRef;
                //m_Bones[i].GetComponent<Renderer>().material.color = new Color(252.0f / 255.0f, 114.0f / 255.0f, 114.0f / 255.0f);
                m_Bones[i].GetComponent<Renderer>().material.color = new Color(252.0f / 255.0f, 164.0f / 255.0f, 63.0f / 255.0f);
            }
            else
            {
                Debug.Log("Wood texture not assigned, will draw red.");
                m_Bones[i].GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }

    override public void Update(string Line)
    {
		frame_num++;

		if (Line.Length == 0) {
			return;
		}
			
        // Parse line
        int ParseOffset = 0; // No offset for real-time system
        string[] Tokens = Line.Split(',');
		if ((Tokens.Length - ParseOffset) / 3 != 21){
			return;
		}
        float LowestY = 0.0f;
        Vector3[] Joints = new Vector3[(Tokens.Length - ParseOffset) / 3];
        for (int k = 0; k < m_JointSpheres.Length; ++k)
        {
			
            int Idx = m_ValidJointIdx[k];
			bool result1 = float.TryParse(Tokens[Idx + 0 + ParseOffset], out Joints[k].x);
			Joints[k].x = Joints[k].x * -0.001f;
			bool result2 = float.TryParse(Tokens[Idx + 1 + ParseOffset], out Joints[k].y);
			Joints[k].y = Joints[k].y * -0.001f;
			bool result3 = float.TryParse (Tokens [Idx + 2 + ParseOffset], out Joints [k].z);
			Joints[k].z = Joints[k].z * -0.001f;

			if (!result1 || !result2 || !result3) {
				return;
			}

            if (Joints[k].y < LowestY)
                LowestY = Joints[k].y;

            m_JointSpheres[k].transform.position = Joints[k];
        }

        // Make floor stick to bottom-most joint (at index 16 or 20)
        GameObject Plane = GameObject.Find("CheckerboardPlane");
        float PlaneFootBuffer = 0.02f;
        float MoveAmount = 0.0f;
        if (Plane != null)
        {
            Vector3 OrigPos = Plane.transform.position;

            if (m_isMoveFloor)
            {
                MoveAmount = Plane.transform.position.y;
                Plane.transform.position = new Vector3(OrigPos[0], LowestY - PlaneFootBuffer, OrigPos[2]);
                MoveAmount = MoveAmount - Plane.transform.position.y;
            }

            if (m_isVRMode)
            {
				Debug.Log ("m_isVRMode == true");
                GameObject Head = GameObject.Find("Main Camera");
                if (Head != null)
                {
					Debug.Log ("Head != null");
                    Head.transform.position = Joints[0];
                    Head.transform.rotation = GvrViewer.Controller.Head.transform.rotation;
                }
            }
            else
            {
                GameObject FollowerCamera = GameObject.Find("ExternalCamera");
                if (FollowerCamera != null)
                {
                    // Move camera with checkerboard plane
                    OrigPos = FollowerCamera.transform.position;
                    FollowerCamera.transform.position = new Vector3(OrigPos[0], Plane.transform.position.y + 1, OrigPos[2]);
                }
                else
                    Debug.Log("Empty follower camera.");
            }
        }
			
        //0-head_top, 16-head
        drawEllipsoid(Joints[0], Joints[16], m_Bones[0]);
		//16-head, 1-neck
		drawEllipsoid(Joints[16], Joints[1], m_Bones[1]);
		//1-neck, 2-right_shoulder
		drawEllipsoid(Joints[1], Joints[2], m_Bones[2]);
		//2-right_shoulder, 3-right_elbow
		drawEllipsoid(Joints[2], Joints[3], m_Bones[3]);
		//3-right_elbow, 4-right_wrist
		drawEllipsoid(Joints[3], Joints[4], m_Bones[4]);
		//4-right_wrist, 17-right_hand
		drawEllipsoid(Joints[4], Joints[17], m_Bones[5]);
		//1-neck, 5-left_shoulder
		drawEllipsoid(Joints[1], Joints[5], m_Bones[6]);
		//5-left_shoulder, 6-left_elbow
		drawEllipsoid(Joints[5], Joints[6], m_Bones[7]);
		//6-left_elbow, 7-left_wrist
		drawEllipsoid(Joints[6], Joints[7], m_Bones[8]);
		//7-left_wrist, 18-left_hand
		drawEllipsoid(Joints[7], Joints[18], m_Bones[9]);
		//1-neck, 15-spine
		drawEllipsoid(Joints[1], Joints[15], m_Bones[10]);
		//15-spine, 14-pelvis
		drawEllipsoid(Joints[15], Joints[14], m_Bones[11]);
		//14-pelvis, 8-right_hip
		drawEllipsoid(Joints[14], Joints[8], m_Bones[12]);
		//8-right_hip, 9-right_knee
		drawEllipsoid(Joints[8], Joints[9], m_Bones[13]);
		//9-right_knee, 10-right_ankle
		drawEllipsoid(Joints[9], Joints[10], m_Bones[14]);
		//10-right_ankle, 19-right_toe
		drawEllipsoid(Joints[10], Joints[19], m_Bones[15]);
		//14-pelvis, 11-left_hip
		drawEllipsoid(Joints[14], Joints[11], m_Bones[16]);
		//11-left_hip, 12-left_knee
		drawEllipsoid(Joints[11], Joints[12], m_Bones[17]);
		//12-left_knee, 13-left_ankle
		drawEllipsoid(Joints[12], Joints[13], m_Bones[18]);
		//13-left_ankle, 20-left_toe
		drawEllipsoid(Joints[13], Joints[20], m_Bones[19]);

		double knee_left_x = Joints[12].x;
		double knee_left_y = Joints[12].y;
		double ankle_left_x = Joints [13].x;
		double ankle_left_y = Joints [13].y;
		double hip_left_x = Joints [11].x;
		double hip_left_y = Joints [11].y;

		double left_knee_ankle_length = distance(knee_left_x, ankle_left_x, knee_left_y, ankle_left_y);
		double left_hip_ankle_length = distance(hip_left_x, ankle_left_x, hip_left_y, ankle_left_y);
		double left_knee_hip_length = distance(knee_left_x, hip_left_x, knee_left_y, hip_left_y);

		double knee_right_x = Joints[9].x;
		double knee_right_y = Joints[9].y;
		double ankle_right_x = Joints [10].x;
		double ankle_right_y = Joints [10].y;
		double hip_right_x = Joints [8].x;
		double hip_right_y = Joints [8].y;

		double right_knee_ankle_length = distance(knee_right_x, ankle_right_x, knee_right_y, ankle_right_y);
		double right_hip_ankle_length = distance(hip_right_x, ankle_right_x, hip_right_y, ankle_right_y);
		double right_knee_hip_length = distance(knee_right_x, hip_right_x, knee_right_y, hip_right_y);


		double left_knee_angle = angle(left_knee_ankle_length, left_hip_ankle_length, left_knee_hip_length);

		double right_knee_angle = angle(right_knee_ankle_length, right_hip_ankle_length, right_knee_hip_length);

		if (frame_num < num_of_angles_to_avg) {
			angles.AddFirst (right_knee_angle);
		} 
		else {
			angles.AddFirst ( right_knee_angle );

			//finding average
			double avg_angle = 0;
			foreach ( double ang in angles ){
				avg_angle = avg_angle + ang;
			}
			avg_angle = avg_angle / num_of_angles_to_avg;

			double sit_angle_thresh = 110;
			double stand_angle_thresh = 160;

			if (avg_angle < sit_angle_thresh && stand_flag == true) {
				stand_flag = false;
				//Debug.Log ( squats_count );
			} else if (avg_angle > stand_angle_thresh && stand_flag == false) {
				stand_flag = true;
				squats_count++;
				Debug.Log ( squats_count );
			}

			angles.RemoveLast();
			//Debug.Log (avg_angle);
			//Debug.Log (left_knee_angle);
			//Debug.Log (right_knee_angle);
		}
        // Disable toe sphere
        //m_JointSpheres[22].GetComponent<MeshRenderer>().enabled = false;
        //m_JointSpheres[23].GetComponent<MeshRenderer>().enabled = false;
        //m_JointSpheres[27].GetComponent<MeshRenderer>().enabled = false;
        //m_JointSpheres[28].GetComponent<MeshRenderer>().enabled = false;
        //m_Bones[22].GetComponent<MeshRenderer>().enabled = false;
        //m_Bones[27].GetComponent<MeshRenderer>().enabled = false;

        // Draw mesh
        //RFoot.transform.rotation = Quaternion.LookRotation((Joints[23] - Joints[22]).normalized);
        //RFoot.transform.rotation = Quaternion.Euler(RFoot.transform.eulerAngles + new Vector3(140, 0, 0));
        //RFoot.transform.position = Joints[22];

        // Rotate z-axis to align with bone vector
        //LFoot.transform.rotation = Quaternion.LookRotation((Joints[28] - Joints[27]).normalized);
        //LFoot.transform.rotation = Quaternion.Euler(LFoot.transform.eulerAngles + new Vector3(140, 0, 0));
        // Position at middle
        //LFoot.transform.position = Joints[27];

    }

	private double angle(double side_beside_angle, double side_infrontof_angle, double sba_second)
	{
		double temp;
		temp = (Math.Pow(sba_second, 2) + Math.Pow(side_beside_angle, 2) - Math.Pow(side_infrontof_angle, 2)) / (2 * side_beside_angle * sba_second);

		return Math.Acos(temp) * (180 / Math.PI);
	}

	private double distance(double x1, double x2, double y1, double y2)
	{
		return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
	}
}
