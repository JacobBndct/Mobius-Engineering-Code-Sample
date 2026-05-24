using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

// For the sake of this sample, this context is included to show how abilities are managed by the player.
public abstract class Entity : MonoBehaviour
{
    [SerializeField] protected EntityState _startState;

    // This state machine is used by all entities and is responsible for managing an entity's state and transitions.
    // In the context of the player, states enable/disable abilities to define the functionality of a state.
    protected EntityStateMachine _stateMachine;

    // This preprocessor directive is technically not needed, but is included to be more explicate about the fact that the code is not included in a production build.
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position, _stateMachine?.CurrentState?.GetStateName());
    }
#endif
}

public class PlayerCharacter : Entity
{
    public PlayerController Controller = new();
    [HideInInspector] public Rigidbody PlayerRigidBody;

    public void Awake()
    {
        InitializePlayerStateMachine();
        PlayerRigidBody = GetComponent<Rigidbody>();
    }

    private void InitializePlayerStateMachine()
    {
        _stateMachine = new EntityStateMachine(_startState, this);
    }

    public void Update()
    {
        _stateMachine?.Update();
    }

    public void FixedUpdate()
    {
        _stateMachine?.FixedUpdate();
    }

    public void LateUpdate()
    {
        _stateMachine?.LateUpdate();
    }
}

public class PlayerController
{
    // controller holds a dictionary of player abilities which is populated at runtime
    // in this scheme each ability is responsible for handling player inputs themselves by connecting to the new player input system
    private readonly Dictionary<Type, PlayerAbility> _playerAbilities = new();

    public void RegisterAbility(PlayerAbility ability)
    {
        _playerAbilities.Add(ability.GetType(), ability);
    }

    public void ClearRegisteredAbilities()
    {
        _playerAbilities.Clear();
    }

    // Here two functions are provided to find the player ability. 
    // This allows us to find an ability either using a generic type or a type as a parameter.
    // The generic type version is used when we know the type at compile time
    // The non-generic version is used to find an ability when we don't know the type at compile time.
    public T FindPlayerAbility<T>() where T : PlayerAbility
    {
        _playerAbilities.TryGetValue(typeof(T), out var ability);
        return (T)ability;
    }

    public PlayerAbility FindPlayerAbility(Type type)
    {
        _playerAbilities.TryGetValue(type, out var ability);
        return ability;
    }
}

[RequireComponent(typeof(PlayerCharacter))]
public abstract class PlayerAbility : MonoBehaviour
{
    protected PlayerCharacter _player;

    private bool _isActionEnabled = true;

    protected virtual void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _player.Controller.RegisterAbility(this);
    }

    public virtual void PreformAction(InputAction.CallbackContext context)
    {
        if (_isActionEnabled)
        {
            Action(context);
        }
    }

    // an abstract function for an ability's specific action
    protected abstract void Action(InputAction.CallbackContext context);

    // Enabled and disable functions are kept seperate to make calls more explicate (Mostly a personal preference)
    public virtual void EnableAbility()
    {
        _isActionEnabled = true;
    }

    public virtual void DisableAbility()
    {
        _isActionEnabled = false;
    }
}

public class PlayerLook : PlayerAbility
{
    [Header("Camera Movement Parameters")]
    [SerializeField] float horizontalSensitivity;
    [SerializeField] float verticalSensitivity;
    [SerializeField] bool invertHorizontal;
    [SerializeField] bool invertVertical;
    [SerializeField] float cameraHorizontalRangeOfMotion;

    [Header("Camera Obeject Parameters")]
    [SerializeField] Transform rotatableObjects;

    protected override void Awake()
    {
        base.Awake();
          
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // action to be called when the mouse moves
    protected override void Action(InputAction.CallbackContext context)
    {
        Vector2 delta = context.ReadValue<Vector2>();

        HorizontalRotation(delta.x);
        VerticalRotation(delta.y);
    }

    void HorizontalRotation(float deltaX)
    {
        float horizontalRotation = deltaX * horizontalSensitivity * Invert(invertHorizontal);

        Quaternion horizontalRotationQuaternion = Quaternion.AngleAxis(horizontalRotation, transform.up) * _player.PlayerRigidBody.rotation;
        _player.PlayerRigidBody.MoveRotation(horizontalRotationQuaternion);
    }

    void VerticalRotation(float deltaY)
    {
        float verticalRotation = -(deltaY * verticalSensitivity) * Invert(invertVertical);
        float currentVerticalRotation = RelativeToForward(rotatableObjects.localEulerAngles.x);

        float min = -cameraHorizontalRangeOfMotion - currentVerticalRotation;
        float max = cameraHorizontalRangeOfMotion - currentVerticalRotation;

        verticalRotation = Mathf.Clamp(verticalRotation, min, max);

        Vector3 verticalRotationVector = new Vector3(verticalRotation, 0f, 0f);
        rotatableObjects.Rotate(verticalRotationVector);
    }

    // Changes the range of a euler angle from 0 - 360 degrees to -180 - 180. This works so that 0 is in the direction of the local forward of the transform.
    float RelativeToForward(float eulerRotation)
    {
        return (eulerRotation >= 180) ? eulerRotation - 360 : eulerRotation;
    }

    // Inverts equation on true and leaves the same on false
    float Invert(bool isInverted)
    {
        return isInverted ? -1 : 1;
    }
}