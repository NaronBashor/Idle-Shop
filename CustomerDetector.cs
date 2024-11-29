using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerDetector : MonoBehaviour
{
    [SerializeField] private float distanceCheck = 5f;  // Distance to check for customers
    [SerializeField] private LayerMask customerLayerMask;  // LayerMask for customer detection

    Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        CustomerRaycastChecker();  // Check for customers in range every frame
    }

    private void CustomerRaycastChecker()
    {
        // Raycast downwards to detect customers
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distanceCheck, customerLayerMask);

        // Debugging ray drawing
        if (hit) {
            Debug.DrawRay(transform.position, Vector2.down * distanceCheck, Color.green);
            //anim.SetBool("isWorking", true);
            // Null check for the CustomerController component
            CustomerController customer = hit.collider.GetComponent<CustomerController>();
            if (customer != null) {
                // If the customer hasn't paid, trigger the action at the counter
                if (!customer.paid) {
                    customer.CustomerAtCounter();  // Trigger the purchase action
                    if (customer.buying) {
                        anim.SetBool("isWorking", true);
                    }
                } else if (customer.paid) {
                    anim.SetBool("isWorking", false);
                }
            } else {
                Debug.LogWarning("Hit object does not have a CustomerController component!");
            }
        } else {
            Debug.DrawRay(transform.position, Vector2.down * distanceCheck, Color.red);
        }
    }
}
