
namespace Hoard
{
    public class Result
    {
        public Result()
        {
            Success = true;
            Error = "";
        }
        public Result(string error)
        {
            Success = false;
            Error = error;
        }

        public bool Success { get; private set; }
        public string Error { get; private set; }
    }
}
