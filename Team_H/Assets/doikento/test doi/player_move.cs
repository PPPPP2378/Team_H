using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_move : MonoBehaviour
{
    private float speed = 0.01f;//�v���C���[�̈ړ����x

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //�ړ�����
        Vector2 position = transform.position;

        if (Input.GetKey("a"))
        {
            position.x -= speed;
        }
        else if (Input.GetKey("d"))
        {
            position.x += speed;
        }
        else if (Input.GetKey("w"))
        {
            position.y += speed;
        }
        else if (Input.GetKey("s"))
        {
            position.y -= speed;
        }

        transform.position = position;

        // ���݈ʒu�̃I�u�W�F�N�g���擾
        Collider2D hit = Physics2D.OverlapPoint(transform.position);

        if (hit != null)
        {
            Debug.Log("���݂̈ʒu�ɂ���I�u�W�F�N�g�̃^�O: " + hit.tag);
        }
        else
        {
            Debug.Log("���݂̈ʒu�ɂ͉�������܂���");
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("�ڐG���̃^�O: " + other.tag);
    }
}
