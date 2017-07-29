using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    // How many times should I be hit before I die
    public int health;
    // When the enemy dies, we play an explosion
    public Transform explosion;
    // What sound to play when hit
    public AudioClip hitSound;
    // The enemy shoot?
    public bool shoot;
    float timeBeforeShooting = 1.5f;
    float timeBetweenShooting = 2f;
    public Transform laser;
    public AudioClip shootSound;
    // Reference to our AudioSource component
    AudioSource audioSource;
    // Player's transform
    Transform player;
    // Enemy's speed
    public float speed = 2.0f;
    float step;
    // Enemy's points
    public int points;

    // Enemies colour
    GameObject enemyBlue;
    GameObject enemyRed;
    GameObject enemyBlack;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        player = GameObject.Find("Player Ship").transform;
        step = speed * Time.deltaTime;

        enemyBlue = this.transform.Find("Blue").gameObject;
        enemyRed = this.transform.Find("Red").gameObject;
        enemyBlack = this.transform.Find("Black").gameObject;
    }

    private void Start()
    {
        if (shoot)
        {
            StartCoroutine(Shoot());
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Player_Laser")
        {
            LaserBehaviour laser = collision.gameObject.GetComponent("LaserBehaviour") as LaserBehaviour;
            health -= laser.damage;
            Destroy(collision.gameObject);
            // Plays a sound from this object's AudioSource
            audioSource.PlayOneShot(hitSound);
        }

        if (health <= 0)
        {
            KillEnemy();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player && !PauseMenuBehaviour.isPaused)
        {
            Vector3 delta = player.position - transform.position;
            delta.Normalize();
            /*float moveSpeed = speed * Time.deltaTime;
            transform.position = transform.position + (delta * moveSpeed);
            transform.rotation = Quaternion.LookRotation(delta);*/

            // Move towards player
            transform.position = Vector3.MoveTowards(transform.position, player.position, step);

            // Rotate towards player
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
        }
    }

    // Shoot
    IEnumerator Shoot()
    {
        yield return new WaitForSeconds(timeBeforeShooting);
        while (true)
        {
            if (!PauseMenuBehaviour.isPaused)
            {
                // Play sound
                audioSource.PlayOneShot(shootSound);
                // We want to position the laser in relation to the enemy's location
                Vector3 laserPos = this.transform.position;
                // The angle the laser will move away from the center
                float rotationAngle = transform.localEulerAngles.z + 90;
                // Calculate the position right in front of the ship's position laserDistance units away
                laserPos.x += (Mathf.Cos((rotationAngle) * Mathf.Deg2Rad) * -.5f);
                laserPos.y += (Mathf.Sin((rotationAngle) * Mathf.Deg2Rad) * -.5f);
                Quaternion rotation = this.transform.rotation * Quaternion.Euler(0, 0, 180f);
                Instantiate(laser, laserPos, rotation);
            }

            yield return new WaitForSeconds(timeBetweenShooting);

        }
    }

    void KillEnemy()
    {
        Destroy(this.gameObject);
        GameController.instance.KilledEnemy();
        GameController.instance.IncreaseScore(points);
        // Check if explosion was set
        if (explosion)
        {
            GameObject exploder = ((Transform)Instantiate(explosion, this.transform.position, this.transform.rotation)).gameObject;
            Destroy(exploder, 2.0f);
        }
    }

    public void ActivateBlueEnemy()
    {
        enemyBlue.SetActive(true);
    }

    public void ActivateRedEnemy()
    {
        enemyRed.SetActive(true);
    }

    public void ActivateBlackEnemy()
    {
        enemyBlack.SetActive(true);
    }

    public void ActivateEnemyByColour(string colour)
    {
        switch (colour)
        {
            case "blue":
                ActivateBlueEnemy();
                break;
            case "red":
                ActivateRedEnemy();
                break;
            case "black":
                ActivateBlackEnemy();
                break;
            default:
                break;
        }
    }
}