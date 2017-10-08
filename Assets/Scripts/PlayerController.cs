using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(PlayerInput))]
[RequireComponent (typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

    [Header ("Movement Settings")]
    [Range (0f, 10f)]
    public float moveSpeed;
    [Range (0f, 10f)]
    public float lookSpeed;
    [Range (0f, 10f)]
    public float turnSpeed;
    [Range (0f, 5f)]
    public float jumpHeight;
    [Range (0f, 1f)]
    public float velocityDampen;
    [Range (0f, 20f)]
    public float maxVelocity;

    [Header ("Air Movement Settings")]
    [Range (0f, 5f)]
    public float airMoveSpeed;
    [Range (0f, 5f)]
    public float airTurnSpeed;

    [Header("Camera Settings")]
    public float dampening = 1f;
    [Range (3f, 20f)]
    public float camMaxDistance;
    [Range (0f, 5f)]
    public float camMinDistance;

    [Header("Debug Settings")]
    public bool debugOn = false;
    public bool debugVelocity = false;
    public bool debugPoleArm = false;

    //General
    private CharacterController cont;
    private Rigidbody rb;
    private Vector3 velocity;
    private Vector3 moveDir;
    private Vector3 lookDir;

    //Camera
    public GameObject cam;
    public GameObject camRoot;
    public GameObject camPoleArm;
    public GameObject camPivot;

    private float lastHitDistance;

	private void Start () {
        rb = GetComponent<Rigidbody>();
        velocity = Vector2.zero;
        cont = GetComponent<CharacterController>();

        lastHitDistance = 0f;

        Debug.Assert(rb != null, "Could not find Rigidbody component");
        Debug.Assert(cont != null, "Could not find CharacterController component");

        Debug.Assert(cam != null, "Could not find Camera!");
        Debug.Assert(camRoot != null, "Could not find Camera Root!");
        Debug.Assert(camPoleArm != null, "Could not find Camera Pole Arm");
        Debug.Assert(camPivot != null, "Could not find Camera Pivot!");

        Debug.Assert(camMinDistance <= camMaxDistance, "Camera Min Distance cannot be greater than Camera Max Distance");
    }
	
	private void FixedUpdate () {
        //Dampen Velocity
        velocity = cont.isGrounded ? new Vector3 (velocity.x * velocityDampen, velocity.y, velocity.z * velocityDampen) : velocity;

        //Gravity
        if (!cont.isGrounded)
        {
            velocity += Physics.gravity * Time.deltaTime;
        }

        //Movement
        if (cont.isGrounded)
        {
            velocity += moveDir * moveSpeed;
        }
        else
        {
            velocity += moveDir * airMoveSpeed;
        }

        velocity = Vector3.ClampMagnitude(velocity, maxVelocity);

        cont.Move(velocity * Time.deltaTime);

        //Reset movement direction
        moveDir = Vector3.zero;

        //Debug
        if (debugOn) DrawDebug();
	}

    private void LateUpdate()
    {
        //Camera Movement
        float curAngle = camPivot.transform.eulerAngles.y;
        float targetAngleY = camPivot.transform.eulerAngles.y + lookDir.x * lookSpeed;
        float targetAngleX = camPivot.transform.eulerAngles.x + lookDir.y * lookSpeed;
        targetAngleX -= targetAngleX > 90f ? 360f : 0f;

        //Clamp Angles
        targetAngleX = Mathf.Clamp(targetAngleX, -85f, 85f);

        //Calculate final angle
        Quaternion targetRot = Quaternion.Euler(targetAngleX, targetAngleY, 0f);
        Quaternion rotation = Quaternion.Lerp(camPivot.transform.rotation, targetRot, dampening);
        camPivot.transform.rotation = rotation;

        //Camera Pole Arm
        Ray poleRay = new Ray(camRoot.transform.localPosition, camPivot.transform.localRotation * -Vector3.forward);
        Vector3 curPos = camPoleArm.transform.localPosition;
        Vector3 targetPos;
        
        RaycastHit hit;
        if (Physics.Raycast(poleRay, out hit, camMaxDistance))
        {
            targetPos = curPos.normalized * hit.distance;
            targetPos = Vector3.ClampMagnitude(targetPos, camMaxDistance);
            if (targetPos.magnitude <= camMinDistance)
                targetPos = targetPos.normalized * camMinDistance;
        } else
        {
            targetPos = curPos.normalized * camMaxDistance;
        }
        float zoomDampening = hit.distance <= 0.1f ? 1f * Time.deltaTime : 1f;
        if (hit.distance >= 0.1f && hit.distance - lastHitDistance > 0f)
        {
            zoomDampening = 1f * Time.deltaTime;
        }
        lastHitDistance = hit.distance;
        camPoleArm.transform.localPosition = Vector3.Lerp(curPos, targetPos, zoomDampening);
    }

    public void Move (Vector2 input)
    {
        moveDir = new Vector3(input.x, 0f, input.y);
    }

    public void Look (Vector2 input)
    {
        lookDir = new Vector3(input.x, input.y, 0f);
    }

    public void Jump()
    {
        if (cont.isGrounded)
        {
            velocity.y = 0f;
            velocity.y += Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
        }
    }

    private void DrawDebug()
    {
        if (debugVelocity)
            Debug.DrawLine(transform.position, transform.position + velocity);
        
    }
}
