using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class CarController : MonoBehaviour
{
	//Functions of Socket
	private void Start()
	{
		ThreadStart ts = new ThreadStart(GetInfo);
		mThread = new Thread(ts);
		mThread.Start();
	}
	void GetInfo()
	{
		localAdd = IPAddress.Parse(connectionIP);
		listener = new TcpListener(IPAddress.Any, connectionPort);
		listener.Start();

		client = listener.AcceptTcpClient();

		running = true;
		while (running)
		{
			SendAndReceiveData();
		}
		listener.Stop();
	}

	void SendAndReceiveData()
	{
		NetworkStream nwStream = client.GetStream();
		byte[] buffer = new byte[client.ReceiveBufferSize];

		//---receiving Data from the Host----
		int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); //Getting data in Bytes from Python
		string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string

		if (dataReceived != null)
		{
			//---Using received data---
			receivedPos = StringToVector3(dataReceived); //<-- assigning receivedPos value from Python
			print("[+] received pos data");

			//---Sending Data to Host----
			byte[] myWriteBuffer = Encoding.ASCII.GetBytes("[+] updated"); //Converting string to byte data
			nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
		}
	}
	public static Vector3 StringToVector3(string sVector)
	{
		// Remove the parentheses
		if (sVector.StartsWith("(") && sVector.EndsWith(")"))
		{
			sVector = sVector.Substring(1, sVector.Length - 2);
		}

		// split the items
		string[] sArray = sVector.Split(',');

		// store as a Vector3
		Vector3 result = new Vector3(
			float.Parse(sArray[0]),
			float.Parse(sArray[1]),
			float.Parse(sArray[2]));

		return result;
	}

	//End of socket funcs

	//public void GetInput()
	//{
	//	m_horizontalInput = Input.GetAxis("Horizontal");
	//	print(m_horizontalInput); 
	//	m_verticalInput = Input.GetAxis("Vertical");
	//	print(m_verticalInput);
	//}
	//Get Input py comm version

	public void GetInput()
    {
		m_horizontalInput = receivedPos.x;
		m_verticalInput = receivedPos.y;
	}

	//else Code
	public void GetInputForManual()
	{
		m_horizontalInput = Input.GetAxis("Horizontal");
		m_verticalInput = Input.GetAxis("Vertical");
	}
	//END
	private void Steer()
	{
		m_steeringAngle = maxSteerAngle * m_horizontalInput;
		FrontLeftWheel.steerAngle = m_steeringAngle;
		FrontRightWheel.steerAngle = m_steeringAngle;
	}

	private void Accelerate()
	{
		FrontLeftWheel.motorTorque = m_verticalInput * motorForce;
		FrontRightWheel.motorTorque = m_verticalInput * motorForce;
	}

	private void UpdateWheelPoses()
	{
		UpdateWheelPose(FrontLeftWheel, frontDriverT);
		UpdateWheelPose(FrontRightWheel, frontPassengerT);
		UpdateWheelPose(RearLeftWheel, rearDriverT);
		UpdateWheelPose(RearRightWheel, rearPassengerT);
	}

	private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
	{
		Vector3 _pos = _transform.position;
		Quaternion _quat = _transform.rotation;
		
		_collider.GetWorldPose(out _pos, out _quat);

		_transform.position = _pos;
		_transform.rotation = _quat;
	}

	private void FixedUpdate()
	{
		if (ManualControl == false)
		{
			GetInput();
		}
		else
		{
			GetInputForManual();
		}
		Steer();
		Accelerate();
		UpdateWheelPoses();
	}

	private float m_horizontalInput;
	private float m_verticalInput;
	private float m_steeringAngle;

	public WheelCollider FrontLeftWheel, FrontRightWheel;
	public WheelCollider RearLeftWheel, RearRightWheel;
	public Transform frontDriverT, frontPassengerT;
	public Transform rearDriverT, rearPassengerT;
	public float maxSteerAngle = 30;
	public float motorForce = 50;

	Thread mThread;
	public string connectionIP = "127.0.0.1";
	public int connectionPort = 25001;
	IPAddress localAdd;
	TcpListener listener;
	TcpClient client;
	Vector3 receivedPos = Vector3.zero;

	bool running;
	public bool ManualControl = true;
}
