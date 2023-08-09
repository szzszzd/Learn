using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002B9 RID: 697
public class Vagon : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06001A5A RID: 6746 RVA: 0x000AF134 File Offset: 0x000AD334
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		Vagon.m_instances.Add(this);
		Heightmap.ForceGenerateAll();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_bodies = base.GetComponentsInChildren<Rigidbody>();
		this.m_lineRenderer = base.GetComponent<LineRenderer>();
		Rigidbody[] bodies = this.m_bodies;
		for (int i = 0; i < bodies.Length; i++)
		{
			bodies[i].maxDepenetrationVelocity = 2f;
		}
		this.m_nview.Register("RequestOwn", new Action<long>(this.RPC_RequestOwn));
		this.m_nview.Register("RequestDenied", new Action<long>(this.RPC_RequestDenied));
		base.InvokeRepeating("UpdateMass", 0f, 5f);
		base.InvokeRepeating("UpdateLoadVisualization", 0f, 3f);
	}

	// Token: 0x06001A5B RID: 6747 RVA: 0x000AF21A File Offset: 0x000AD41A
	private void OnDestroy()
	{
		Vagon.m_instances.Remove(this);
	}

	// Token: 0x06001A5C RID: 6748 RVA: 0x000AF228 File Offset: 0x000AD428
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001A5D RID: 6749 RVA: 0x000AF230 File Offset: 0x000AD430
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	// Token: 0x06001A5E RID: 6750 RVA: 0x000AF24C File Offset: 0x000AD44C
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		this.m_useRequester = character;
		if (!this.m_nview.IsOwner())
		{
			this.m_nview.InvokeRPC("RequestOwn", Array.Empty<object>());
		}
		return false;
	}

	// Token: 0x06001A5F RID: 6751 RVA: 0x000AF280 File Offset: 0x000AD480
	public void RPC_RequestOwn(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.InUse())
		{
			ZLog.Log("Requested use, but is already in use");
			this.m_nview.InvokeRPC(sender, "RequestDenied", Array.Empty<object>());
			return;
		}
		this.m_nview.GetZDO().SetOwner(sender);
	}

	// Token: 0x06001A60 RID: 6752 RVA: 0x000AF2D5 File Offset: 0x000AD4D5
	private void RPC_RequestDenied(long sender)
	{
		ZLog.Log("Got request denied");
		if (this.m_useRequester)
		{
			this.m_useRequester.Message(MessageHud.MessageType.Center, this.m_name + " is in use by someone else", 0, null);
			this.m_useRequester = null;
		}
	}

	// Token: 0x06001A61 RID: 6753 RVA: 0x000AF314 File Offset: 0x000AD514
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.UpdateAudio(Time.fixedDeltaTime);
		if (this.m_nview.IsOwner())
		{
			if (this.m_useRequester)
			{
				if (this.IsAttached())
				{
					this.Detach();
				}
				else if (this.CanAttach(this.m_useRequester.gameObject))
				{
					this.AttachTo(this.m_useRequester.gameObject);
				}
				else
				{
					this.m_useRequester.Message(MessageHud.MessageType.Center, "$msg_cart_incorrectposition", 0, null);
				}
				this.m_useRequester = null;
			}
			if (this.IsAttached() && !this.CanAttach(this.m_attachJoin.connectedBody.gameObject))
			{
				this.Detach();
				return;
			}
		}
		else if (this.IsAttached())
		{
			this.Detach();
		}
	}

	// Token: 0x06001A62 RID: 6754 RVA: 0x000AF3DC File Offset: 0x000AD5DC
	private void LateUpdate()
	{
		if (this.IsAttached())
		{
			this.m_lineRenderer.enabled = true;
			this.m_lineRenderer.SetPosition(0, this.m_lineAttachPoints0.position);
			this.m_lineRenderer.SetPosition(1, this.m_attachJoin.connectedBody.transform.position + this.m_lineAttachOffset);
			this.m_lineRenderer.SetPosition(2, this.m_lineAttachPoints1.position);
			return;
		}
		this.m_lineRenderer.enabled = false;
	}

	// Token: 0x06001A63 RID: 6755 RVA: 0x000AF464 File Offset: 0x000AD664
	public bool IsAttached(Character character)
	{
		return this.m_attachJoin && this.m_attachJoin.connectedBody.gameObject == character.gameObject;
	}

	// Token: 0x06001A64 RID: 6756 RVA: 0x000AF493 File Offset: 0x000AD693
	public bool InUse()
	{
		return (this.m_container && this.m_container.IsInUse()) || this.IsAttached();
	}

	// Token: 0x06001A65 RID: 6757 RVA: 0x000AF4B7 File Offset: 0x000AD6B7
	private bool IsAttached()
	{
		return this.m_attachJoin != null;
	}

	// Token: 0x06001A66 RID: 6758 RVA: 0x000AF4C8 File Offset: 0x000AD6C8
	private bool CanAttach(GameObject go)
	{
		if (base.transform.up.y < 0.1f)
		{
			return false;
		}
		Humanoid component = go.GetComponent<Humanoid>();
		return (!component || (!component.InDodge() && !component.IsTeleporting())) && Vector3.Distance(go.transform.position + this.m_attachOffset, this.m_attachPoint.position) < this.m_detachDistance;
	}

	// Token: 0x06001A67 RID: 6759 RVA: 0x000AF540 File Offset: 0x000AD740
	private void AttachTo(GameObject go)
	{
		Vagon.DetachAll();
		this.m_attachJoin = base.gameObject.AddComponent<ConfigurableJoint>();
		this.m_attachJoin.autoConfigureConnectedAnchor = false;
		this.m_attachJoin.anchor = this.m_attachPoint.localPosition;
		this.m_attachJoin.connectedAnchor = this.m_attachOffset;
		this.m_attachJoin.breakForce = this.m_breakForce;
		this.m_attachJoin.xMotion = ConfigurableJointMotion.Limited;
		this.m_attachJoin.yMotion = ConfigurableJointMotion.Limited;
		this.m_attachJoin.zMotion = ConfigurableJointMotion.Limited;
		SoftJointLimit linearLimit = default(SoftJointLimit);
		linearLimit.limit = 0.001f;
		this.m_attachJoin.linearLimit = linearLimit;
		SoftJointLimitSpring linearLimitSpring = default(SoftJointLimitSpring);
		linearLimitSpring.spring = this.m_spring;
		linearLimitSpring.damper = this.m_springDamping;
		this.m_attachJoin.linearLimitSpring = linearLimitSpring;
		this.m_attachJoin.zMotion = ConfigurableJointMotion.Locked;
		this.m_attachJoin.connectedBody = go.GetComponent<Rigidbody>();
	}

	// Token: 0x06001A68 RID: 6760 RVA: 0x000AF638 File Offset: 0x000AD838
	private static void DetachAll()
	{
		foreach (Vagon vagon in Vagon.m_instances)
		{
			vagon.Detach();
		}
	}

	// Token: 0x06001A69 RID: 6761 RVA: 0x000AF688 File Offset: 0x000AD888
	private void Detach()
	{
		if (this.m_attachJoin)
		{
			UnityEngine.Object.Destroy(this.m_attachJoin);
			this.m_attachJoin = null;
			this.m_body.WakeUp();
			this.m_body.AddForce(0f, 1f, 0f);
		}
	}

	// Token: 0x06001A6A RID: 6762 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001A6B RID: 6763 RVA: 0x000AF6DC File Offset: 0x000AD8DC
	private void UpdateMass()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_container == null)
		{
			return;
		}
		float totalWeight = this.m_container.GetInventory().GetTotalWeight();
		float mass = this.m_baseMass + totalWeight * this.m_itemWeightMassFactor;
		this.SetMass(mass);
	}

	// Token: 0x06001A6C RID: 6764 RVA: 0x000AF730 File Offset: 0x000AD930
	private void SetMass(float mass)
	{
		float mass2 = mass / (float)this.m_bodies.Length;
		Rigidbody[] bodies = this.m_bodies;
		for (int i = 0; i < bodies.Length; i++)
		{
			bodies[i].mass = mass2;
		}
	}

	// Token: 0x06001A6D RID: 6765 RVA: 0x000AF768 File Offset: 0x000AD968
	private void UpdateLoadVisualization()
	{
		if (this.m_container == null)
		{
			return;
		}
		float num = this.m_container.GetInventory().SlotsUsedPercentage();
		foreach (Vagon.LoadData loadData in this.m_loadVis)
		{
			loadData.m_gameobject.SetActive(num >= loadData.m_minPercentage);
		}
	}

	// Token: 0x06001A6E RID: 6766 RVA: 0x000AF7EC File Offset: 0x000AD9EC
	private void UpdateAudio(float dt)
	{
		float num = 0f;
		foreach (Rigidbody rigidbody in this.m_wheels)
		{
			num += rigidbody.angularVelocity.magnitude;
		}
		num /= (float)this.m_wheels.Length;
		float target = Mathf.Lerp(this.m_minPitch, this.m_maxPitch, Mathf.Clamp01(num / this.m_maxPitchVel));
		float target2 = this.m_maxVol * Mathf.Clamp01(num / this.m_maxVolVel);
		foreach (AudioSource audioSource in this.m_wheelLoops)
		{
			audioSource.volume = Mathf.MoveTowards(audioSource.volume, target2, this.m_audioChangeSpeed * dt);
			audioSource.pitch = Mathf.MoveTowards(audioSource.pitch, target, this.m_audioChangeSpeed * dt);
		}
	}

	// Token: 0x04001C67 RID: 7271
	private static List<Vagon> m_instances = new List<Vagon>();

	// Token: 0x04001C68 RID: 7272
	public Transform m_attachPoint;

	// Token: 0x04001C69 RID: 7273
	public string m_name = "Wagon";

	// Token: 0x04001C6A RID: 7274
	public float m_detachDistance = 2f;

	// Token: 0x04001C6B RID: 7275
	public Vector3 m_attachOffset = new Vector3(0f, 0.8f, 0f);

	// Token: 0x04001C6C RID: 7276
	public Container m_container;

	// Token: 0x04001C6D RID: 7277
	public Transform m_lineAttachPoints0;

	// Token: 0x04001C6E RID: 7278
	public Transform m_lineAttachPoints1;

	// Token: 0x04001C6F RID: 7279
	public Vector3 m_lineAttachOffset = new Vector3(0f, 1f, 0f);

	// Token: 0x04001C70 RID: 7280
	public float m_breakForce = 10000f;

	// Token: 0x04001C71 RID: 7281
	public float m_spring = 5000f;

	// Token: 0x04001C72 RID: 7282
	public float m_springDamping = 1000f;

	// Token: 0x04001C73 RID: 7283
	public float m_baseMass = 20f;

	// Token: 0x04001C74 RID: 7284
	public float m_itemWeightMassFactor = 1f;

	// Token: 0x04001C75 RID: 7285
	public AudioSource[] m_wheelLoops;

	// Token: 0x04001C76 RID: 7286
	public float m_minPitch = 1f;

	// Token: 0x04001C77 RID: 7287
	public float m_maxPitch = 1.5f;

	// Token: 0x04001C78 RID: 7288
	public float m_maxPitchVel = 10f;

	// Token: 0x04001C79 RID: 7289
	public float m_maxVol = 1f;

	// Token: 0x04001C7A RID: 7290
	public float m_maxVolVel = 10f;

	// Token: 0x04001C7B RID: 7291
	public float m_audioChangeSpeed = 2f;

	// Token: 0x04001C7C RID: 7292
	public Rigidbody[] m_wheels = new Rigidbody[0];

	// Token: 0x04001C7D RID: 7293
	public List<Vagon.LoadData> m_loadVis = new List<Vagon.LoadData>();

	// Token: 0x04001C7E RID: 7294
	private ZNetView m_nview;

	// Token: 0x04001C7F RID: 7295
	private ConfigurableJoint m_attachJoin;

	// Token: 0x04001C80 RID: 7296
	private Rigidbody m_body;

	// Token: 0x04001C81 RID: 7297
	private LineRenderer m_lineRenderer;

	// Token: 0x04001C82 RID: 7298
	private Rigidbody[] m_bodies;

	// Token: 0x04001C83 RID: 7299
	private Humanoid m_useRequester;

	// Token: 0x020002BA RID: 698
	[Serializable]
	public class LoadData
	{
		// Token: 0x04001C84 RID: 7300
		public GameObject m_gameobject;

		// Token: 0x04001C85 RID: 7301
		public float m_minPercentage;
	}
}
