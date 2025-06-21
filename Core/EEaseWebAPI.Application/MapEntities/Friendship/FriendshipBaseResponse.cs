using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.MapEntities.Friendship
{
    public abstract class FriendshipBaseResponse<TBody> where TBody : class
    {
        public virtual EEaseWebAPI.Application.MapEntities.Header Header { get; set; }
        public virtual TBody Body { get; set; }

        protected FriendshipBaseResponse()
        {
            Header = new EEaseWebAPI.Application.MapEntities.Header();
        }
    }
} 