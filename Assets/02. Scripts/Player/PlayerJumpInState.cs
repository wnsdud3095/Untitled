using UnityEngine;

public class PlayerJumpInState : MonoBehaviour, IState<PlayerCtrl>
{
    private PlayerCtrl m_player_ctrl;

    public void ExecuteEnter(PlayerCtrl sender)
    {
        m_player_ctrl = sender;
        if(m_player_ctrl)
        {
            m_player_ctrl.Animator.SetTrigger("JumpIn");
        }  
    }

    public void Execute(PlayerCtrl sender)
    {
        m_player_ctrl.Move(5f);

        if(m_player_ctrl.FallTime > 0.3f)
        {
            Debug.Log("호출");
            m_player_ctrl.ChangeState(PlayerState.JUMPING);
        }
    }

    public void ExecuteExit(PlayerCtrl sender)
    {
        m_player_ctrl.Animator.ResetTrigger("JumpIn");
    }
}