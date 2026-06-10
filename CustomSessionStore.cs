using Microsoft.AspNetCore.Authentication;
using NuGet.Protocol.Plugins;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Monitoring.Store
{
    public class MySessionStore : ITicketStore
    {
        private ConcurrentDictionary<string, AuthenticationTicket> mytickets = new();

        public MySessionStore()
        {
        }

        public Task RemoveAsync(string key)
        {
            //Log.Debug("MySessionStore.RemoveAsync Key=" + key);

            if (mytickets.ContainsKey(key))
            {
                mytickets.TryRemove(key, out _);
            }

            return Task.FromResult(0);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            //Log.Debug("MySessionStore.RenewAsync Key=" + key + ", ticket = " + ticket.AuthenticationScheme);


            mytickets[key] = ticket;

            return Task.FromResult(false);
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            //Log.Debug("MySessionStore.RetrieveAsync Key=" + key);

            if (mytickets.ContainsKey(key))
            {
                var ticket = mytickets[key];
                return Task.FromResult(ticket);
            }
            else
            {
                return Task.FromResult((AuthenticationTicket)null!);
            }
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = Guid.NewGuid().ToString();
            var result = mytickets.TryAdd(key, ticket);

            if (result)
            {
                string username = ticket?.Principal?.Identity?.Name ?? "Unknown";
                //Log.Debug("MySessionStore.StoreAsync ticket=" + username + ", key=" + key);

                return Task.FromResult(key);
            }
            else
            {
                throw new Exception("Failed to add entry to the MySessionStore.");
            }
        }
    }
}
