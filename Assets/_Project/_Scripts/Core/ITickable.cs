using HairvestMoon.Core;

public interface ITickable
{
    void Tick(GameTimeChangedArgs args);
}