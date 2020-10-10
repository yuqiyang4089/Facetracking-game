using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TotalOffset : MonoBehaviour {
    /// <summary>
    ///     A UDP Client for receiving UDP packages. Assign a GameObject to get things to work.
    /// 
    ///     `x_offset`: offset in x-axis
    ///     `y_offset`: offset in y-axis
    ///     `para`: parameter for tweaking the final outcome
    /// </summary>

    // TODO (Yuqi/Jiayu): implement y-axis, and the parameter still need some tweaking


	public Camera cam;
	public Transform target;
    public float para = 0.08f;
	Thread receiveThread;
	UdpClient client;
	public int port;
	public string[] lastReceivedUDPPacket = new string[2];
	public string[] curReceivedUDPPacket = new string[2];
	private Vector2 orig_position = new Vector2(-1, -1);
    private Vector3 origCamPosition;
	private Vector3 rawOffset;
	private float origZoom;
	private float curZoom;
	public float zoomPara = 0.001f;

	void Start () {
		init();
        origCamPosition = target.position + new Vector3(0, 4, -11);
        cam.transform.position = origCamPosition;
		cam.focalLength = 24;
	}

	void OnGUI(){
		Rect rectObj=new Rect(40,10,200,400);
		
		GUIStyle style  = new GUIStyle();
		
		style.alignment = TextAnchor.UpperLeft;
		
		GUI.Box(rectObj,"# UDPReceive\n127.0.0.1 "+port +" #\n"
		          
		        //+ "shell> nc -u 127.0.0.1 : "+port +" \n"
		          
		        // + "\nLast Packet: \n"+ "X:"+lastReceivedUDPPacket[0]+"  Y:"+lastReceivedUDPPacket[1]
		          
		        //+ "\n\nAll Messages: \n"+allReceivedUDPPackets
		          
		        ,style);
	}

	private void init(){
		print ("UPDSend.init()");

		print ("Sending to 127.0.0.1 : " + port);

		receiveThread = new Thread(new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();

	}

	private void ReceiveData(){
		client = new UdpClient(port);
		while (true) {
			try{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
				byte[] data = client.Receive(ref anyIP);
				string text = Encoding.UTF8.GetString(data);
                lastReceivedUDPPacket = curReceivedUDPPacket;
                curReceivedUDPPacket = text.Split(',');
				if (Vector2.Equals(orig_position, new Vector2(-1, -1))) {
					orig_position = new Vector2(float.Parse(curReceivedUDPPacket[0]), float.Parse(curReceivedUDPPacket[1]));
					origZoom = float.Parse(curReceivedUDPPacket[2]);
					curZoom = origZoom;
				}

                if (curReceivedUDPPacket[0] == "no") {
					Debug.Log("No message received.\n");
                } else {
                    float x_cur = float.Parse(curReceivedUDPPacket[0]);
					float y_cur = float.Parse(curReceivedUDPPacket[1]);
					curZoom = float.Parse(curReceivedUDPPacket[2]);

					rawOffset = new Vector2(x_cur, y_cur) - orig_position; // z should be set to 0 automatically
                }

			}catch(Exception e){
				print (e.ToString());
			}
		}
	}

	public double dist(float x1, float y1, float x2, float y2) {
		double d = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
		// print(d);
		return d;
	}

	public string[] getLatestUDPPacket(){
		return lastReceivedUDPPacket;
	}
	
	// Update is called once per frame
	void Update() {
        Vector3 desiredPosition = origCamPosition - rawOffset * para;
		cam.transform.position = Vector3.Lerp(cam.transform.position, desiredPosition, 0.125f);
		cam.transform.LookAt(target);
		cam.focalLength = Mathf.Lerp(cam.focalLength, 24 + (curZoom - origZoom) * zoomPara, 0.1f);
	}

	void OnApplicationQuit(){
		if (receiveThread != null) {
			receiveThread.Abort();
			Debug.Log(receiveThread.IsAlive); //must be false
		}
	}
}