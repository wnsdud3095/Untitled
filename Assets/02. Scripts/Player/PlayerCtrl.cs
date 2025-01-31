using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    #region State Variables
    private IState<PlayerCtrl> m_idle_state;
    private IState<PlayerCtrl> m_walk_state;
    private IState<PlayerCtrl> m_run_state;
    private IState<PlayerCtrl> m_jump_in_state;
    private IState<PlayerCtrl> m_jumping_state;
    private IState<PlayerCtrl> m_jump_out_state;
    private IState<PlayerCtrl> m_attack_state;
    private IState<PlayerCtrl> m_block_state;
    private IState<PlayerCtrl> m_damage_state;
    private IState<PlayerCtrl> m_block_damage_state;
    #endregion

    #region Properties
    public Transform Model { get; private set; }
    public Transform CameraArm { get; private set; }
    public Animator Animator { get; private set; }
    public DataManager Data { get; private set;}

    [Header("Move Component")]
    public Rigidbody Rigidbody { get; private set; }
    public Vector3 Direction { get; set; }    

    [Header("Jump Component")]
    public float FallTime { get; set; }
    public float JumpPower { get; set; } = 7f;
    public bool IsGround { get; set; }

    [Header("Attack Component")]
    public float AttackDelay { get; set; }
    public bool AttackReady { get; set; }
    public WeaponCtrl Weapon { get; set; }
    public float AttackSpeed { get; set; }

    [Header("Block Component")]
    public bool IsBlock { get; set; }
    public float BlockTime { get; set; }

    [Header("State Component")]
    public PlayerStateContext StateContext { get; set; }
    #endregion

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Model = GameObject.Find("PlayerModel").GetComponent<Transform>();
        CameraArm = GameObject.Find("CameraArm").GetComponent<Transform>();
        Animator = Model.GetComponent<Animator>();
        Weapon = FindAnyObjectByType<WeaponCtrl>();
        Data =  GameObject.Find("DataManager").GetComponent<DataManager>();
        
    
        StateContext = new PlayerStateContext(this);

        m_idle_state = gameObject.AddComponent<PlayerIdleState>();
        m_walk_state = gameObject.AddComponent<PlayerWalkState>();
        m_run_state = gameObject.AddComponent<PlayerRunState>();
        m_jump_in_state = gameObject.AddComponent<PlayerJumpInState>();
        m_jumping_state = gameObject.AddComponent<PlayerJumpingState>();
        m_jump_out_state = gameObject.AddComponent<PlayerJumpOutState>();
        m_attack_state = gameObject.AddComponent<PlayerAttackState>();
        m_block_state = gameObject.AddComponent<PlayerBlockState>();
        m_damage_state = gameObject.AddComponent<PlayerDamageState>();
        m_block_damage_state = gameObject.AddComponent<PlayerBlockDamageState>();

        ChangeState(PlayerState.IDLE);
    }

    private void Start()
    {
        // TODO: 위치 변경
        AttackSpeed = Data.PlayerStat.Rate + Weapon.Info.Rate;
        Animator.SetFloat("AttackSpeed", 1.5f / AttackSpeed);
    }

    private void Update()
    {
        Direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        CheckGround();
        CheckFalling();
        CheckBlocking();
        CheckAttackDelay();

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            GetDamage(10);
        }

        StateContext.ExecuteUpdate();
    }

    private void FixedUpdate()
    {
        
    }

    public void Move(float speed)
    {
        Vector3 forward_direction = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z);
        Vector3 right_direction = new Vector3(CameraArm.right.x, 0f, CameraArm.right.z);

        Vector3 final_direction = ((forward_direction * Direction.z) + (right_direction * Direction.x)).normalized;

        Vector3 velocity = final_direction * speed;

        Model.forward = Vector3.Lerp(forward_direction, Model.forward, Time.deltaTime);

        Vector3 new_position = Rigidbody.position + velocity * Time.deltaTime;
        Rigidbody.MovePosition(new_position);
    }

        public void Jump(float power)
        {
            if(IsGround && Input.GetButtonDown("Jump"))
            {
                Rigidbody.linearVelocity = new Vector3(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
                Rigidbody.AddForce(Vector3.up * power, ForceMode.Impulse);
                ChangeState(PlayerState.JUMPIN);
            }
        }

    public void Attack()
    {
        if(AttackReady && Input.GetKeyDown(KeyCode.Mouse0) && IsGround)
        {
            Debug.Log("클릭함");
            Weapon.Use();
            AttackDelay = 0;
            ChangeState(PlayerState.ATTACK);
        }
    }

    public void GetDamage(float damage)
    {
        if(IsBlock)
        {
            // 방어 이펙트가 있었으면 함.
            ChangeState(PlayerState.BLOCKDAMAGE);
            Data.PlayerStat.HP = damage * 0.2f;
        }
        else
        {
            ChangeState(PlayerState.DAMAGE);
            Data.PlayerStat.HP = damage;
        }
    }

    public void ChangeState(PlayerState state)
    {
        switch(state)
        {
            case PlayerState.IDLE:
                StateContext.Transition(m_idle_state);
                break;
            
            case PlayerState.WALK:
                StateContext.Transition(m_walk_state);
                break;
            
            case PlayerState.RUN:
                StateContext.Transition(m_run_state);
                break;

            case PlayerState.JUMPIN:
                StateContext.Transition(m_jump_in_state);
                break;

            case PlayerState.JUMPING:
                StateContext.Transition(m_jumping_state);
                break;
            
            case PlayerState.JUMPOUT:
                StateContext.Transition(m_jump_out_state);
                break;
            
            case PlayerState.ATTACK:
                StateContext.Transition(m_attack_state);
                break;
            
            case PlayerState.BLOCK:
                StateContext.Transition(m_block_state);
                break;
            
            case PlayerState.BLOCKDAMAGE:
                StateContext.Transition(m_block_damage_state);
                break;

            case PlayerState.DAMAGE:
                StateContext.Transition(m_damage_state);
                break;
        }
    }

    private void CheckGround()
    {
        RaycastHit[] hit_infos = new RaycastHit[4];
        Vector3[] ray_directions = new Vector3[4] 
        { 
            (Quaternion.Euler(45, 0, 0) * new Vector3(0, -transform.position.y, 0)).normalized,
            (Quaternion.Euler(-45, 0, 0) * new Vector3(0, -transform.position.y, 0)).normalized,
            (Quaternion.Euler(0, 0, 45) * new Vector3(0, -transform.position.y, 0)).normalized,
            (Quaternion.Euler(0, 0, -45) * new Vector3(0, -transform.position.y, 0)).normalized 
        };

        for(int i = 0; i < 4; i++)
        {
            
            if(Physics.Raycast(transform.position + Vector3.up * 0.2f, ray_directions[i], out hit_infos[i], 1.3f))
            {
                Debug.DrawRay(transform.position + Vector3.up * 0.2f, ray_directions[i], Color.green);
            }
            else
            {
                Debug.DrawRay(transform.position + Vector3.up * 0.2f, ray_directions[i], Color.red);
            }
        }

        IsGround = false;
        for(int i = 0; i < 4; i++)
        {
            if(hit_infos[i].collider != null && !hit_infos[i].collider.CompareTag("Player"))
            {
                IsGround = true;                
            }
        }

        // if(Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit_info, 1f))
        // {
        //     if(hit_info.collider != null && !hit_info.collider.CompareTag("Player"))
        //     {
        //         Debug.DrawRay(transform.position + Vector3.up * 0.2f, Vector3.down * 1f, Color.green);
        //         IsGround = true;
        //     }

        // }
        // else
        // {
        //     Debug.DrawRay(transform.position + Vector3.up * 0.2f, Vector3.down * 1f, Color.red);
        //     IsGround = false;
        // }
    }

    private void CheckFalling()
    {
        if(IsGround)
        {
            FallTime = 0f;
        }
        else
        {
            FallTime += Time.deltaTime;
        }
    }

    private void CheckBlocking()
    {
        if(IsBlock)
        {
            BlockTime += Time.deltaTime;
        }
        else
        {
            BlockTime = 0f;
        }
    }

    private void CheckAttackDelay()
    {
        if(AttackDelay <= AttackSpeed)
        {
            AttackDelay += Time.deltaTime;
        }
        

        AttackReady = AttackDelay >= AttackSpeed;
    }
}
