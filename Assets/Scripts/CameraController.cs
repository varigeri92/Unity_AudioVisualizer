using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform CameraTransform;

    Vector2 mouseInput;
    float scrollInput;

    [SerializeField] float inputGravity;
    [SerializeField] float rotationSpeed;
    [SerializeField] float zoomSpeed;

    // Start is called before the first frame update
    void Start()
    {
        CameraTransform = transform.GetChild(0).transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            mouseInput = new Vector2(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
            mouseInput *= rotationSpeed;
        }
        else
        {
            mouseInput = Vector2.Lerp(mouseInput, Vector2.zero, inputGravity);

        }

        Vector3 newEuler = transform.rotation.eulerAngles;
        newEuler.x += mouseInput.x * Time.deltaTime;
        newEuler.y += mouseInput.y * Time.deltaTime;
        newEuler.z += 0;

        newEuler.x = Mathf.Clamp(newEuler.x, 0, 88);
       
        transform.rotation = Quaternion.Euler(newEuler);

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            scrollInput = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        }
        else
        {
            scrollInput = Mathf.Lerp(scrollInput, 0, inputGravity);
        }


        CameraTransform.Translate(Vector3.forward * scrollInput * Time.deltaTime);


    }
}
