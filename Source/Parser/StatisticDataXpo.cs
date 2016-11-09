using System;
using DevExpress.Xpo;

namespace StatisticApp
{
    public sealed class StatisticDataXpo : XPObject
    {
        private int count;
        private string description;
        private DateTime lastTime;
        private string name;
        private ClientXPO client;

        public StatisticDataXpo(Session session) : base(session)
        {
        }

        [Association( "Client-StatisticData" )]
        public ClientXPO Client
        {
            get { return client; }
            set { SetPropertyValue("Client", ref client, value); }
        }

        public DateTime LastTime
        {
            get { return lastTime; }
            set { SetPropertyValue("LastTime", ref lastTime, value); }
        }

        public int Count
        {
            get { return count; }
            set { SetPropertyValue("Count", ref count, value); }
        }

        public string Name
        {
            get { return name; }
            set { SetPropertyValue("Name", ref name, value); }
        }

        [DbType("nvarchar(4000)")]
        public string Description
        {
            get { return description; }
            set { SetPropertyValue("Description", ref description, value); }
        }
    }
}