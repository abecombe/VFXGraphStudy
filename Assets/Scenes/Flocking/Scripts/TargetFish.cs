using UnityEngine;

public class TargetFish : MonoBehaviour
{
    private Vector2 prevPosition = Vector2.zero;

    Vector2 GetMousePosition()
    {
        var MainCamera = Camera.main.gameObject.GetComponent<Camera>();
        var height = -MainCamera.transform.position.z * Mathf.Tan(MainCamera.fieldOfView * 0.5f * Mathf.PI / 180f) * 2f;
        var width = height * Screen.width / Screen.height;
        var position = Input.mousePosition / new Vector2(Screen.width, Screen.height) - Vector2.one * 0.5f;
        //if (Mathf.Max(Mathf.Abs(position.x), Mathf.Abs(position.y)) > 0.5f)
        //    position = Vector2.zero;
        position *= new Vector2(width, height);

        return position;
    }
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        var position = GetMousePosition() * 0.1f + prevPosition * 0.9f;
        transform.position = position;
        var deltaPosition = position - prevPosition;

        var velocity = new Vector3(deltaPosition.x, deltaPosition.y, -0.012f) / Time.deltaTime;

        transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);

        GetComponent<Animator>().speed = Mathf.Pow(velocity.magnitude, 0.6f) + 1.2f;

        prevPosition = position;
    }
}
