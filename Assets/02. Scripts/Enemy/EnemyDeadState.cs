using UnityEngine;

namespace Junyoung
{
    public class EnemyDeadState : MonoBehaviour , IEnemyState<EnemyCtrl>
    {
        private EnemyCtrl m_enemy_ctrl;
        public void OnStateEnter(EnemyCtrl sender)
        {
            m_enemy_ctrl = sender;
            m_enemy_ctrl.Animator.SetTrigger("Dead");
            Invoke("Destroy", 4f);
        }
        public void OnStateUpdate(EnemyCtrl sender)
        {

        }
        public void OnStateExit(EnemyCtrl sender)
        {
        
        }

        private void Destroy()
        {
            //오브젝트 반환
        }
    }
}