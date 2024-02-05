using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player_Controller : MonoBehaviourPunCallbacks
{
    [Header("Movement")]
    [SerializeField] CharacterController controller;
    [SerializeField] Animator anim;
    [SerializeField] GameObject playerModel;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] LayerMask groundLayers;
    [SerializeField] float WalkSpeed = 10f, RunSpeed = 18f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float gravityMod = 2.5f;
    float currentSpeed;
    float y_Velovity;
    bool isGrounded = false;
    Vector3 movement, movDir;

    [Space]
    [SerializeField] int maxHealth = 100;
    int currentHealth;

    [Header("Gun Systeam")]
    [SerializeField] Gun[] allGuns;
    [SerializeField] GameObject bulletImpact;
    [SerializeField] float maxHeat = 10, coolRate = 4, overHeatCoolRate = 5;
    [SerializeField] float muzzelDesplayTime = 0.5f;
    [SerializeField] GameObject playerHitImpact;
    [SerializeField] Transform ModelGunPoint, mainGunHolder;
    [SerializeField] float adsSpeed = 5;
    float muzzelTimeCounter;
    float shotCounter;
    bool overHeated;
    float heatCounter;
    int selectedGun;


    [Header("Camera")]
    [SerializeField] Transform viewPoint;
    [SerializeField] float sencitivity = 1f;
    Camera cam;
    float verticalRotStore;
    Vector2 mouseInput;

    [Header("PlayerSkin")]
    [SerializeField] Material[] allPlayerSkins;


    void Start()
    {
        cam = Camera.main;

        // SwitchGun();
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);

        if (photonView.IsMine)
        {
            playerModel.SetActive(false);
            currentHealth = maxHealth;
            UI_Controller.instance.tempSlider.maxValue = maxHeat;
            UI_Controller.instance.Health_text.text = maxHealth.ToString("00");
        }
        else
        {
            mainGunHolder.parent = ModelGunPoint;
            mainGunHolder.localPosition = Vector3.zero;
            mainGunHolder.localRotation = Quaternion.identity;
        }

        playerModel.GetComponent<SkinnedMeshRenderer>().material = allPlayerSkins[photonView.Owner.ActorNumber % 7];
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        #region Camera
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y") * sencitivity);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);
        viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        #endregion

        #region Player Movement
        movDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = RunSpeed;
        }
        else
        {
            currentSpeed = WalkSpeed;
        }

        y_Velovity = movement.y;
        movement = ((transform.forward * movDir.z) + (transform.right * movDir.x)).normalized * currentSpeed;
        movement.y = y_Velovity;
        if (controller.isGrounded)
        {
            movement.y = 0f;
        }

        isGrounded = Physics.CheckSphere(groundCheckPoint.position, 0.25f, groundLayers);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            movement.y = jumpForce;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        controller.Move(movement * Time.deltaTime);
        #endregion

        #region Shoot And Fire
        if (allGuns[selectedGun].muzzelFlash.activeInHierarchy)
        {
            muzzelTimeCounter -= Time.deltaTime;
            if (muzzelTimeCounter <= 0)
            {
                allGuns[selectedGun].muzzelFlash.SetActive(false);
                muzzelTimeCounter = 0;
            }
        }
        if (!overHeated)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
            else if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter <= 0f)
                {
                    Shoot();
                }
            }
            heatCounter -= coolRate * Time.deltaTime;
        }
        else
        {
            heatCounter -= overHeatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {
                heatCounter = 0;
                overHeated = false;
            }
        }
        if (heatCounter < 0)
        {
            heatCounter = 0f;
        }
        UI_Controller.instance.tempSlider.value = heatCounter;
        #endregion

        #region Wepon Change
        if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun++;
            if (selectedGun >= allGuns.Length)
            {
                selectedGun = allGuns.Length - 1;
            }
            // SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun--;
            if (selectedGun < 0)
            {
                selectedGun = 0;
            }
            // SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }
        for (int i = 0; i < allGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedGun = i;
                // SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }
        }
        #endregion

        #region Animations
        anim.SetBool("grounded", isGrounded);
        anim.SetFloat("speed", movDir.magnitude);
        #endregion

        #region WeponAds
        if (Input.GetMouseButton(1))
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, allGuns[selectedGun].adsZoom, adsSpeed * Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, adsSpeed * Time.deltaTime);
        }
        #endregion

        #region CursorLock
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None && !UI_Controller.instance.OptionsScreen.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        #endregion
    }

    void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //this vector is center point of screen
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.tag == "Player")
            {
                GameObject brustImpact = PhotonNetwork.Instantiate(playerHitImpact.name, hit.point + (hit.normal * 0.002f), Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, allGuns[selectedGun].ShotDamage, PhotonNetwork.LocalPlayer.ActorNumber);// call this fun by hited player not call by me
            }
            else
            {
                GameObject brustImpact = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(brustImpact, 5f);
            }
        }
        shotCounter = allGuns[selectedGun].timeBetweenShots;
        heatCounter += allGuns[selectedGun].heatParShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;
        }
        allGuns[selectedGun].muzzelFlash.SetActive(true);
        muzzelTimeCounter = muzzelDesplayTime;
    }

    void SwitchGun()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        heatCounter = 0;
        overHeated = false;
        allGuns[selectedGun].gameObject.SetActive(true);
        allGuns[selectedGun].muzzelFlash.SetActive(false);
    }

    [PunRPC]
    public void DealDamage(string damager, int damageAmount, int actor)
    {
        TackDamage(damager, damageAmount, actor);
    }

    public void TackDamage(string damager, int damageAmount, int actor)
    {
        if (photonView.IsMine)
        {
            currentHealth -= damageAmount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Player_Spawner.instance.Die(damager);
                Match_Manager.instance.UpdateStateSend(actor, 0, 1); //Give Kill
            }
            UI_Controller.instance.Health_text.text = currentHealth.ToString("00");
        }
    }

    [PunRPC]
    public void SetGun(int gunToSwitch)
    {
        if (gunToSwitch < allGuns.Length)
        {
            selectedGun = gunToSwitch;
            SwitchGun();
        }
    }

    void LateUpdate()
    {
        if (photonView.IsMine)
        {
            if (Match_Manager.instance.state == Match_Manager.GameState.Playing)
            {
                cam.transform.position = viewPoint.position;
                cam.transform.rotation = viewPoint.rotation;
            }
            else
            {
                cam.transform.position = Match_Manager.instance.mapCameraPoint.position;
                cam.transform.rotation = Match_Manager.instance.mapCameraPoint.rotation;
            }
        }
    }
}