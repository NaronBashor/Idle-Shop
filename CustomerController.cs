using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class CustomerController : MonoBehaviour
{
    Animator anim;
    SpriteLibrary spriteLibrary;
    BoxCollider2D coll;

    [SerializeField] private GameObject coinDisplayPrefab;
    [SerializeField] private Transform coinDisplayPrefabPosition;
    [SerializeField] private Transform coinDisplayPrefabParent;

    [SerializeField] private SpriteLibraryAsset[] SpriteRefs;

    [SerializeField] private GameObject progressionBar;

    [SerializeField] private LayerMask customerMask;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveSpeedInitialValue;

    [SerializeField] private float timePerCustomerToOrder;
    [SerializeField] private float yOffset;

    [SerializeField] private float distanceBetweenCustomers;

    [SerializeField] private int initialPricePerDrink = 8;
    [SerializeField] private int currentPricePerDrink;

    private float num;
    private float den;

    private bool canMove = false;
    public bool buying = false;
    public bool paid = false;
    private bool atCounter = false;  // NEW: flag for customer being at counter
    private bool runOnce = false;
    private bool managerPurchased = false;

    public float TimePerCustomerToOrder { get { return timePerCustomerToOrder; } set { timePerCustomerToOrder = value; } }
    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }
    public int CurrentPricePerDrink { get { return currentPricePerDrink; } set { currentPricePerDrink = value; } }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteLibrary = GetComponent<SpriteLibrary>();
        coll = GetComponent<BoxCollider2D>();

        coinDisplayPrefabParent = GameObject.Find("CoinCountParentObject").GetComponent<Transform>();

        progressionBar.SetActive(false);

        // Initially allow the customer to move
        canMove = true;

        spriteLibrary.spriteLibraryAsset = SpriteRefs[Random.Range(0, SpriteRefs.Length)];

        moveSpeed = moveSpeedInitialValue;
        currentPricePerDrink = initialPricePerDrink;

        // Get the latest work speed from the Shop or GameplayManager
        TimePerCustomerToOrder = GameObject.Find("GameManager").GetComponent<GameplayManager>().GetCurrentWorkSpeed();
        MoveSpeed = GameObject.Find("GameManager").GetComponent<GameplayManager>().GetCurrentMoveSpeed();
        CurrentPricePerDrink = GameObject.Find("GameManager").GetComponent<GameplayManager>().GetCurrentPricePerDrink();
    }

    private void Update()
    {
        // Check surroundings to decide if movement is allowed
        CheckCustomerSurroundings();

        // Move only if allowed (i.e., canMove is true) and customer hasn't reached the counter
        if (canMove && !atCounter) {
            this.transform.Translate(Vector3.left * moveSpeed);
        }

        // If the customer is buying, update the progression bar
        if (buying) {
            ProgressionBarTimer();
        }
    }

    private void ProgressionBarTimer()
    {
        if (GameObject.Find("Progression Bar Slider") != null) {
            if (!runOnce) {
                runOnce = true;
                num = timePerCustomerToOrder * 1;
                den = timePerCustomerToOrder * 1;
            }
            GameObject.Find("Progression Bar Slider").GetComponent<Slider>().value = ((num -= Time.deltaTime) / (den));
        }
    }

    // Method to trigger customer action at the counter (triggered when reaching the counter)
    public void CustomerAtCounter()
    {
        atCounter = true;  // Customer is now at the counter
        canMove = false;  // Stop customer movement
        anim.SetBool("isIdle", true);  // Play idle animation
        anim.SetBool("isWalking", false);  // Play idle animation

        if (managerPurchased) {
            StartCoroutine(PurchaseTimer());
        }
    }

    // Detect mouse click to start the order process
    public void StartPurchase()
    {
        if (atCounter && !buying && !managerPurchased)  // Only allow click if customer is at the counter and not already buying
        {
            StartCoroutine(PurchaseTimer());  // Start the purchase process on click
        }
    }

    // Coroutine to handle the purchase process
    IEnumerator PurchaseTimer()
    {
        buying = true;
        progressionBar.SetActive(true);  // Show progression bar

        yield return new WaitForSeconds(timePerCustomerToOrder);  // Wait for the purchase time

        // When done with purchase
        buying = false;
        progressionBar.SetActive(false);  // Hide progression bar
        if (!paid) {
            paid = true;
            CoinManager.AddGold(currentPricePerDrink);  // Customer paid
            GameObject coins = Instantiate(coinDisplayPrefab, coinDisplayPrefabPosition.position, Quaternion.identity, coinDisplayPrefabParent);
            coins.GetComponent<TextMeshProUGUI>().text = "+" + currentPricePerDrink.ToString();
            coins.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }

        // Resume movement after the purchase is complete
        MovingAfterBuy();

        // Inform the GameplayManager that the customer has completed the purchase
        GameObject.Find("GameManager").GetComponent<GameplayManager>().CustomerLeftLine(this.gameObject);
    }

    // Method to move the customer after buying
    private void MovingAfterBuy()
    {
        // Allow movement after the purchase
        canMove = true;
        buying = false;
        atCounter = false;  // Reset counter flag
        anim.SetBool("isIdle", false);
        anim.SetBool("isWalking", true);
        anim.SetBool("isDone", true);
    }

    // Method to pause customer movement
    private void IdleNotMoving()
    {
        // Stop movement
        canMove = false;
        anim.SetBool("isIdle", true);  // Set idle animation
        anim.SetBool("isWalking", false);
    }

    // Method to check customer surroundings and handle movement stops
    private void CheckCustomerSurroundings()
    {
        // Check if there is a customer ahead
        RaycastHit2D hit = Physics2D.Raycast(new Vector2((coll.bounds.min.x - 0.125f), (coll.bounds.max.y / 2)), Vector2.left, distanceBetweenCustomers, customerMask);

        // Update canMove based on surroundings and whether the customer is buying
        if (hit)  // If there's a customer too close ahead
        {
            IdleNotMoving();  // Stop moving because someone is ahead
            Debug.DrawRay(new Vector2((coll.bounds.min.x - 0.125f), (coll.bounds.max.y / 2)), Vector3.left * distanceBetweenCustomers, Color.green);
        } else if (!buying && !atCounter)  // No one is ahead, customer is not buying, and not at the counter
        {
            MovingBeforeBuy();  // Allow the customer to move
            Debug.DrawRay(new Vector2((coll.bounds.min.x - 0.125f), (coll.bounds.max.y / 2)), Vector3.left * distanceBetweenCustomers, Color.red);
        }
    }

    // Method to allow movement before buying
    private void MovingBeforeBuy()
    {
        // Only allow movement if the customer is not buying
        canMove = true;
        anim.SetBool("isIdle", false);
        anim.SetBool("isWalking", true);
    }

    // Handle trigger to despawn the player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Despawn Player")) {
            Destroy(this.gameObject);  // Despawn the customer
        }
    }
}
