namespace robot_controller_api.Services.Robot;

public class RobotDomainException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }
    public IDictionary<string, object?> Extensions { get; }

    public RobotDomainException(int statusCode, string title, string detail)
        : base(detail)
    {
        StatusCode = statusCode;
        Title = title;
        Extensions = new Dictionary<string, object?>();
    }

    public RobotDomainException WithExtension(string key, object? value)
    {
        Extensions[key] = value;
        return this;
    }
}
