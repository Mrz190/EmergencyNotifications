namespace API.Middleware
{
    public class ExceptionApi
    {
        public ExceptionApi(int statusCode, string message = null, string detail = null)
        {
            StatusCode = statusCode;
            Message = message ?? "An error occurred.";
            Detail = detail;
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Detail { get; set; }
    }
}
