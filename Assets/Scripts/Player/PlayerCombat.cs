using UnityEngine;
using StarterAssets;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private StarterAssetsInputs input;
    private StarterAssets.ThirdPersonController controller;
    private PlayerCombatState combatState;

    [Header("Combo")]
    [SerializeField] private int maxCombo = 3;
    [SerializeField] private float comboBufferTime = 0.6f; // 다음 입력을 인정해주는 시간


    private int _comboIndex = 0;          // 1~maxCombo
    private bool _canAcceptCombo = false; // 애니메이션이 “지금 눌러도 됨” 윈도우를 열어줌
    private bool _queuedCombo = false;    // 사용자가 윈도우 안에 눌렀는지
    private float _bufferTimer = 0f;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DoComboHash = Animator.StringToHash("DoCombo");
    private static readonly int ComboIndexHash = Animator.StringToHash("Comboindex");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        input = GetComponentInParent<StarterAssetsInputs>();
        combatState = GetComponentInParent<PlayerCombatState>();
        controller = GetComponentInParent<StarterAssets.ThirdPersonController>();
    }

    private void Update()
    {
        if (input == null || controller._isRolling || controller._speed >= 5f) return;

        //콤보를 이어가는 버퍼타이머 감소
        if (_bufferTimer > 0f)
            _bufferTimer -= Time.deltaTime;
        else
            _queuedCombo = false;

        if (input.attack)
        { 
            // 전투 상태 진입/유지
            combatState?.EnterCombat();
            // 버튼 1회 처리
            input.attack = false;

            // 1타 시작
            if (_comboIndex == 0)
            {
                StartCombo();
                return;
            }

            // 콤보 입력(2타/3타) 예약
            QueueCombo();
        }
    }
    private void StartCombo()
    {
        _comboIndex = 1;
        animator.SetInteger(ComboIndexHash, _comboIndex);
        animator.SetTrigger(AttackHash);

        _queuedCombo = false;
        _canAcceptCombo = false;
        _bufferTimer = 0f;
    }

    private void QueueCombo()
    {
        _queuedCombo = true;
        _bufferTimer = comboBufferTime;

        // 윈도우가 열려있으면 즉시 다음 타로 넘김
        if (_canAcceptCombo)
            ConsumeComboIfPossible();
    }
    private void ConsumeComboIfPossible()
    {
        if (!_queuedCombo) return;
        if (_comboIndex >= maxCombo) return;

        _comboIndex++;
        animator.SetInteger(ComboIndexHash, _comboIndex);
        animator.SetTrigger(DoComboHash);

        _queuedCombo = false;
        _bufferTimer = 0f;
        _canAcceptCombo = false; // 다음 윈도우는 다음 공격에서 다시 열림
    }

    // ===== Animation Events에서 호출 =====
    // 공격 애니메이션 클립 중간(다음 타 허용 타이밍)에 이벤트로 호출
    public void ComboWindowOpen()
    {
        _canAcceptCombo = true;

        // 이미 눌러둔 게 있으면 즉시 소비
        ConsumeComboIfPossible();
    }

    // 공격 애니메이션 클립이 끝나기 직전에 이벤트로 호출(또는 상태 종료 시)
    public void ComboWindowClose()
    {
        _canAcceptCombo = false;
    }

    // 콤보가 완전히 끝났을 때(마지막 타 끝 / Idle 복귀 시점)에 이벤트로 호출
    public void ComboReset()
    {
        _comboIndex = 0;
        animator.SetInteger(ComboIndexHash, 0);
        _queuedCombo = false;
        _canAcceptCombo = false;
        _bufferTimer = 0f;
    }

    //강제로 Comboindex를 0으로 만들기 - StateMachineBehavior에 사용하였음
    public void ForceComboReset() 
    {
        _comboIndex = 0;
        animator.SetInteger("Comboindex", 0);

        _queuedCombo = false;
        _canAcceptCombo = false;
        _bufferTimer = 0f;
    }

}
