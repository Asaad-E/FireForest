namespace FireForest;

public interface IScreen
{
    void Update(float deltaTime);
    void Draw();
    void Close();
    bool ShouldClose {get;}
    IScreen? NextScreen {get;}
}