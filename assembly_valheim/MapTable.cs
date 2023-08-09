using System;
using UnityEngine;

// Token: 0x02000265 RID: 613
public class MapTable : MonoBehaviour
{
	// Token: 0x060017AA RID: 6058 RVA: 0x0009D658 File Offset: 0x0009B858
	private void Start()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_nview.Register<ZPackage>("MapData", new Action<long, ZPackage>(this.RPC_MapData));
		Switch readSwitch = this.m_readSwitch;
		readSwitch.m_onUse = (Switch.Callback)Delegate.Combine(readSwitch.m_onUse, new Switch.Callback(this.OnRead));
		Switch readSwitch2 = this.m_readSwitch;
		readSwitch2.m_onHover = (Switch.TooltipCallback)Delegate.Combine(readSwitch2.m_onHover, new Switch.TooltipCallback(this.GetReadHoverText));
		Switch writeSwitch = this.m_writeSwitch;
		writeSwitch.m_onUse = (Switch.Callback)Delegate.Combine(writeSwitch.m_onUse, new Switch.Callback(this.OnWrite));
		Switch writeSwitch2 = this.m_writeSwitch;
		writeSwitch2.m_onHover = (Switch.TooltipCallback)Delegate.Combine(writeSwitch2.m_onHover, new Switch.TooltipCallback(this.GetWriteHoverText));
	}

	// Token: 0x060017AB RID: 6059 RVA: 0x0009D72C File Offset: 0x0009B92C
	private string GetReadHoverText()
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_readmap ");
	}

	// Token: 0x060017AC RID: 6060 RVA: 0x0009D788 File Offset: 0x0009B988
	private string GetWriteHoverText()
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_writemap ");
	}

	// Token: 0x060017AD RID: 6061 RVA: 0x0009D7E4 File Offset: 0x0009B9E4
	private bool OnRead(Switch caller, Humanoid user, ItemDrop.ItemData item)
	{
		if (item != null)
		{
			return false;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		byte[] byteArray = this.m_nview.GetZDO().GetByteArray(ZDOVars.s_data, null);
		if (byteArray != null)
		{
			byte[] dataArray = Utils.Decompress(byteArray);
			if (Minimap.instance.AddSharedMapData(dataArray))
			{
				user.Message(MessageHud.MessageType.Center, "$msg_mapsynced", 0, null);
			}
			else
			{
				user.Message(MessageHud.MessageType.Center, "$msg_alreadysynced", 0, null);
			}
		}
		else
		{
			user.Message(MessageHud.MessageType.Center, "$msg_mapnodata", 0, null);
		}
		return false;
	}

	// Token: 0x060017AE RID: 6062 RVA: 0x0009D85C File Offset: 0x0009BA5C
	private bool OnWrite(Switch caller, Humanoid user, ItemDrop.ItemData item)
	{
		if (item != null)
		{
			return false;
		}
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		byte[] array = this.m_nview.GetZDO().GetByteArray(ZDOVars.s_data, null);
		if (array != null)
		{
			array = Utils.Decompress(array);
		}
		ZPackage mapData = this.GetMapData(array);
		this.m_nview.InvokeRPC("MapData", new object[]
		{
			mapData
		});
		user.Message(MessageHud.MessageType.Center, "$msg_mapsaved", 0, null);
		this.m_writeEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		return true;
	}

	// Token: 0x060017AF RID: 6063 RVA: 0x0009D914 File Offset: 0x0009BB14
	private void RPC_MapData(long sender, ZPackage pkg)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		byte[] array = pkg.GetArray();
		this.m_nview.GetZDO().Set(ZDOVars.s_data, array);
	}

	// Token: 0x060017B0 RID: 6064 RVA: 0x0009D94C File Offset: 0x0009BB4C
	private ZPackage GetMapData(byte[] currentMapData)
	{
		byte[] array = Utils.Compress(Minimap.instance.GetSharedMapData(currentMapData));
		ZLog.Log("Compressed map data:" + array.Length.ToString());
		return new ZPackage(array);
	}

	// Token: 0x060017B1 RID: 6065 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x0400191F RID: 6431
	public string m_name = "$piece_maptable";

	// Token: 0x04001920 RID: 6432
	public Switch m_readSwitch;

	// Token: 0x04001921 RID: 6433
	public Switch m_writeSwitch;

	// Token: 0x04001922 RID: 6434
	public EffectList m_writeEffects = new EffectList();

	// Token: 0x04001923 RID: 6435
	private ZNetView m_nview;
}
