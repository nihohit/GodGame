using UnityEngine;

public class CameraControl : MonoBehaviour {
    // Zooming
    public float dampTime = 0.2f;                 // Approximate time for the camera to refocus.
    public float minZoom = 15f; // The smallest orthographic size the camera can be.
    public float maxZoom = 40f;
    public float zoomcChangePerFrame = 25f;
    
    // Moving
    public float moveSpeed = 20f;

    // Rotating
    public float rotationSpeed = 30f;

    // Pitch
    public float pitchSpeed = 10f;

    private Camera cameraObject;                        // Used for referencing the camera.
    private float zoomSpeed;                      // Reference speed for the smooth damping of the orthographic size.
    private Vector3 moveVelocity;                 // Reference velocity for the smooth damping of the position.
    private Vector3 desiredPosition;              // The position the camera is moving towards.


    private void Awake() {

        cameraObject = GetComponentInChildren<Camera>();
    }


    private void FixedUpdate() {
        // Move the camera towards a desired position.
        move();

        // Change the size of the camera based.
        zoom();

        rotate();

        rotateAroundcenter();

        pitch();
    }


    private void move() {
        var movementDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) {
            movementDirection += Vector3.right;
            movementDirection += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.A)) {
            movementDirection += Vector3.forward;
            movementDirection += Vector3.left;
        }
        if (Input.GetKey(KeyCode.S)) {
            movementDirection += Vector3.left;
            movementDirection += Vector3.back;
        }
        if (Input.GetKey(KeyCode.D)) {
            movementDirection += Vector3.back;
            movementDirection += Vector3.right;
        }
        movementDirection = movementDirection * moveSpeed * Time.deltaTime;
        transform.parent.position += movementDirection;
    }


    private void zoom() {
        float zoomChange = Input.mouseScrollDelta.y;
        
        // Find the required size based on the desired position and smoothly transition to that size.
        float requiredSize = cameraObject.fieldOfView - zoomChange * zoomcChangePerFrame;
        cameraObject.fieldOfView = Mathf.SmoothDamp(cameraObject.fieldOfView, requiredSize, ref zoomSpeed, dampTime);
        cameraObject.fieldOfView = Mathf.Clamp(cameraObject.fieldOfView, minZoom, maxZoom);
    }

    void rotate() {
        float yawChange = 0;
        if (Input.GetMouseButton(2)) {
            yawChange += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        }

        transform.Rotate(Vector3.up * yawChange);
    }

    void rotateAroundcenter() {
        Vector3 rotationDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.Q)) {
            rotationDirection += Vector3.left;
        }
        if (Input.GetKey(KeyCode.E)) {
            rotationDirection += Vector3.right;
        }
        transform.LookAt(transform.parent);
        transform.Translate(rotationDirection * Time.deltaTime * rotationSpeed);
    }

    void pitch() {
        if (!Input.GetMouseButton(2)) {
            return;
        }

        float pitchChange = Input.GetAxis("Mouse Y") * pitchSpeed * Time.deltaTime;
        transform.position += Vector3.up * pitchChange;
    }
}