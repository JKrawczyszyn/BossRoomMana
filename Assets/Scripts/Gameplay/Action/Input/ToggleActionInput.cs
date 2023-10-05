namespace Unity.BossRoom.Gameplay.Actions
{
    public class ToggleActionInput : BaseActionInput
    {
        bool m_IsOn;

        void Update()
        {
            if (!m_IsOn)
                return;

            var data = new ActionRequestData
            {
                Position = transform.position,
                ActionID = m_ActionPrototypeID,
                ShouldQueue = true,
                TargetIds = null
            };
            m_SendInput(data);
        }

        public override void OnReleaseKey()
        {
            if (!m_IsOn)
            {
                m_IsOn = true;
                
                return;
            }

            Destroy(gameObject);
        }
    }
}
