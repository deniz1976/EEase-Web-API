namespace EEaseWebAPI.Application.MapEntities
{
    /// <summary>
    /// Every endpoint answers with the same two parts: a header saying how it went, and a
    /// body carrying what was asked for. Endpoints used to invent their own shape, so a
    /// caller reading one of them had to know whether the payload sat under "login",
    /// "response", "userInfo" or straight at the top.
    /// </summary>
    public abstract class ApiResponse<TBody>
        where TBody : class
    {
        public Header? Header { get; set; }

        public TBody? Body { get; set; }
    }
}
