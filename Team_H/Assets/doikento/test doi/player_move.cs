using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_move : MonoBehaviour
{
    private float speed = 0.01f;//プレイヤーの移動速度

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //移動処理
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

        // 現在位置のオブジェクトを取得
        Collider2D hit = Physics2D.OverlapPoint(transform.position);

        if (hit != null)
        {
            Debug.Log("現在の位置にあるオブジェクトのタグ: " + hit.tag);
        }
        else
        {
            Debug.Log("現在の位置には何もありません");
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("接触中のタグ: " + other.tag);
    }
}
