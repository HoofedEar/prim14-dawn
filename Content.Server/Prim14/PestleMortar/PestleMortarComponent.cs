namespace Content.Server.Prim14.PestleMortar;

[RegisterComponent]
[Access(typeof(PestleMortarSystem))]
public sealed partial class PestleMortarComponent : Component
{

}


public static class SharedPestleMortar
{
    public static readonly string InputContainerId = "inputContainer";
}
