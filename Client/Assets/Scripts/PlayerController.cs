using System;
using UnityEngine;
using System.IO;
using System.Text;

public class PlayerController : MonoBehaviour
{
    public float speed = 10.0f; // 速度
    public float anSpeed;
    public event Action<int> OnCollision;
    public event Action<int> OnSpace;

    private float cameraX;
    private float cameraY;
    private Vector3 front;
    private Vector2 inputVec;
    private float inputAngle;
    private Vector3 dir;
    private bool C;
    private bool ANTA;
    void Start()
    {
        FileInfo fi = new FileInfo(Application.dataPath + "/" + "c.txt");
        StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8);
        if(sr.ReadToEnd() == "10")
        {
            C = true;
        }
        else
        {
            C = false;
        }
        if (sr.ReadToEnd() == "5")
        {
            ANTA = true;
        }
        else
        {
            ANTA = false;
        }
        cameraX = 0.0f;
        cameraY = 0.0f;
    }

    // 固定フレームレートで呼び出されるハンドラ
    void FixedUpdate()
    {

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        inputVec = new Vector2(moveHorizontal, moveVertical);
        float inputPower = inputVec.magnitude;

        if (!C)
        {
            if (Input.GetKey(KeyCode.J))
            {
                cameraX -= anSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.L))
            {
                cameraX += anSpeed * Time.deltaTime;
            }

           


            var rb = GetComponent<Rigidbody>();



            // カメラの設定
            gameObject.transform.GetChild(0).transform.rotation = Quaternion.identity;
            gameObject.transform.GetChild(0).transform.rotation = Quaternion.AngleAxis(cameraX, new Vector3(0, 1, 0));
            front = gameObject.transform.GetChild(0).transform.forward;

            if (Input.GetKey(KeyCode.W))
            {
                rb.AddForce(front * speed * inputPower);
            }
            if (Input.GetKey(KeyCode.S))
            {
                front = Quaternion.Euler(0, 180, 0) * front;
                rb.AddForce(front * speed * inputPower);
            }
            if (Input.GetKey(KeyCode.D))
            {
                front = Quaternion.Euler(0, 90, 0) * front;
                rb.AddForce(front * speed * inputPower);
            }
            if (Input.GetKey(KeyCode.A))
            {
                front = Quaternion.Euler(0, -90, 0) * front;
                rb.AddForce(front * speed * inputPower);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (ANTA)
                {
                    OnSpace(1);
                }

            }
        }
        else
        {
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            float Y = 0.0f;
            if (Input.GetKey(KeyCode.J))
            {
                cameraX -= anSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.L))
            {
                cameraX += anSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.I))
            {
                cameraY -= anSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.K))
            {
                cameraY += anSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                Y = 1.0f;
            }
            gameObject.transform.GetChild(0).transform.rotation = Quaternion.identity;
            //gameObject.transform.GetChild(0).transform.rotation = Quaternion.AngleAxis(cameraX,new Vector3(0,1,0));
            gameObject.transform.GetChild(0).transform.rotation = Quaternion.EulerRotation(cameraY * 0.01f, cameraX * 0.01f, 0);
            front = gameObject.transform.GetChild(0).transform.forward;

            gameObject.transform.position = gameObject.transform.position + new Vector3(moveHorizontal * inputPower, Y,moveVertical * inputPower);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var otherPlayerController = collision.gameObject.GetComponent<OtherPlayerController>();
        if (otherPlayerController != null)
        {
            OnCollision(otherPlayerController.Id);
        }
       
    }
}
