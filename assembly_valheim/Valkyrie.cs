using System;
using UnityEngine;

// Token: 0x020002BB RID: 699
public class Valkyrie : MonoBehaviour
{
	// Token: 0x06001A72 RID: 6770 RVA: 0x000AF9C0 File Offset: 0x000ADBC0
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_animator = base.GetComponentInChildren<Animator>();
		if (!this.m_nview.IsOwner())
		{
			base.enabled = false;
			return;
		}
		ZLog.Log("Setting up valkyrie ");
		float f = UnityEngine.Random.value * 3.1415927f * 2f;
		Vector3 vector = new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
		Vector3 a = Vector3.Cross(vector, Vector3.up);
		Player.m_localPlayer.SetIntro(true);
		this.m_targetPoint = Player.m_localPlayer.transform.position + new Vector3(0f, this.m_dropHeight, 0f);
		Vector3 position = this.m_targetPoint + vector * this.m_startDistance;
		position.y = this.m_startAltitude;
		base.transform.position = position;
		this.m_descentStart = this.m_targetPoint + vector * this.m_startDescentDistance + a * 200f;
		this.m_descentStart.y = this.m_descentAltitude;
		Vector3 a2 = this.m_targetPoint - this.m_descentStart;
		a2.y = 0f;
		a2.Normalize();
		this.m_flyAwayPoint = this.m_targetPoint + a2 * this.m_startDescentDistance;
		this.m_flyAwayPoint.y = this.m_startAltitude;
		this.ShowText();
		this.SyncPlayer(true);
		ZLog.Log("World pos " + base.transform.position.ToString() + "   " + ZNet.instance.GetReferencePosition().ToString());
	}

	// Token: 0x06001A73 RID: 6771 RVA: 0x000AFB8C File Offset: 0x000ADD8C
	private void ShowText()
	{
		TextViewer.instance.ShowText(TextViewer.Style.Intro, this.m_introTopic, this.m_introText, false);
	}

	// Token: 0x06001A74 RID: 6772 RVA: 0x000023E2 File Offset: 0x000005E2
	private void HideText()
	{
	}

	// Token: 0x06001A75 RID: 6773 RVA: 0x000AFBA6 File Offset: 0x000ADDA6
	private void OnDestroy()
	{
		ZLog.Log("Destroying valkyrie");
	}

	// Token: 0x06001A76 RID: 6774 RVA: 0x000AFBB2 File Offset: 0x000ADDB2
	private void FixedUpdate()
	{
		this.UpdateValkyrie(Time.fixedDeltaTime);
		if (!this.m_droppedPlayer)
		{
			this.SyncPlayer(true);
		}
	}

	// Token: 0x06001A77 RID: 6775 RVA: 0x000AFBCE File Offset: 0x000ADDCE
	private void LateUpdate()
	{
		if (!this.m_droppedPlayer)
		{
			this.SyncPlayer(false);
		}
	}

	// Token: 0x06001A78 RID: 6776 RVA: 0x000AFBE0 File Offset: 0x000ADDE0
	private void UpdateValkyrie(float dt)
	{
		this.m_timer += dt;
		if (this.m_timer < this.m_startPause)
		{
			return;
		}
		Vector3 vector;
		if (this.m_droppedPlayer)
		{
			vector = this.m_flyAwayPoint;
		}
		else if (this.m_descent)
		{
			vector = this.m_targetPoint;
		}
		else
		{
			vector = this.m_descentStart;
		}
		if (Utils.DistanceXZ(vector, base.transform.position) < 0.5f)
		{
			if (!this.m_descent)
			{
				this.m_descent = true;
				ZLog.Log("Starting descent");
			}
			else if (!this.m_droppedPlayer)
			{
				ZLog.Log("We are here");
				this.DropPlayer();
			}
			else
			{
				this.m_nview.Destroy();
			}
		}
		Vector3 normalized = (vector - base.transform.position).normalized;
		Vector3 vector2 = base.transform.position + normalized * 25f;
		float num;
		if (ZoneSystem.instance.GetGroundHeight(vector2, out num))
		{
			vector2.y = Mathf.Max(vector2.y, num + this.m_dropHeight);
		}
		Vector3 normalized2 = (vector2 - base.transform.position).normalized;
		Quaternion quaternion = Quaternion.LookRotation(normalized2);
		Vector3 to = normalized2;
		to.y = 0f;
		to.Normalize();
		Vector3 forward = base.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		float num2 = Mathf.Clamp(Vector3.SignedAngle(forward, to, Vector3.up), -30f, 30f) / 30f;
		quaternion = Quaternion.Euler(0f, 0f, num2 * 45f) * quaternion;
		float num3 = this.m_droppedPlayer ? (this.m_turnRate * 4f) : this.m_turnRate;
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, num3 * dt);
		Vector3 a = base.transform.forward * this.m_speed;
		Vector3 vector3 = base.transform.position + a * dt;
		float num4;
		if (ZoneSystem.instance.GetGroundHeight(vector3, out num4))
		{
			vector3.y = Mathf.Max(vector3.y, num4 + this.m_dropHeight);
		}
		base.transform.position = vector3;
	}

	// Token: 0x06001A79 RID: 6777 RVA: 0x000AFE34 File Offset: 0x000AE034
	private void DropPlayer()
	{
		ZLog.Log("We are here");
		this.m_droppedPlayer = true;
		Vector3 forward = base.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		Player.m_localPlayer.transform.rotation = Quaternion.LookRotation(forward);
		Player.m_localPlayer.SetIntro(false);
		this.m_animator.SetBool("dropped", true);
	}

	// Token: 0x06001A7A RID: 6778 RVA: 0x000AFEA4 File Offset: 0x000AE0A4
	private void SyncPlayer(bool doNetworkSync)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			ZLog.LogWarning("No local player");
			return;
		}
		localPlayer.transform.rotation = this.m_attachPoint.rotation;
		localPlayer.transform.position = this.m_attachPoint.position - localPlayer.transform.TransformVector(this.m_attachOffset);
		localPlayer.GetComponent<Rigidbody>().position = localPlayer.transform.position;
		if (doNetworkSync)
		{
			ZNet.instance.SetReferencePosition(localPlayer.transform.position);
			localPlayer.GetComponent<ZSyncTransform>().SyncNow();
			base.GetComponent<ZSyncTransform>().SyncNow();
		}
	}

	// Token: 0x04001C86 RID: 7302
	public float m_startPause = 10f;

	// Token: 0x04001C87 RID: 7303
	public float m_speed = 10f;

	// Token: 0x04001C88 RID: 7304
	public float m_turnRate = 5f;

	// Token: 0x04001C89 RID: 7305
	public float m_dropHeight = 10f;

	// Token: 0x04001C8A RID: 7306
	public float m_startAltitude = 500f;

	// Token: 0x04001C8B RID: 7307
	public float m_descentAltitude = 100f;

	// Token: 0x04001C8C RID: 7308
	public float m_startDistance = 500f;

	// Token: 0x04001C8D RID: 7309
	public float m_startDescentDistance = 200f;

	// Token: 0x04001C8E RID: 7310
	public Vector3 m_attachOffset = new Vector3(0f, 0f, 1f);

	// Token: 0x04001C8F RID: 7311
	public float m_textDuration = 5f;

	// Token: 0x04001C90 RID: 7312
	public string m_introTopic = "";

	// Token: 0x04001C91 RID: 7313
	[TextArea]
	public string m_introText = "";

	// Token: 0x04001C92 RID: 7314
	public Transform m_attachPoint;

	// Token: 0x04001C93 RID: 7315
	private Vector3 m_targetPoint;

	// Token: 0x04001C94 RID: 7316
	private Vector3 m_descentStart;

	// Token: 0x04001C95 RID: 7317
	private Vector3 m_flyAwayPoint;

	// Token: 0x04001C96 RID: 7318
	private bool m_descent;

	// Token: 0x04001C97 RID: 7319
	private bool m_droppedPlayer;

	// Token: 0x04001C98 RID: 7320
	private Animator m_animator;

	// Token: 0x04001C99 RID: 7321
	private ZNetView m_nview;

	// Token: 0x04001C9A RID: 7322
	private float m_timer;
}
