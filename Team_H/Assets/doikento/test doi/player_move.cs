using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_move : MonoBehaviour
{
    public float speed = 0.01f;//�v���C���[�̈ړ����x
    private Rigidbody2D rb;
    private Collider2D currentTarget; // ���G��Ă���I�u�W�F�N�g���L�^����
    private bool canmove = true;

    [Header("���̃X�v���C�g�����ւ��p")]
    [SerializeField] private Sprite plowedSprite;  // �k������̌�����
    [SerializeField] private Sprite wateredSprite; // ������̌�����
    [SerializeField] private Sprite seedSprite;�@�@//���A������̌�����
    [SerializeField] private Sprite grownSprite;   //������
    [SerializeField] private Sprite plowSprite;    //���n��̌�����
    [SerializeField] private Sprite dischargeSprite;     //���d㩐ݒu��

    [Header("�����ɂ����鎞��(�b)")]
    [SerializeField] private float growTime = 5f;   // �킪��܂ł̎���

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Rigidbody2D �̐ݒ�
        rb.gravityScale = 0;  // 2D�̏�����d�͂��s�v�Ȃ�I�t
        rb.freezeRotation = true; // ��]���Œ�i�Փˎ��ɉ�]���Ȃ��j
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

        
    }
    void DoActon()
    {
        //�w��^�O�ŃX�y�[�X�L�[�����������̏���
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentTarget != null)
            {
                SpriteRenderer sr = currentTarget.GetComponent<SpriteRenderer>();
                if (sr == null) return; // SpriteRenderer���Ȃ��ꍇ�͉������Ȃ�

                if (currentTarget.CompareTag("Plow"))
                {
                    Debug.Log("�����k��");
                    sr.sprite = plowedSprite; // �O���t�B�b�N�ύX
                    currentTarget.tag = "Plowed"; // �^�O���ύX
                }
                else if (currentTarget.CompareTag("Plowed"))
                {
                    Debug.Log("���ɐ���������");
                    sr.sprite = wateredSprite; // �O���t�B�b�N�ύX
                    currentTarget.tag = "Moist_Plowe"; // �^�O���ύX
                }
                else if (currentTarget.CompareTag("Moist_Plowe"))
                {
                    Debug.Log("���Ɏ��A����");
                    sr.sprite = seedSprite; // �O���t�B�b�N�ύX
                    currentTarget.tag = "Seed"; // �^�O���ύX

                    //StartCoroutine�Ő����J�n
                    StartCoroutine(GrowPlant(currentTarget, sr));
                }
                else if (currentTarget.CompareTag("Grown"))
                {
                    Debug.Log("�앨�����n");
                    sr.sprite = plowSprite;//�O���t�B�b�N�ύX
                    currentTarget.tag = "Plow";//�^�O���ύX
                }
                else if (currentTarget.CompareTag("Grassland"))
                {
                    Debug.Log("㩐ݒu");
                    sr.sprite = dischargeSprite;//�O���t�B�b�N�ύX
                    currentTarget.tag = "Discharge";//�^�O�ύX
                }
            }
        }
    }

    //�앨�̐�������
    private IEnumerator GrowPlant(Collider2D target, SpriteRenderer sr)
    {
        yield return new WaitForSeconds(growTime);//�����҂�

        if (target != null && target.CompareTag("Seed"))
        {
            Debug.Log("�앨������");
            sr.sprite = grownSprite;//�O���t�B�b�N�ύX
            target.tag = "Grown";//�^�O�ύX
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("�ڐG���̃^�O: " + other.tag);
        currentTarget = other; // ���ݐG��Ă���I�u�W�F�N�g���L�^
    }

    // ���ꂽ�Ƃ��ɉ���
    void OnTriggerExit2D(Collider2D other)
    {
        if (currentTarget == other)
        {
            currentTarget = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject .CompareTag("Wall"))
        {
            Debug.Log("��Q���ɐڐG");
            canmove = false;
            rb.linearVelocity = Vector2.zero;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("��Q�����痣��܂���");
            canmove = true;
        }
    }


}

