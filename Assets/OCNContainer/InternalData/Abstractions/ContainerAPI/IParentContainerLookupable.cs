namespace OCNContainer.InternalData
{
    public interface IParentContainerLookupable
    {
        public bool TryFindRegistration<T>(out T foundObject) where T : class;
    }
}