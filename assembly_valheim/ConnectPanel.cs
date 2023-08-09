using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000098 RID: 152
public class ConnectPanel : MonoBehaviour
{
	// Token: 0x17000023 RID: 35
	// (get) Token: 0x06000681 RID: 1665 RVA: 0x00031729 File Offset: 0x0002F929
	public static ConnectPanel instance
	{
		get
		{
			return ConnectPanel.m_instance;
		}
	}

	// Token: 0x06000682 RID: 1666 RVA: 0x00031730 File Offset: 0x0002F930
	private void Start()
	{
		ConnectPanel.m_instance = this;
		this.m_root.gameObject.SetActive(false);
		this.m_playerListBaseSize = this.m_playerList.rect.height;
	}

	// Token: 0x06000683 RID: 1667 RVA: 0x0003176D File Offset: 0x0002F96D
	public static bool IsVisible()
	{
		return ConnectPanel.m_instance && ConnectPanel.m_instance.m_root.gameObject.activeSelf;
	}

	// Token: 0x06000684 RID: 1668 RVA: 0x00031794 File Offset: 0x0002F994
	private void Update()
	{
		if (ZInput.GetKeyDown(KeyCode.F2) || (ZInput.GetButton("JoyLTrigger") && ZInput.GetButton("JoyLBumper") && ZInput.GetButtonDown("JoyBack")))
		{
			this.m_root.gameObject.SetActive(!this.m_root.gameObject.activeSelf);
		}
		if (this.m_root.gameObject.activeInHierarchy)
		{
			if (!ZNet.instance.IsServer() && ZNet.GetConnectionStatus() == ZNet.ConnectionStatus.Connected)
			{
				this.m_serverField.gameObject.SetActive(true);
				this.m_serverField.text = ZNet.GetServerString(true);
			}
			else
			{
				this.m_serverField.gameObject.SetActive(false);
			}
			this.m_worldField.text = ZNet.instance.GetWorldName();
			this.UpdateFps();
			this.m_myPort.gameObject.SetActive(ZNet.instance.IsServer());
			this.m_myPort.text = ZNet.instance.GetHostPort().ToString();
			this.m_myUID.text = ZNet.GetUID().ToString();
			if (ZDOMan.instance != null)
			{
				this.m_zdos.text = ZDOMan.instance.NrOfObjects().ToString();
				float num;
				float num2;
				ZDOMan.instance.GetAverageStats(out num, out num2);
				this.m_zdosSent.text = num.ToString("0.0");
				this.m_zdosRecv.text = num2.ToString("0.0");
				this.m_activePeers.text = ZNet.instance.GetNrOfPlayers().ToString();
			}
			this.m_zdosPool.text = string.Concat(new string[]
			{
				ZDOPool.GetPoolActive().ToString(),
				" / ",
				ZDOPool.GetPoolSize().ToString(),
				" / ",
				ZDOPool.GetPoolTotal().ToString()
			});
			if (ZNetScene.instance)
			{
				this.m_zdosInstances.text = ZNetScene.instance.NrOfInstances().ToString();
			}
			float num3;
			float num4;
			int num5;
			float num6;
			float num7;
			ZNet.instance.GetNetStats(out num3, out num4, out num5, out num6, out num7);
			this.m_dataSent.text = (num6 / 1024f).ToString("0.0") + "kb/s";
			this.m_dataRecv.text = (num7 / 1024f).ToString("0.0") + "kb/s";
			this.m_ping.text = num5.ToString("0") + "ms";
			this.m_quality.text = ((int)(num3 * 100f)).ToString() + "% / " + ((int)(num4 * 100f)).ToString() + "%";
			this.m_clientSendQueue.text = ZDOMan.instance.GetClientChangeQueue().ToString();
			this.m_nrOfConnections.text = ZNet.instance.GetPeerConnections().ToString();
			string text = "";
			foreach (ZNetPeer znetPeer in ZNet.instance.GetConnectedPeers())
			{
				if (znetPeer.IsReady())
				{
					text = string.Concat(new string[]
					{
						text,
						znetPeer.m_socket.GetEndPointString(),
						" UID: ",
						znetPeer.m_uid.ToString(),
						"\n"
					});
				}
				else
				{
					text = text + znetPeer.m_socket.GetEndPointString() + " connecting \n";
				}
			}
			this.m_connections.text = text;
			List<ZNet.PlayerInfo> playerList = ZNet.instance.GetPlayerList();
			float num8 = 16f;
			if (playerList.Count != this.m_playerListElements.Count)
			{
				foreach (GameObject obj in this.m_playerListElements)
				{
					UnityEngine.Object.Destroy(obj);
				}
				this.m_playerListElements.Clear();
				for (int i = 0; i < playerList.Count; i++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_playerElement, this.m_playerList);
					(gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)i * -num8);
					this.m_playerListElements.Add(gameObject);
				}
				float num9 = (float)playerList.Count * num8;
				num9 = Mathf.Max(this.m_playerListBaseSize, num9);
				this.m_playerList.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num9);
				this.m_playerListScroll.value = 1f;
			}
			for (int j = 0; j < playerList.Count; j++)
			{
				ZNet.PlayerInfo playerInfo = playerList[j];
				Text component = this.m_playerListElements[j].transform.Find("name").GetComponent<Text>();
				Text component2 = this.m_playerListElements[j].transform.Find("hostname").GetComponent<Text>();
				Component component3 = this.m_playerListElements[j].transform.Find("KickButton").GetComponent<Button>();
				component.text = playerInfo.m_name;
				component2.text = playerInfo.m_host;
				component3.gameObject.SetActive(false);
			}
			this.m_connectButton.interactable = this.ValidHost();
		}
	}

	// Token: 0x06000685 RID: 1669 RVA: 0x00031D4C File Offset: 0x0002FF4C
	private void UpdateFps()
	{
		this.m_frameTimer += Time.deltaTime;
		this.m_frameSamples++;
		if (this.m_frameTimer > 1f)
		{
			float num = this.m_frameTimer / (float)this.m_frameSamples;
			this.m_fps.text = (1f / num).ToString("0.0");
			this.m_frameTime.text = "( " + (num * 1000f).ToString("00.0") + "ms )";
			this.m_frameSamples = 0;
			this.m_frameTimer = 0f;
		}
	}

	// Token: 0x06000686 RID: 1670 RVA: 0x00031DF4 File Offset: 0x0002FFF4
	private bool ValidHost()
	{
		int num = 0;
		try
		{
			num = int.Parse(this.m_hostPort.text);
		}
		catch
		{
			return false;
		}
		return !string.IsNullOrEmpty(this.m_hostName.text) && num != 0;
	}

	// Token: 0x040007F2 RID: 2034
	private static ConnectPanel m_instance;

	// Token: 0x040007F3 RID: 2035
	public Transform m_root;

	// Token: 0x040007F4 RID: 2036
	public Text m_serverField;

	// Token: 0x040007F5 RID: 2037
	public Text m_worldField;

	// Token: 0x040007F6 RID: 2038
	public Text m_statusField;

	// Token: 0x040007F7 RID: 2039
	public Text m_connections;

	// Token: 0x040007F8 RID: 2040
	public RectTransform m_playerList;

	// Token: 0x040007F9 RID: 2041
	public Scrollbar m_playerListScroll;

	// Token: 0x040007FA RID: 2042
	public GameObject m_playerElement;

	// Token: 0x040007FB RID: 2043
	public InputField m_hostName;

	// Token: 0x040007FC RID: 2044
	public InputField m_hostPort;

	// Token: 0x040007FD RID: 2045
	public Button m_connectButton;

	// Token: 0x040007FE RID: 2046
	public Text m_myPort;

	// Token: 0x040007FF RID: 2047
	public Text m_myUID;

	// Token: 0x04000800 RID: 2048
	public Text m_knownHosts;

	// Token: 0x04000801 RID: 2049
	public Text m_nrOfConnections;

	// Token: 0x04000802 RID: 2050
	public Text m_pendingConnections;

	// Token: 0x04000803 RID: 2051
	public Toggle m_autoConnect;

	// Token: 0x04000804 RID: 2052
	public Text m_zdos;

	// Token: 0x04000805 RID: 2053
	public Text m_zdosPool;

	// Token: 0x04000806 RID: 2054
	public Text m_zdosSent;

	// Token: 0x04000807 RID: 2055
	public Text m_zdosRecv;

	// Token: 0x04000808 RID: 2056
	public Text m_zdosInstances;

	// Token: 0x04000809 RID: 2057
	public Text m_activePeers;

	// Token: 0x0400080A RID: 2058
	public Text m_ntp;

	// Token: 0x0400080B RID: 2059
	public Text m_upnp;

	// Token: 0x0400080C RID: 2060
	public Text m_dataSent;

	// Token: 0x0400080D RID: 2061
	public Text m_dataRecv;

	// Token: 0x0400080E RID: 2062
	public Text m_clientSendQueue;

	// Token: 0x0400080F RID: 2063
	public Text m_fps;

	// Token: 0x04000810 RID: 2064
	public Text m_frameTime;

	// Token: 0x04000811 RID: 2065
	public Text m_ping;

	// Token: 0x04000812 RID: 2066
	public Text m_quality;

	// Token: 0x04000813 RID: 2067
	private float m_playerListBaseSize;

	// Token: 0x04000814 RID: 2068
	private List<GameObject> m_playerListElements = new List<GameObject>();

	// Token: 0x04000815 RID: 2069
	private int m_frameSamples;

	// Token: 0x04000816 RID: 2070
	private float m_frameTimer;
}
