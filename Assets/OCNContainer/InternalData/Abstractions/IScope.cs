namespace OCNContainer
{
    public interface IScope
    {
        public T Resolve<T>() where T : class;
    }
}