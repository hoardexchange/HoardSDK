
namespace Hoard
{
    public class DataResult
    {
        public DataResult()
        {
            Success = true;
            Error = "";
        }
        public DataResult(string error)
        {
            Success = false;
            Error = error;
        }

        public bool Success { get; private set; }
        public string Error { get; private set; }
    }
}
