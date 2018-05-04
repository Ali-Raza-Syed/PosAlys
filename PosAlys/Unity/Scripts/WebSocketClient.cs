using System;
using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;


public class WebSocketClient : MonoBehaviour
{
	WebSocketSharp.WebSocket m_ws;
	public string m_Message;
	bool m_isWSConnected = false;

	public GameObject LFoot;
	public GameObject RFoot;
	private Quaternion m_ParentCamRot;
	private Vector3 m_ParentCamPos;
	private Vector3 m_ParentCamScale;

	private bool m_isKFWMode = true;
	private bool m_isRotateCamera = false;
	private runLive m_AnyMethod;
	private Vector3 m_Offset;
	private int num = 0;
	string[] text;
	int count;

	void ConnectWS()
	{
		if (m_isWSConnected)
			return;

		m_Message = "No data.";

		using (m_ws = new WebSocket("ws://localhost:8080/"))
		{
			//m_ws.Log.Level = WebSocketSharp.LogLevel.TRACE;
			//m_ws.Log.File = "D:\\ws_log.txt";

			m_ws.OnOpen += (sender, e) =>
			{
				m_ws.Send(String.Format("Hello server."));
				Debug.Log("Connection opened.");
				m_isWSConnected = true;
			};
			m_ws.OnMessage += (sender, e) =>
			{
				m_Message = e.Data;
				//Debug.Log(m_Message);
			};
			m_ws.OnClose += (sender, e) =>
			{
				m_ws.Connect(); // This is a hack, but whatever
				m_isWSConnected = false;
			};
			m_ws.OnError += (sender, e) =>
			{
				// NOT PRINTING ERRORS
				//Debug.LogError(e.Message);
				m_isWSConnected = false;
			};

			m_ws.Connect();
		}
	}

	public void Start()
	{
		text = System.IO.File.ReadAllLines(@"C:\Users\Syed_Ali_Raza_36CEA\Documents\new_Wooden_Man\Assets\StreamingAssets\csv_matrix_test3.txt");

		//Screen.SetResolution(1920, 1080, true);
		Application.runInBackground = true;

		m_isKFWMode = false;
		ConnectWS();

		if (m_isKFWMode)
			m_AnyMethod = new runLiveKFW();
		else
			m_AnyMethod = new runLiveVNect();

		if (gameObject.name == "FollowerCamera" || gameObject.name == "ExternalCamera")
		{
			m_AnyMethod.m_isVRMode = false;
			m_ParentCamRot = transform.rotation;
			m_ParentCamPos = transform.position;
			m_ParentCamScale = transform.localScale;
		}
		else
		{
			Debug.Log("Running in VR mode.");
			m_AnyMethod.m_isVRMode = true;
		}

		m_AnyMethod.Start();

		m_AnyMethod.LFoot = LFoot;
		m_AnyMethod.RFoot = RFoot;
	}

	public void Update()
	{
		if (num < 290) {
			num = num + 1;
			m_Message = text [num];


			if (m_isRotateCamera && m_AnyMethod.m_JointSpheres.Length > 0 && m_AnyMethod.m_isVRMode == false) {
				m_Offset = transform.position - m_AnyMethod.m_JointSpheres [0].transform.position;
				m_Offset = Quaternion.AngleAxis (100.0f * Time.deltaTime, Vector3.up) * m_Offset;

				transform.position = m_AnyMethod.m_JointSpheres [0].transform.position + m_Offset;
				transform.LookAt (m_AnyMethod.m_JointSpheres [0].transform.position);

				m_isRotateCamera = false;
			}

			if (Input.touchCount > 0 || Input.GetKeyUp (KeyCode.S)) {
				//Debug.Log("tst");
				int childCount = GameObject.Find ("AllBalls").GetComponent<Transform> ().GetChildCount ();
				Transform Feet = GameObject.Find ("feet").GetComponent<Transform> ();
				float xdist = -3f;
				for (int i = 0; i < childCount; ++i) {
					float ydist = -3f;
					int childCnt = GameObject.Find ("AllBalls").GetComponent<Transform> ().GetChild (i).GetChildCount ();
					for (int j = 0; j < childCnt; ++j) {
						xdist += 0.1f;
						ydist += 0.2f;
						Transform Ball = GameObject.Find ("AllBalls").GetComponent<Transform> ().GetChild (i).GetChild (j);
						Ball.position = new Vector3 (Feet.position.x + xdist, Feet.position.y + 4, Feet.position.z + ydist);
					}
				}

			}
			//Debug.Log(m_Message);
			if (m_AnyMethod == null)
				return;
			m_AnyMethod.Update (m_Message);
		
		} 
		else {
			num = 0;
			return;
		}
	}

	public void OnApplicationQuit()
	{
		if (m_ws != null && m_ws.ReadyState == WebSocketState.OPEN)
			m_ws.Close();
	}

	void LateUpdate()
	{
		if (m_AnyMethod.m_isVRMode)
		{
			if (Input.GetKeyUp(KeyCode.V))
			{
				Debug.Log("Recentering VR.");
				if (m_AnyMethod != null)
					m_AnyMethod.recenter();
			}
		}

		if (Input.GetKey(KeyCode.R) && m_AnyMethod.m_isVRMode == false)
		{
			m_isRotateCamera = !m_isRotateCamera;
			if (m_isRotateCamera)
				Debug.Log("Rotating camera.");
			else
				Debug.Log("Stopping camera rotation.");
		}

		if (Input.GetKey(KeyCode.C) && m_AnyMethod.m_isVRMode == false)
		{
			Debug.Log("Before cam: " + transform.position);
			Debug.Log("Before parent cam: " + m_ParentCamPos);
			transform.position = m_ParentCamPos;
			transform.rotation = m_ParentCamRot;
			transform.localScale = m_ParentCamScale;
			Debug.Log("After: " + transform.position);

			Debug.Log("Reset camera to original position.");
		}
	}

	void OnGUI()
	{
		if (m_AnyMethod.m_isVRMode)
		{
			if (GUI.Button(new Rect(Screen.currentResolution.width / 2, Screen.currentResolution.height / 20, Screen.currentResolution.width / 15, Screen.currentResolution.height / 20), "Recenter"))
			{
				Debug.Log("Recentering VR.");
				if (m_AnyMethod != null)
					m_AnyMethod.recenter();
			}
		}
	}
}