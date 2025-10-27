
using UnityEngine;

<<<<<<< HEAD:Team_H/Assets/OkunoRyota/PlayerCont.cs

public class PlayerCont : MonoBehaviour
=======
public class NewMonoBehaviourScript : MonoBehaviour
>>>>>>> 70466030a6997ef5dfa1b102695defa98fe17270:Team_H/Assets/OkunoRyota/NewMonoBehaviourScript.cs
{
    public float speed = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput) * speed * Time.deltaTime;

        transform.position += movement;
    }
}
