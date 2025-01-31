using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStat", menuName = "Scriptable Objects/EnemyStat")]
public class EnemyStat : ScriptableObject
{
    [SerializeField]
    private float m_hp;
    public float HP { get { return m_hp; } set { m_hp = value; } }
    
    [SerializeField]
    private float m_move_speed;
    public float MoveSpeed { get { return m_move_speed; } set { m_move_speed = value;} }

    [SerializeField]
    private float m_atk_damage;
    public float AtkDamege { get { return m_atk_damage; } set { m_atk_damage = value; } }

    [SerializeField]
    private float m_atk_rate;
    public float AtkRate { get { return m_atk_rate; } set { m_atk_rate = value;} }

    [SerializeField]
    private float m_atk_range;
    public float AtkRange { get { return m_atk_range; } set { m_atk_range = value;} }

    [SerializeField]
    private float m_atk_ani_length; 
    public float AtkAniLength { get { return m_atk_ani_length;} set {m_atk_ani_length= value;} }

    [SerializeField]
    private float m_atk_ani_speed;
    public float AtkAniSpeed { get { return m_atk_ani_speed;} set {m_atk_ani_speed= value;} }
}
