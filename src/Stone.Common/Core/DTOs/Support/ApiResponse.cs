namespace Stone.Common.Core.DTOs.Support
{
    public class ApiResponse
    {
        public bool Success { get; set; }

        public ResponseResult ResponseResult { get; set; }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }
}
