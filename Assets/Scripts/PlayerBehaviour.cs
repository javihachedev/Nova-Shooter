using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerBehaviour : MonoBehaviour
{

    // Singleton
    public static PlayerBehaviour instance;

    public float playerSpeed; // Speed
    public Transform laser; // Laser transform
    public float laserDistance; // Distance between the ship and the laser
    public float timeBetweenFires; // Time to fire again
    public AudioClip shootSound; // What sound to play when we're shooting
    public int lives; // Player's lives
    public int damage; // Player's damage
    public Transform explosion;

    // Player sprite
    Sprite playerSprite;

    // Damage game objects and sprites
    GameObject damage1;
    GameObject damage2;
    GameObject damage3;

    Sprite damageSprite1;
    Sprite damageSprite2;
    Sprite damageSprite3;

    // Lives image
    public Image livesImage;

    float currentSpeed; // Current player speed
    Vector3 lastMovement = new Vector3(); // Last position
    float timeTillNextFire; // Time till the next fire
    AudioSource audioSource; // Reference to our AudioSource component
    Vector3 playerPosition; // Player position
    bool invincible; // Invincibility after respawn
    Vector3 worldPos;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerSpeed = 5.0f;
        laserDistance = .7f;
        timeBetweenFires = .3f;
        lives = 5;
        damage = 0;
        currentSpeed = 0.0f;
        timeTillNextFire = 0.0f;
        invincible = false;

        instance = this;

        damage1 = this.transform.Find("playerShip_damage1").gameObject;
        damage2 = this.transform.Find("playerShip_damage2").gameObject;
        damage3 = this.transform.Find("playerShip_damage3").gameObject;
    }

    void Start()
    {
        LoadSprites();
    }

    // Update is called once per frame
    void Update()
    {

        if (!PauseMenuBehaviour.isPaused)
        {

            Rotation();
            Movement();

#if UNITY_STANDALONE_WIN || UNITY_WEBGL
            if (CrossPlatformInputManager.GetButton("Fire1") && timeTillNextFire < 0)
                {
                    timeTillNextFire = timeBetweenFires;
                    ShootLaser();
                }
#endif

#if UNITY_ANDROID
                if (timeTillNextFire < 0)
                {
                    timeTillNextFire = timeBetweenFires;
                    ShootLaser();
                }
#endif

            timeTillNextFire -= Time.deltaTime;

        }

    }

    // Will rotate the ship to face the mouse (only pc version)
    void Rotation()
    {

#if UNITY_ANDROID
        worldPos.x = CrossPlatformInputManager.GetAxis("Horizontal2");
        worldPos.y = CrossPlatformInputManager.GetAxis("Vertical2");
        float angle = Mathf.Atan2(worldPos.x, worldPos.y) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -angle));
        //this.transform.rotation = Quaternion.LookRotation(worldPos, Vector3.back);
#endif

#if UNITY_STANDALONE_WIN || UNITY_WEBGL
        // We need to tell where the mouse is relative to the player
        worldPos = Input.mousePosition;
        worldPos = Camera.main.ScreenToWorldPoint(worldPos);
        // Get the differences from each axis (stands for deltaX and deltaY)
        float dx = this.transform.position.x - worldPos.x;
        float dy = this.transform.position.y - worldPos.y;
        // Get the angle between the two objects
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        /*
        * The transform's rotation property uses a Quaternion,
        * so we need to convert the angle in a Vector
        * (The Z axis is for rotation for 2D).
        */
        Quaternion rot = Quaternion.Euler(new Vector3(0, 0, angle + 90));
        // Assign the ship's rotation
        this.transform.rotation = rot;
#endif
    }

    // Will move the player based off of keys pressed
    void Movement()
    {
        // The movement that needs to occur this frame
        Vector3 movement = new Vector3();
        // Check for input
        movement.x += CrossPlatformInputManager.GetAxis("Horizontal");
        movement.y += CrossPlatformInputManager.GetAxis("Vertical");
        //If we pressed multiple buttons, make sure we're only moving the same length
        movement.Normalize();

        // Check if we pressed anything
        if (movement.magnitude > 0)
        {
            // If we did, move in that direction
            currentSpeed = playerSpeed;
            this.transform.Translate(movement * Time.deltaTime * playerSpeed, Space.World);

            lastMovement = movement;
        }
        else
        {
            // Otherwise, move in the direction we were going
            this.transform.Translate(lastMovement * Time.deltaTime * currentSpeed, Space.World);
            // Slow down over time
            currentSpeed *= .9f;
        }

        // Keeps player at the camera view
        playerPosition = Camera.main.WorldToViewportPoint(this.transform.position);
        playerPosition.x = Mathf.Clamp(playerPosition.x, 0.02f, 0.98f);
        playerPosition.y = Mathf.Clamp(playerPosition.y, 0.05f, 0.95f);
        this.transform.position = Camera.main.ViewportToWorldPoint(playerPosition);
    }

    // Creates a laser and gives it an initial position in
    // front of the ship.
    void ShootLaser()
    {
        // Play sound
        audioSource.PlayOneShot(shootSound);
        // We want to position the laser in relation to
        // our player's location
        Vector3 laserPos = this.transform.position;
        // The angle the laser will move away from the center
        float rotationAngle = transform.localEulerAngles.z - 90;
        // Calculate the position right in front of the ship's
        // position laserDistance units away
        laserPos.x += (Mathf.Cos((rotationAngle) * Mathf.Deg2Rad) * -laserDistance);
        laserPos.y += (Mathf.Sin((rotationAngle) * Mathf.Deg2Rad) * -laserDistance);
        Instantiate(laser, laserPos, this.transform.rotation);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject colliderObject = collision.gameObject;

        // Damage by enemy collision
        if (colliderObject.tag == "Enemy" && !invincible)
        {
            AddPlayerDamage(1);
        }

        // Damage by laser collision
        if (colliderObject.tag == "Enemy_Laser" && !invincible)
        {
            int damageFromLaser = colliderObject.GetComponent<LaserBehaviour>().damage;
            AddPlayerDamage(damageFromLaser);
            Destroy(colliderObject);
        }

        if (damage > 3)
        {
            KillPlayer();
        }
    }

    // Add damage to the player
    void AddPlayerDamage(int damageApplied)
    {
        // Increase the player's damage
        damage += damageApplied;
        // Activate the damage's sprite that it's needed
        if (damage == 1)
        {
            damage1.SetActive(true);
        }
        else if (damage == 2)
        {
            damage1.SetActive(false);
            damage2.SetActive(true);
        }
        else if (damage >= 3)
        {
            damage2.SetActive(false);
            damage3.SetActive(true);
        }
    }

    // Kill the player
    void KillPlayer()
    {
        // Decrease 1 live
        lives--;
        // Set the damage to its original value and deactivate the damage gameObject
        damage = 0;
        damage3.SetActive(false);
        // Set the original player position
        this.transform.position = new Vector3(0, 0, 0);
        // Update the lives on the UI
        GameController.instance.UpdateUILives(lives);
        // Control the player's explosion
        if (explosion)
        {
            GameObject exploder = ((Transform)Instantiate(explosion, this.transform.position, this.transform.rotation)).gameObject;
            Destroy(exploder, 2.0f);
        }
        // If lives reach 0, then it's the game over
        if (lives <= 0)
        {
            Destroy(this.gameObject);
            PauseMenuBehaviour.instance.OpenGameOverMenu();
        }
        // Activate grace time
        StartCoroutine(GraceTime());
    }

    // Grace time after killing
    IEnumerator GraceTime()
    {
        SpriteRenderer spriteRendPlayer = this.gameObject.GetComponent<SpriteRenderer>();
        Color playerColor = new Color(1f, 1f, 1f, 0.5f);

        // Activate invincibility and half transparency for the player sprite
        invincible = true;
        spriteRendPlayer.color = playerColor;

        // Wait 3 seconds
        yield return new WaitForSeconds(3);

        // Deactivate invincibility and zero transparency for the player sprite
        invincible = false;
        playerColor.a = 1f;
        spriteRendPlayer.color = playerColor;
    }

    public void LoadSprites()
    {
        // Get the spaceship name selected on main menu (or default)
        string spaceShipName = MainMenuBehaviour.GetSpaceshipSpriteName();

        // Load spaceship sprite
        playerSprite = Resources.Load<Sprite>("Sprites/Player/" + spaceShipName);
        this.gameObject.GetComponent<SpriteRenderer>().sprite = playerSprite;

        // Load damage sprites
        String[] spritesName = spaceShipName.Split('_');
        string damageSpriteName = "Sprites/Player/Damage/playerShip" + spritesName[0] + "_damage";
        damageSprite1 = Resources.Load<Sprite>(damageSpriteName + "1");
        damage1.gameObject.GetComponent<SpriteRenderer>().sprite = damageSprite1;
        damageSprite2 = Resources.Load<Sprite>(damageSpriteName + "2");
        damage2.gameObject.GetComponent<SpriteRenderer>().sprite = damageSprite2;
        damageSprite3 = Resources.Load<Sprite>(damageSpriteName + "3");
        damage3.gameObject.GetComponent<SpriteRenderer>().sprite = damageSprite3;

        // Load lives sprite
        string livesSpriteName = "Sprites/Player/Lives/Lives_" + spaceShipName;
        Sprite livesSprite = Resources.Load<Sprite>(livesSpriteName);
        livesImage.sprite = livesSprite;

    }

}
