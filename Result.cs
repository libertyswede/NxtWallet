namespace NxtWallet
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public T Value { get; set; }

        public Result()
        {
        }

        public Result(T value, bool success = true)
        {
            Success = success;
            Value = value;
        }
    }
}