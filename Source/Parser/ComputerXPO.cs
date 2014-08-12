using DevExpress.Xpo;

namespace StatisticApp
{
    public class ComputerXPO : XPObject
    {
        private ClientXPO client;
        private string machine;

        public ComputerXPO( Session session ) : base( session )
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public string Machine
        {
            get { return machine; }
            set { SetPropertyValue( "Machine", ref machine, value ); }
        }

        [Association( "Client-Computers" )]
        public ClientXPO Client
        {
            get { return client; }
            set { SetPropertyValue( "Client", ref client, value ); }
        }
    }
}