using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001E3 RID: 483
public class RenderGroupSystem : MonoBehaviour
{
	// Token: 0x060013C3 RID: 5059 RVA: 0x00082000 File Offset: 0x00080200
	private void Awake()
	{
		if (RenderGroupSystem.s_instance != null)
		{
			ZLog.LogError("Instance already set!");
			return;
		}
		RenderGroupSystem.s_instance = this;
		foreach (object obj in Enum.GetValues(typeof(RenderGroup)))
		{
			RenderGroup key = (RenderGroup)obj;
			this.m_renderGroups.Add(key, new RenderGroupSystem.RenderGroupState());
		}
	}

	// Token: 0x060013C4 RID: 5060 RVA: 0x0008208C File Offset: 0x0008028C
	private void OnDestroy()
	{
		if (RenderGroupSystem.s_instance == this)
		{
			RenderGroupSystem.s_instance = null;
		}
	}

	// Token: 0x060013C5 RID: 5061 RVA: 0x000820A4 File Offset: 0x000802A4
	private void LateUpdate()
	{
		bool flag = Player.m_localPlayer != null && Player.m_localPlayer.InInterior();
		this.m_renderGroups[RenderGroup.Always].Active = true;
		this.m_renderGroups[RenderGroup.Overworld].Active = !flag;
		this.m_renderGroups[RenderGroup.Interior].Active = flag;
	}

	// Token: 0x060013C6 RID: 5062 RVA: 0x00082108 File Offset: 0x00080308
	public static void Register(RenderGroup group, RenderGroupSystem.GroupChangedHandler subscriber)
	{
		RenderGroupSystem.RenderGroupState renderGroupState = RenderGroupSystem.s_instance.m_renderGroups[group];
		renderGroupState.GroupChanged += subscriber;
		subscriber(renderGroupState.Active);
	}

	// Token: 0x060013C7 RID: 5063 RVA: 0x00082139 File Offset: 0x00080339
	public static void Unregister(RenderGroup group, RenderGroupSystem.GroupChangedHandler subscriber)
	{
		if (RenderGroupSystem.s_instance == null)
		{
			return;
		}
		RenderGroupSystem.s_instance.m_renderGroups[group].GroupChanged -= subscriber;
	}

	// Token: 0x060013C8 RID: 5064 RVA: 0x0008215F File Offset: 0x0008035F
	public static bool IsGroupActive(RenderGroup group)
	{
		return RenderGroupSystem.s_instance == null || RenderGroupSystem.s_instance.m_renderGroups[group].Active;
	}

	// Token: 0x040014A8 RID: 5288
	private static RenderGroupSystem s_instance;

	// Token: 0x040014A9 RID: 5289
	private Dictionary<RenderGroup, RenderGroupSystem.RenderGroupState> m_renderGroups = new Dictionary<RenderGroup, RenderGroupSystem.RenderGroupState>();

	// Token: 0x020001E4 RID: 484
	// (Invoke) Token: 0x060013CB RID: 5067
	public delegate void GroupChangedHandler(bool shouldRender);

	// Token: 0x020001E5 RID: 485
	private class RenderGroupState
	{
		// Token: 0x170000CB RID: 203
		// (get) Token: 0x060013CE RID: 5070 RVA: 0x00082198 File Offset: 0x00080398
		// (set) Token: 0x060013CF RID: 5071 RVA: 0x000821A0 File Offset: 0x000803A0
		public bool Active
		{
			get
			{
				return this.active;
			}
			set
			{
				if (this.active == value)
				{
					return;
				}
				this.active = value;
				RenderGroupSystem.GroupChangedHandler groupChanged = this.GroupChanged;
				if (groupChanged == null)
				{
					return;
				}
				groupChanged(this.active);
			}
		}

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x060013D0 RID: 5072 RVA: 0x000821CC File Offset: 0x000803CC
		// (remove) Token: 0x060013D1 RID: 5073 RVA: 0x00082204 File Offset: 0x00080404
		public event RenderGroupSystem.GroupChangedHandler GroupChanged;

		// Token: 0x040014AA RID: 5290
		private bool active;
	}
}
