using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Weapon_scr : MonoBehaviour
{
    [Header("WeaponComponents")]
    [SerializeField] private Animator _weaponAnimator;
    [SerializeField] private Camera fpsCamera;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject muzzleLight;
    [SerializeField] private CharacterController _characterController;

    [Header("Weapon Aspects")]
    [SerializeField] private bool isAutomatic;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private float weaponFiringDistance;
    [SerializeField][Tooltip("Adjusts how long it takes to switch firing modes")] private float fireSelectTime;
    public int ammo = 10;
    public int maxAmmo = 10;
    [SerializeField] private float reloadTime = 1f;

    [Header("Weapon Sway")]
    [SerializeField] private float smoothingTime = 1.0f;
    [SerializeField] private float swayMultiplier = 1.0f;

    [Header("Weapon Sounds")]
    public AudioSource audioSource;
    public AudioClip fireModeSwitchSound;
    public AudioClip fireSound;
    public AudioClip reloadSound;

    //Hidden Stuff
    private bool isChangingFireMode;

    [Header("Debug Stuff")]
    //Private vectors for ADS Logic
    public float ADStime = 0.35f;
    public Vector3 hipFirePoint = Vector3.zero;
    public Vector3 adsFirePoint = Vector3.zero;
    public Quaternion adsRotation;
    public Quaternion hipfireRotation;
    public bool isAiming;

    private bool gunCanFire;
    private bool isReloading;
    private float autoPlayBackSpeed;
    private float lastFireTime = 0;

    void Start()
    {
        _weaponAnimator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();

        muzzleLight.SetActive(false);

        autoPlayBackSpeed = 1.5f;

        isReloading = false;

        gunCanFire = true;

        isAiming = false;

        hipFirePoint = transform.localPosition;

        hipfireRotation = transform.localRotation;
    }

    #region - Firing Modes -

    //---Handles firing in SemiAuto
    void FireSemi()
    {
        if(ammo > 0)
        {
            ammo--;

            _weaponAnimator.SetBool("isFiring", true);

            _weaponAnimator.speed = autoPlayBackSpeed;

            audioSource.PlayOneShot(fireSound);

            HandleWeaponPhysics();
        }
    }

    //---Handles firing in automatic
    void FireAuto()
    {
        if(ammo > 0)
        {
            ammo--;

            HandleWeaponPhysics();

            _weaponAnimator.SetBool("isFiringAuto", true);

            _weaponAnimator.speed = autoPlayBackSpeed;

            audioSource.PlayOneShot(fireSound);

            //last fire time gets reset to the time of the start of the frame.
            lastFireTime = Time.time;
        }
    }

    void HandleWeaponPhysics()
    {
        //Raycast stuff
        RaycastHit hitinfo;

        Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hitinfo, weaponFiringDistance);

        Debug.DrawRay(fpsCamera.transform.position, fpsCamera.transform.forward, Color.blue, 10f);

        if (hitinfo.collider != null)
        {
            Debug.Log("You Have Hit " + hitinfo.collider.name);
        }
    }

    #endregion

    #region - Reload -

    IEnumerator ReloadCoroutine()
    {
        gunCanFire = false;
        isReloading = true;
        Debug.Log("Reloading...");

        _weaponAnimator.SetBool("IsReload_anim", true);

        ammo = maxAmmo;
        audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        Debug.Log("Reload complete!");
        gunCanFire = true;
        _weaponAnimator.SetBool("IsReload_anim", false);
        _weaponAnimator.SetBool("isReloading", false);
        isReloading = false;
    }

    #endregion

    #region - Weapon State Check -

    //---Takes into account what the weapon is set to/doing, and calls methods based off of that info
    void WeaponStateCheck()
    {
        #region - Weapon Fire -
        
        if (Input.GetButtonDown("Fire1") && !isAutomatic && gunCanFire && ammo != 0)
        {
            FireSemi();
            MuzzleFlash();
            muzzleLight.SetActive(true);
        }

        else
        {
            _weaponAnimator.SetBool("isFiring", false);
            muzzleLight.SetActive(false);
        }

        //Here we grab the input, check if weapon is automatic, and check to see if the weapon has been fired.
        //
        //Checking the fire time helps make sure the method is called only after the weapon has fired. 
        if (Input.GetButton("Fire1") && isAutomatic && gunCanFire && ammo != 0)
        {
            if(Time.time - lastFireTime > fireRate)
            {
                FireAuto();
                MuzzleFlash();
                muzzleLight.SetActive(true);
            }

            else
            {
                muzzleLight.SetActive(false);
            }
        }

        else
        {
            _weaponAnimator.SetBool("isFiringAuto", false);
            _weaponAnimator.speed = 1;
        }

        //Handles Reload Check
        if(Input.GetKeyDown(KeyCode.R) && ammo != maxAmmo)
        {
            audioSource.PlayOneShot(reloadSound);
            StartCoroutine(ReloadCoroutine());
        }

        if(Input.GetButtonDown("Fire1") && ammo == 0)
        {
            audioSource.PlayOneShot(fireModeSwitchSound);
        }

        #endregion

        #region - Weapon Move

        //Checks to see if the walking/sprinting animations should be played
        if(_characterController.velocity.magnitude > 0.1f)
        {
            _weaponAnimator.SetBool("isWalking", true);
        }

        else
        {
            _weaponAnimator.SetBool("isWalking", false);
        }

        if(Input.GetKey(KeyCode.LeftShift) && _characterController.velocity.magnitude > 0f)
        {
            _weaponAnimator.SetBool("isSprint", true);
        }

        else
        {
            _weaponAnimator.SetBool("isSprint", false);
        }

        //Here will check to make sure the weapon doesnt fire while running animation is playing
        if(_weaponAnimator.GetBool("isSprint") == true)
        {
            gunCanFire = false;
        }

        else if (_weaponAnimator.GetBool("isSprint") == false)
        {
            gunCanFire = true;
        }

        #endregion

        #region - ADS Check -

        if(Input.GetButtonDown("Fire2") && !isReloading && !isChangingFireMode && !isAiming)
        {
            isAiming = true;

            _weaponAnimator.enabled = false;
        }

        else if (Input.GetButtonUp("Fire2") && !isReloading && !isChangingFireMode && isAiming)
        {
            isAiming = false;
            _weaponAnimator.enabled = true;
        }

        #endregion
    }

    #endregion

    void ADS_Logic()
    {

        if (isAiming)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, adsFirePoint, ADStime);

            transform.localRotation = adsRotation;
        }

        else if (!isAiming)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, hipFirePoint, ADStime);
            transform.localRotation = hipfireRotation;
        }
    }

    #region - Fire Mode Select -

    void FireModeSelect()
    {
        if(Input.GetKeyDown(KeyCode.Q) && !isAutomatic && !isChangingFireMode)
        {
            audioSource.PlayOneShot(fireModeSwitchSound);
            isAutomatic = true;
            StartCoroutine(FireSelectCorutine());
        }

        else if(Input.GetKeyDown(KeyCode.Q)&& isAutomatic && !isChangingFireMode)
        {
            audioSource.PlayOneShot(fireModeSwitchSound);
            isAutomatic = false;
            StartCoroutine(FireSelectCorutine());
        }
    }

    IEnumerator FireSelectCorutine()
    {
        Debug.Log("Switching Fire modes...");

        gunCanFire = false;

        isChangingFireMode = true;

        _weaponAnimator.SetBool("changeFireType", true);

        yield return new WaitForSeconds(fireSelectTime);

        gunCanFire = true;

        _weaponAnimator.SetBool("changeFireType", false);

        Debug.Log("Fire Mode has been set");

        isChangingFireMode = false;
    }

    #endregion

    #region - Weapon Sway -

    //void HandleWeaponSway()
    //{
    //    float mouseX = Input.GetAxisRaw("Mouse X") * swayMultiplier - 90f;
    //    float mouseY = Input.GetAxisRaw("Mouse Y") * swayMultiplier;

    //    Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
    //    Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

    //    Quaternion targetRotation = rotationX * rotationY;

    //    transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smoothingTime * Time.deltaTime);
    //}

    #endregion

    #region - Muzzle Flash -

    void MuzzleFlash()
    {
        if(_weaponAnimator.GetBool("isFiring") == true)
        {
            muzzleFlash.Play();
        }

        else if(_weaponAnimator.GetBool("isFiringAuto") == true)
        {
            muzzleFlash.Play();
        }

        else
        {
            muzzleFlash.Stop();
        }
    }

    #endregion

    void Update()
    {
        FireModeSelect();
        WeaponStateCheck();
        //HandleWeaponSway();
        ADS_Logic();
    }
}
