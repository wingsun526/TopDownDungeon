using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Enemy : LivingThings
{
    [SerializeField] protected float speed = 1.0f;
    [SerializeField] protected float chaseLength = 0.5f;
    [SerializeField] private float pushRecoveryTime = 0.8f;

    protected Transform playerTransform;
    protected Rigidbody2D myRigidbody2D;
    protected Animator myAnimator;
    protected bool beingPush = false;
    private float lastPush;
    protected bool lockSpriteFlip = false;
    private bool spawningDoNotMove = true;
    
     
    

    protected virtual void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        playerTransform = GameManager.instance.GetPlayerMovement().GetComponent<Transform>();
        StartCoroutine(Spawning());
    }

    
    protected virtual void Update()
    {
        if (spawningDoNotMove) return;
        FlipSprite();
        FreeToMove();
        IndividualAction();
    }

    IEnumerator Spawning()
    {
        yield return new WaitForSeconds(3);
        spawningDoNotMove = false;
    }

    private void FreeToMove()
    {
        if (Time.time - lastPush > pushRecoveryTime)
        {
            beingPush = false;
        }
    }

    protected virtual void IndividualAction()
    {
        
        bool enemyIsMoving = Mathf.Abs(myRigidbody2D.velocity.x) + Mathf.Abs(myRigidbody2D.velocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("isMoving", enemyIsMoving);
        
        if (beingPush)
        {
            return;
        }

        
        // start chasing player when player is in range(chaseLength)
        if (Vector2.Distance(playerTransform.position, myRigidbody2D.transform.position) < chaseLength)
        {
            
            Vector3 delta = playerTransform.position - myRigidbody2D.transform.position;
            myRigidbody2D.velocity = new Vector2(delta.x, delta.y).normalized * speed;
            
            
            
        }
        

    }
    
    private void FlipSprite()
    {
        if (lockSpriteFlip) return;
        
        if (myRigidbody2D.position.x > playerTransform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (myRigidbody2D.position.x < playerTransform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void ReceiveDamage(DamageData dmgData)
    {
        beingPush = true;
        lastPush = Time.time;
        //string msg, int fontSize, Color color, Vector3 position, Vector3 motion, float duration
        FloatingDamageTextManager.instance.Show(dmgData.damage.ToString(), 20, new Color(1f, 0.8f, 0.17f), myRigidbody2D.transform.position, Vector3.up * 40, 1f);
        health -= dmgData.damage;
        GameManager.instance.OnScoreChange(dmgData.damage);
        if (health <= 0) 
        {
            Die();
        }
    }
    
    protected override void Die()
    {
        Destroy(gameObject);
        GameManager.instance.OnEnemyDestroy();
    }
}
