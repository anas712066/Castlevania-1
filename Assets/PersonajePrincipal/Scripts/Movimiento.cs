using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movimiento : MonoBehaviour
{
    [SerializeField]
    float speed;
    [SerializeField]
    float jumpImpulse;
    [SerializeField]
    Transform raycastOrigin;
    [SerializeField]
    LayerMask FloorLayer;
    private enum STATES
    {
        ONFLOOR, ONAIR, ONATTACK, ONTHROW
    };
    private STATES currentStates;
    public InputActionAsset inputActionMapping;
    private InputAction horizontal_ia, jump_ia, attack_ia, throw_ai;
    Rigidbody2D rb2D;
    Animator ator;
    public float raycastDistance = 0.35f;
    private PlayerWeaponSwich weaponSwitch;

    void Start()
    {
        inputActionMapping.Enable();
        horizontal_ia = inputActionMapping.FindActionMap("Movimiento").FindAction("Horizontal");
        jump_ia = inputActionMapping.FindActionMap("Movimiento").FindAction("Jump");
        attack_ia = inputActionMapping.FindActionMap("Atacar").FindAction("Cuerpo");
        throw_ai = inputActionMapping.FindActionMap("Atacar").FindAction("Lanzar");
        currentStates = STATES.ONFLOOR;
        rb2D = GetComponent<Rigidbody2D>();
        ator = GetComponent<Animator>();
        weaponSwitch = GetComponent<PlayerWeaponSwich>();

        // Verificaci�n de que weaponSwitch no es null
        if (weaponSwitch == null)
        {
            Debug.LogError("No se encontr� el componente PlayerWeaponSwich en el mismo GameObject. Asigna el script PlayerWeaponSwich al GameObject.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStates)
        {
            case STATES.ONFLOOR:
                ONFLOOR();
                break;
            case STATES.ONAIR:
                ONAIR();
                break;
            case STATES.ONATTACK:
                ONATTACK();
                break;
            case STATES.ONTHROW:
                ONTHROW();
                break;
        }
    }

    private void ONFLOOR()
    {
        if (ToOnAir())
            return;
        if (ToOnAttack())
            return;
        if (ToOnThrow())
            return;

        float mx = horizontal_ia.ReadValue<float>();
        rb2D.velocity = new Vector2(mx * speed, rb2D.velocity.y);
        ator.SetFloat("caminar", Math.Abs(mx * speed));
        TransformP(mx);
        ator.SetBool("jump", false);
        ator.SetBool("latigo_A", false);
        ator.SetBool("Lanzar", false);
        if (jump_ia.triggered)
        {
            rb2D.AddForce(new Vector2(0, 1) * jumpImpulse, ForceMode2D.Impulse);
        }
    }

    private void ONAIR()
    {
        if (ToOnFloor())
            return;
        if (ToOnAttack())
            return;
        if (ToOnThrow())
            return;

        float mx = horizontal_ia.ReadValue<float>();
        rb2D.velocity = new Vector2(mx * speed / 2, rb2D.velocity.y);
        ator.SetBool("jump", true);
        ator.SetBool("latigo_A", false);
        ator.SetBool("Lanzar", false);
    }
    private void ONATTACK()
    {
        if (ToOnFloor())
            return;
        if (ToOnAir())
            return;

   
        ator.SetBool("latigo_A", true);
        ator.SetBool("Lanzar", false);
    }
    private void ONTHROW()
    {
        if (ToOnFloor())
            return;
        if (ToOnAir())
            return;
        ator.SetBool("latigo_A", false);
        ator.SetBool("Lanzar", true);
    }

    bool ToOnAir()
    {
        RaycastHit2D hit1 = Physics2D.Raycast(raycastOrigin.position, -raycastOrigin.up, raycastDistance, FloorLayer);
        Debug.DrawRay(raycastOrigin.position, -raycastOrigin.up * raycastDistance, Color.red);
        if (!hit1 && !attack_ia.triggered && !throw_ai.triggered)
        {
            currentStates = STATES.ONAIR;
            ONAIR();
            return true;
        }
        return false;
    }

    bool ToOnFloor()
    {
        RaycastHit2D hit1 = Physics2D.Raycast(raycastOrigin.position, -raycastOrigin.up, raycastDistance, FloorLayer);
        Debug.DrawRay(raycastOrigin.position, -raycastOrigin.up * raycastDistance, Color.red);
        if (hit1 && !attack_ia.triggered && !throw_ai.triggered )
        {
            currentStates = STATES.ONFLOOR;
            ONFLOOR();
            return true;
        }
        return false;
    }
    bool ToOnAttack()
    {
        if(attack_ia.triggered && !throw_ai.triggered)
        {
            if (weaponSwitch.armaActual == PlayerWeaponSwich.TipoArma.Latigo)
            {
                currentStates = STATES.ONATTACK;
                ONATTACK();
                return true;
            }
            return false;
        }
        return false;
    }
    bool ToOnThrow()
    {
        if (throw_ai.triggered && !attack_ia.triggered)
        {
            if (weaponSwitch.armaActual == PlayerWeaponSwich.TipoArma.Cuchillo || weaponSwitch.armaActual == PlayerWeaponSwich.TipoArma.Hacha)
            {
                Debug.Log("Hola");
                currentStates = STATES.ONTHROW;
                ONTHROW();
                return true;
            }
            return false;
        }
        return false;
    }

    private void TransformP(float mx)
    {
        if (mx > 0)
        {
            gameObject.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
        }
        else if (mx < 0)
        {
            gameObject.transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
        }

    }
}
