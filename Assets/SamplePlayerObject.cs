using UnityEngine;

public class SamplePlayerObject : MigrationBehaviour
{
    public override void Migrated()
    {
        Runner.SetPlayerAlwaysInterested(Object.InputAuthority, Object, true);

        DontDestroyOnLoad(this);
        Runner.SetPlayerObject(Object.InputAuthority, Object);

        GetComponent<FusionPlayerInput>().enabled = false;
    }
}
