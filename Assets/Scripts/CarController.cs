using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody theRB;

    public float groundAccel = 2f, forwardAccel = 8f, reverseAccel = 4f, turnAccel = 4f, maxSpeed = 50f, turnStrength = 180, gravityForce = 10f, dragOnGround = 3f;

    private float speedInput, turnInput, direction;
    private int delay = 20, count = 0;
    public static float playerProgress = 0;
    public static float playerLap = 0;

    private bool grounded, track;

    public List<Transform> waypoints = new List<Transform>();
    private Transform targetWaypoint;
    public int targetWaypointIndex = 0;
    public float minDistance = 7f; //If the distance between the enemy and the waypoint is less than this, then it has reacehd the waypoint
    private int lastWaypointIndex;
    public float dirToWaypoint;
    public float distance;

    public LayerMask whatIsGround;
    public float groundRayLenght = .5f;
    public Transform groundRayPoint;

    public LayerMask whatIsTrack;
    public float trackRayLenght = .5f;

    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;

    public ParticleSystem[] dustTrail;
    public float maxEmission = 25f;
    private float emissionRate;

    // Start is called before the first frame update
    void Start()
    {
        theRB.transform.parent = null;

        lastWaypointIndex = waypoints.Count - 1;
        targetWaypoint = waypoints[targetWaypointIndex]; //Set the first target waypoint at the start so the enemy starts moving towards a waypoint

    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(transform.position, targetWaypoint.position);
        CheckDistanceToWaypoint(distance);
        playerProgress = (targetWaypointIndex + (60 - distance)/60)/(lastWaypointIndex + 1);

        speedInput = groundAccel * 1000f;
        count = count + 1;
        if(track)
        {
              speedInput = forwardAccel * 1000f;      
        }

        direction = 1f;
        if (Input.GetAxis("Vertical") < 0)
        {
            speedInput = Input.GetAxis("Vertical") * reverseAccel * 1000f;
            direction = -1f;
        }

        //android control
        turnInput = 0;
        if (Input.touchCount > 0) {
            if (Input.GetTouch (0).position.x > Screen.width / 2) {
                turnInput = 1;
            } else {
                turnInput = -1;
            }
        } else {
            turnInput = Input.GetAxis("Horizontal");
        }
        if (turnInput != 0)
        {
                if(track)
            {
              speedInput =  turnAccel * 1000f;   
            }
        }

        if (count < delay) {
            speedInput = 0f;
            turnInput = 0f;
        }

        if(grounded)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * direction, 0f));
            //theRB.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * direction, 0f));
        }

        leftFrontWheel.transform.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.transform.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, turnInput * maxWheelTurn, rightFrontWheel.localRotation.eulerAngles.z);

        transform.position = theRB.transform.position;


    }

    private void FixedUpdate()
    {
        grounded = false;
        track = false;
        RaycastHit hit;

        if(Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLenght, whatIsGround))
        {
            grounded = true;

            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            //theRB.transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }

        if(Physics.Raycast(groundRayPoint.position, -transform.up, out hit, trackRayLenght, whatIsTrack))
        {
            track = true;
            grounded = true;

            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            //theRB.transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }

        emissionRate = 0;

        if(grounded)
        {
            theRB.drag = dragOnGround;
            theRB.transform.rotation = transform.rotation;
            if(Mathf.Abs(speedInput) > 0)
            {
                theRB.AddForce(transform.forward * speedInput);

                emissionRate = maxEmission;
            }
        } else
        {
            theRB.drag = 0.1f;

            theRB.AddForce(Vector3.up * -gravityForce * 100f);
        }

        foreach(ParticleSystem part in dustTrail)
        {
            var emissionModule = part.emission;
            emissionModule.rateOverTime = emissionRate;
        }
    }
        void CheckDistanceToWaypoint(float currentDistance)
    {
        if(currentDistance <= minDistance)
        {
            targetWaypointIndex++;
            UpdateTargetWaypoint();
        }
    }


    /// Increaes the index of the target waypoint. If the enemy has reached the last waypoint in the waypoints list, it resets the targetWaypointIndex to the first waypoint in the list (causes the enemy to loop)
    void UpdateTargetWaypoint()
    {
        if(targetWaypointIndex > lastWaypointIndex)
        {
            targetWaypointIndex = 0;
            playerLap++;
        }

        targetWaypoint = waypoints[targetWaypointIndex];
    }
}
