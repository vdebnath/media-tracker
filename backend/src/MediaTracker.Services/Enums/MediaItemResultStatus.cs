namespace MediaTracker.Services.Enums
{
    public enum MediaItemResultStatus
    {
        Ok = 200,
        CreatedAtAction = 201,
        NoContent = 204,
        NotFound = 404,
        ValidationFailed = 400,
        InvalidData = 400
    }
}