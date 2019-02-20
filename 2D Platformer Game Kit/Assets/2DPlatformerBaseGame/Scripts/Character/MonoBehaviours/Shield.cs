using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Shield : MonoBehaviour
{
    // Private
    private Vector3 mousePos, screenPos;
    private Camera mainCamera;
    private bool canBoost = true;
    private Transform shieldGO;

    // Hidden Public
    [HideInInspector]
    public Transform shieldPivot;
    [HideInInspector]
    public Quaternion shieldDirection;
    [HideInInspector]
    public float shieldX, shieldY;
    [HideInInspector]
    public Vector2 boost = new Vector2();
    [HideInInspector]
    public bool shieldThrown = false;
    [HideInInspector]
    public bool shieldReturn = false;
    [HideInInspector]
    public bool canReturn = true;

    // Public
    public bool shieldOn = false;
    public float boostAmountX, boostAmountY;
    [Range(0f, 1f)] public float xMin;
    [Range(0f, 1f)] public float xMax;
    [Range(0f, 1f)] public float yMin;
    [Range(0f, 1f)] public float yMax;
    [Range(0f, 1f)] public float shieldAirborneDecelProportion;
    public bool shieldThrowOn = false;
    public GameObject thrownShieldPrefab;
    public float returnCD = 0.5f;
    
    private void Awake()
    {
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        shieldPivot = transform.Find("ShieldPivot");
        shieldGO = shieldPivot.Find("Shield");
    }

    private void Start()
    {
        if (shieldOn)
            EnableShield();
        else
            DisableShield();
    }

    private void Update()
    {
        if (shieldPivot != null)
        {
            ShieldDirection();

            if (shieldThrowOn && Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (!shieldThrown)
                    ShieldThrow();
                else if (canReturn)
                    ShieldReturn();
            }
        }
    }

    private void ShieldDirection()
    {
        mousePos = Input.mousePosition;
        screenPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mousePos.z - mainCamera.transform.position.z));
        shieldX = screenPos.x - PlayerCharacter.Instance.transform.position.x;
        shieldY = screenPos.y - PlayerCharacter.Instance.transform.position.y;
        boost = new Vector2(shieldX, shieldY).normalized;
        boost.x = Mathf.Sign(boost.x) * Mathf.Clamp(Mathf.Abs(boost.x), xMin, xMax);
        boost.y = Mathf.Sign(boost.y) * Mathf.Clamp(Mathf.Abs(boost.y), yMin, yMax);
        shieldDirection = Quaternion.Euler(0, 0, (Mathf.Atan2(shieldY, shieldX) * Mathf.Rad2Deg));
        shieldPivot.transform.rotation = shieldDirection;
    }

    public bool CheckForBoostInput()
    {
        if (canBoost)
            return PlayerInput.Instance.Boost.Up;
        else
            return false;
    }

    public void DisableShield()
    {
        canBoost = false;
        shieldPivot.gameObject.SetActive(false);
    }

    public void EnableShield()
    {
        canBoost = true;
        shieldPivot.gameObject.SetActive(true);
    }

    private void ShieldThrow()
    {
        Debug.Log("Throw shield");
        shieldThrown = true;
        canReturn = false;
        Instantiate(thrownShieldPrefab, shieldGO.transform.position, shieldGO.transform.rotation);
        shieldPivot.gameObject.SetActive(false);
        StartCoroutine(ReturnTimer());
    }

    private void ShieldReturn()
    {
        Debug.Log("Shield return");
        shieldReturn = true;
    }

    IEnumerator ReturnTimer()
    {
        yield return new WaitForSeconds(returnCD);
        canReturn = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (shieldThrown && shieldReturn && collision.gameObject.tag == "ThrownShield")
        {
            Debug.Log("Shield collected");

            shieldReturn = false;
            shieldThrown = false;
            shieldPivot.gameObject.SetActive(true);

            Destroy(collision.gameObject);
        }
    }
}
