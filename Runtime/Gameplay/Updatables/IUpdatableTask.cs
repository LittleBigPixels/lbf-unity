namespace LBF.Gameplay.Updatables {
    public interface IUpdatableTask<T> : IUpdatable
    {
        bool Complete { get; }
        T Target { get; set; }
    }
}
