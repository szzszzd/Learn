using System;
using System.Threading;
using UnityEngine;

// Token: 0x020002A8 RID: 680
public class TestSceneCharacter : MonoBehaviour
{
	// Token: 0x060019E8 RID: 6632 RVA: 0x000ABE8A File Offset: 0x000AA08A
	private void Start()
	{
		this.m_body = base.GetComponent<Rigidbody>();
	}

	// Token: 0x060019E9 RID: 6633 RVA: 0x000ABE98 File Offset: 0x000AA098
	private void Update()
	{
		Thread.Sleep(30);
		this.HandleInput(Time.deltaTime);
	}

	// Token: 0x060019EA RID: 6634 RVA: 0x000ABEAC File Offset: 0x000AA0AC
	private void HandleInput(float dt)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		zero.x = Input.GetAxis("Mouse X");
		zero.y = Input.GetAxis("Mouse Y");
		if (Input.GetKey(KeyCode.Mouse1) || Cursor.lockState != CursorLockMode.None)
		{
			this.m_lookYaw *= Quaternion.Euler(0f, zero.x, 0f);
			this.m_lookPitch = Mathf.Clamp(this.m_lookPitch - zero.y, -89f, 89f);
		}
		if (Input.GetKeyDown(KeyCode.F1))
		{
			if (Cursor.lockState == CursorLockMode.None)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
			}
		}
		Vector3 a = Vector3.zero;
		if (Input.GetKey(KeyCode.A))
		{
			a -= base.transform.right * this.m_speed;
		}
		if (Input.GetKey(KeyCode.D))
		{
			a += base.transform.right * this.m_speed;
		}
		if (Input.GetKey(KeyCode.W))
		{
			a += base.transform.forward * this.m_speed;
		}
		if (Input.GetKey(KeyCode.S))
		{
			a -= base.transform.forward * this.m_speed;
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			this.m_body.AddForce(Vector3.up * 10f, ForceMode.VelocityChange);
		}
		Vector3 force = a - this.m_body.velocity;
		force.y = 0f;
		this.m_body.AddForce(force, ForceMode.VelocityChange);
		base.transform.rotation = this.m_lookYaw;
		Quaternion rotation = this.m_lookYaw * Quaternion.Euler(this.m_lookPitch, 0f, 0f);
		mainCamera.transform.position = base.transform.position - rotation * Vector3.forward * this.m_cameraDistance;
		mainCamera.transform.LookAt(base.transform.position + Vector3.up);
	}

	// Token: 0x04001BC3 RID: 7107
	public float m_speed = 5f;

	// Token: 0x04001BC4 RID: 7108
	public float m_cameraDistance = 10f;

	// Token: 0x04001BC5 RID: 7109
	private Rigidbody m_body;

	// Token: 0x04001BC6 RID: 7110
	private Quaternion m_lookYaw = Quaternion.identity;

	// Token: 0x04001BC7 RID: 7111
	private float m_lookPitch;
}
