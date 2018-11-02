using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;

public class MMOLanMessageSender : SingleMonoBehaviour<MMOLanMessageSender>
{

	int port = 50001;
	private UdpClient client;
	IPEndPoint mIPEndPoint;
	string mTargetIP = "127.0.0.1";
	byte[] dgram = new byte[1];

	void Start ()
	{
		client = new UdpClient ();
		mTargetIP = Network.player.ipAddress;
		mTargetIP = mTargetIP.Substring(0, mTargetIP.LastIndexOf("."));
		StartCoroutine (_Sender ());
	}

	IEnumerator _Sender ()
	{
		while (true) {
			yield return null;
			for (int i = 0; i < 255; i++) { //建立255个线程扫描IP
				string ip = mTargetIP + "." + i.ToString ();
				IPAddress address = IPAddress.Parse (ip);
				mIPEndPoint = new IPEndPoint (address, port);
				try {
					client.Send (dgram, dgram.Length, mIPEndPoint);
				}
				catch (Exception ex) {
//					Debug.LogError (ex.Message);
				}
				yield return null;
			}
		}
	}

}
