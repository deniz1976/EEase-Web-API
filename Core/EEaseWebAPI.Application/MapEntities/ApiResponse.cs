namespace EEaseWebAPI.Application.MapEntities
{
    public abstract class ApiResponse<TBody>
        where TBody : class
    {
        public Header? Header { get; set; }

        public TBody? Body { get; set; }
    }
}
