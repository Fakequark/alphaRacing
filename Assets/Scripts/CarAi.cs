using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAi : MonoBehaviour
{
    public Rigidbody theRB;

    public float groundAccel = 2f, forwardAccel = 8f, reverseAccel = 4f, turnAccel = 4f, maxSpeed = 50f, turnStrength = 180, gravityForce = 10f, dragOnGround = 3f;
    public int delay = 20;
    public float speedInput, turnInput, direction;
    public float progress = 0;
    public float lap = 0;
    public float deltaPlayer = 0;

    public bool grounded, track;

    public List<Transform> waypoints = new List<Transform>();
    private Transform targetWaypoint;
    public int targetWaypointIndex = 0;
    public float minDistance = 10f; //If the distance between the enemy and the waypoint is less than this, then it has reacehd the waypoint
    private int lastWaypointIndex;
    public float dirToWaypoint;
    public float dirTol = 7f;
    public float dirSlope = 10f;
    public float noise = 20f;
    public float distance;
    public float skill = 0;

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

    public float randNoise;
    public int count = 0;

    Quaternion rotationToTarget;
    private GameObject Player;

    // Start is called before the first frame update
    void Start()
    {
        //Physics
        theRB.transform.parent = null;

        Player = GameObject.Find("Car"); //finds player to compare
        //Waypoints
        rotationToTarget = new Quaternion();
        lastWaypointIndex = waypoints.Count - 1;
        targetWaypoint = waypoints[targetWaypointIndex]; //Set the first target waypoint at the start so the enemy starts moving towards a waypoint
    }

    // Update is called once per frame
    void Update()
    {
        // Waypoints
        Vector3 directionToTarget = targetWaypoint.position - transform.position;
        rotationToTarget.SetFromToRotation(transform.forward, directionToTarget);
        //Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);   
        count = count + 1;
        if (count == 100) 
        {
            count = delay;
            randNoise = Random.Range(-noise, noise);
            deltaPlayer = (progress + lap) - (CarController.playerProgress + CarController.playerLap);
            skill =  Mathf.Clamp((deltaPlayer + 0.24f)*0.5f, 0, 1f);
            dirSlope = skill*3f;
            noise = skill*14f;
        }
        dirToWaypoint = rotationToTarget.eulerAngles[1] + randNoise;
        if (dirToWaypoint > 180)
        {
            dirToWaypoint = dirToWaypoint - 360;
        }

        Debug.DrawRay(transform.position, transform.forward * 50f, Color.green, 0f); //Draws a ray forward in the direction the enemy is facing
        Debug.DrawRay(transform.position, directionToTarget, Color.red, 0f); //Draws a ray in the direction of the current target waypoint
        distance = Vector3.Distance(transform.position, targetWaypoint.position);
        CheckDistanceToWaypoint(distance);
        progress = (targetWaypointIndex + (60 - distance)/60)/(lastWaypointIndex + 1);
        
        // Car physics and control
        speedInput = groundAccel * 1000f;

        //turnInput = Input.GetAxis("Horizontal");
        if (dirToWaypoint < -dirTol)
        {
            turnInput = -1f * Mathf.Min(1f,-(dirToWaypoint + dirTol)/dirSlope);
        } else if (dirToWaypoint > dirTol)
        {
            turnInput = Mathf.Min(1f,(dirToWaypoint - dirTol)/dirSlope);
        } else {
            turnInput = 0;
        }
        
        if(track)
        {
              speedInput = forwardAccel * 1000f;      
        }


        if (turnInput != 0)
        {
                if(track)
            {
              speedInput =  turnAccel * 1000f;   
            }
        }


        direction = 1f;
        

        if (count < delay) {
            speedInput = 0f;
            turnInput = 0f;
        }

        if(grounded)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * direction, 0f));
            //theRB.transform.rotation = Quaternion.Euler(theRB.transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * direction, 0f));
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


    // Checks to see if the enemy is within distance of the waypoint. If it is, it called the UpdateTargetWaypoint function 

    // currentDistance The enemys current distance from the waypoint
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
            lap++;

        }

        targetWaypoint = waypoints[targetWaypointIndex];
    }

    public virtual float GetProgress() {
        return (progress + lap);
    }
}
