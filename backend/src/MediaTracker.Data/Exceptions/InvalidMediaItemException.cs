namespace MediaTracker.Data.Exceptions
{
    public class InvalidMediaItemException : Exception
    {
        public InvalidMediaItemException(string message) : base (message) {}
    }
}