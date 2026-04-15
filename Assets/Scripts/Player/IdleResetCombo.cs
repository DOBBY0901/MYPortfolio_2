using StarterAssets;
using UnityEngine;

public class IdleResetCombo : StateMachineBehaviour
{
  
    public override void OnStateEnter(Animator animator,AnimatorStateInfo stateInfo,int layerIndex)
    {
        
        var combat = animator.GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.ForceComboReset();
        }
    }
}
