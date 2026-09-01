using MediaTracker.Services.Enums;

namespace MediaTracker.Services.Results
{
    public class Result<T>
    {
        public MediaItemResultStatus Status {get; set;}
        public T? Data {get; set;}
        public string? Message {get; set;}

        public Result() {}

        public Result(MediaItemResultStatus status, T? data = default, string? message = null)
        {
            Status = status;
            Data = data;
            Message = message;
        }
    }
}